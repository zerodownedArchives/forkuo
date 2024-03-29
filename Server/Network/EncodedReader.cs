/***************************************************************************
*                              EncodedReader.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: EncodedReader.cs 4 2006-06-15 04:28:39Z mark $
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

namespace Server.Network
{
    public class EncodedReader
    {
        private readonly PacketReader m_Reader;

        public EncodedReader(PacketReader reader)
        {
            this.m_Reader = reader;
        }

        public byte[] Buffer
        {
            get
            {
                return this.m_Reader.Buffer;
            }
        }

        public void Trace(NetState state)
        {
            this.m_Reader.Trace(state);
        }

        public int ReadInt32()
        {
            if (this.m_Reader.ReadByte() != 0)
                return 0;

            return this.m_Reader.ReadInt32();
        }

        public Point3D ReadPoint3D()
        {
            if (this.m_Reader.ReadByte() != 3)
                return Point3D.Zero;

            return new Point3D(this.m_Reader.ReadInt16(), this.m_Reader.ReadInt16(), this.m_Reader.ReadByte());
        }

        public string ReadUnicodeStringSafe()
        {
            if (this.m_Reader.ReadByte() != 2)
                return "";

            int length = this.m_Reader.ReadUInt16();

            return this.m_Reader.ReadUnicodeStringSafe(length);
        }

        public string ReadUnicodeString()
        {
            if (this.m_Reader.ReadByte() != 2)
                return "";

            int length = this.m_Reader.ReadUInt16();

            return this.m_Reader.ReadUnicodeString(length);
        }
    }
}