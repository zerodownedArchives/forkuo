/***************************************************************************
*                               FileQueue.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: FileQueue.cs 4 2006-06-15 04:28:39Z mark $
*
***************************************************************************/

/***************************************************************************
*
*   This program is free software; you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation; either version 2 of the License, or
*   (at your option) any later version.
*
***************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using Server.Network;

namespace Server
{
    public delegate void FileCommitCallback(FileQueue.Chunk chunk);

    public sealed class FileQueue : IDisposable
    {
        public sealed class Chunk
        {
            private readonly FileQueue owner;
            private readonly int slot;

            private readonly byte[] buffer;
            private readonly int offset;
            private readonly int size;

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }

            public int Offset
            {
                get
                {
                    return 0;
                }
            }

            public int Size
            {
                get
                {
                    return this.size;
                }
            }

            public Chunk(FileQueue owner, int slot, byte[] buffer, int offset, int size)
            {
                this.owner = owner;
                this.slot = slot;

                this.buffer = buffer;
                this.offset = offset;
                this.size = size;
            }

            public void Commit()
            {
                this.owner.Commit(this, this.slot);
            }
        }

        private struct Page
        {
            public byte[] buffer;
            public int length;
        }

        private static int bufferSize;
        private static BufferPool bufferPool;

        static FileQueue()
        {
            bufferSize = FileOperations.BufferSize;
            bufferPool = new BufferPool("File Buffers", 64, bufferSize);
        }

        private readonly object syncRoot;

        private readonly Chunk[] active;
        private int activeCount;

        private readonly Queue<Page> pending;
        private readonly Page buffered;

        private readonly FileCommitCallback callback;

        private ManualResetEvent idle;

        private long position;

        public long Position
        {
            get
            {
                return this.position;
            }
        }

        public FileQueue(int concurrentWrites, FileCommitCallback callback)
        {
            if (concurrentWrites < 1)
            {
                throw new ArgumentOutOfRangeException("concurrentWrites");
            }
            else if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            else if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            this.syncRoot = new object();

            this.active = new Chunk[concurrentWrites];
            this.pending = new Queue<Page>();

            this.callback = callback;

            this.idle = new ManualResetEvent(true);
        }

        private void Append(Page page)
        {
            lock (this.syncRoot)
            {
                if (this.activeCount == 0)
                {
                    this.idle.Reset();
                }

                ++this.activeCount;

                for (int slot = 0; slot < this.active.Length; ++slot)
                {
                    if (this.active[slot] == null)
                    {
                        this.active[slot] = new Chunk(this, slot, page.buffer, 0, page.length);

                        this.callback(this.active[slot]);

                        return;
                    }
                }

                this.pending.Enqueue(page);
            }
        }

        public void Dispose()
        {
            if (this.idle != null)
            {
                this.idle.Close();
                this.idle = null;
            }
        }

        public void Flush()
        {
            if (this.buffered.buffer != null)
            {
                this.Append(buffered);

                this.buffered.buffer = null;
                this.buffered.length = 0;
            }

            /*lock ( syncRoot ) {
            if ( pending.Count > 0 ) {
            idle.Reset();
            }

            for ( int slot = 0; slot < active.Length && pending.Count > 0; ++slot ) {
            if ( active[slot] == null ) {
            Page page = pending.Dequeue();

            active[slot] = new Chunk( this, slot, page.buffer, 0, page.length );

            ++activeCount;

            callback( active[slot] );
            }
            }
            }*/

            this.idle.WaitOne();
        }

        private void Commit(Chunk chunk, int slot)
        {
            if (slot < 0 || slot >= this.active.Length)
            {
                throw new ArgumentOutOfRangeException("slot");
            }

            lock (this.syncRoot)
            {
                if (this.active[slot] != chunk)
                {
                    throw new ArgumentException();
                }

                bufferPool.ReleaseBuffer(chunk.Buffer);

                if (this.pending.Count > 0)
                {
                    Page page = this.pending.Dequeue();

                    this.active[slot] = new Chunk(this, slot, page.buffer, 0, page.length);

                    this.callback(this.active[slot]);
                }
                else
                {
                    this.active[slot] = null;
                }

                --this.activeCount;

                if (this.activeCount == 0)
                {
                    this.idle.Set();
                }
            }
        }

        public void Enqueue(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            else if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            else if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            else if ((buffer.Length - offset) < size)
            {
                throw new ArgumentException();
            }

            this.position += size;

            while (size > 0)
            {
                if (this.buffered.buffer == null)
                { // nothing yet buffered
                    this.buffered.buffer = bufferPool.AcquireBuffer();
                }

                byte[] page = this.buffered.buffer; // buffer page
                int pageSpace = page.Length - this.buffered.length; // available bytes in page
                int byteCount = (size > pageSpace ? pageSpace : size); // how many bytes we can copy over

                Buffer.BlockCopy(buffer, offset, page, this.buffered.length, byteCount);

                this.buffered.length += byteCount;
                offset += byteCount;
                size -= byteCount;

                if (this.buffered.length == page.Length)
                { // page full
                    this.Append(buffered);

                    this.buffered.buffer = null;
                    this.buffered.length = 0;
                }
            }
        }
    }
}