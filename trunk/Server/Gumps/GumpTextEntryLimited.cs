/***************************************************************************
*                          GumpTextEntryLimited.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: GumpTextEntryLimited.cs 77 2006-08-27 19:36:26Z krrios $
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
using Server.Network;

namespace Server.Gumps
{
    public class GumpTextEntryLimited : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private int m_Hue;
        private int m_EntryID;
        private string m_InitialText;
        private int m_Size;

        public int X
        {
            get
            {
                return this.m_X;
            }
            set
            {
                this.Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return this.m_Y;
            }
            set
            {
                this.Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return this.m_Width;
            }
            set
            {
                this.Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return this.m_Height;
            }
            set
            {
                this.Delta(ref m_Height, value);
            }
        }

        public int Hue
        {
            get
            {
                return this.m_Hue;
            }
            set
            {
                this.Delta(ref m_Hue, value);
            }
        }

        public int EntryID
        {
            get
            {
                return this.m_EntryID;
            }
            set
            {
                this.Delta(ref m_EntryID, value);
            }
        }

        public string InitialText
        {
            get
            {
                return this.m_InitialText;
            }
            set
            {
                this.Delta(ref m_InitialText, value);
            }
        }

        public int Size
        {
            get
            {
                return this.m_Size;
            }
            set
            {
                this.Delta(ref m_Size, value);
            }
        }

        public GumpTextEntryLimited(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_Width = width;
            this.m_Height = height;
            this.m_Hue = hue;
            this.m_EntryID = entryID;
            this.m_InitialText = initialText;
            this.m_Size = size;
        }

        public override string Compile()
        {
            return String.Format("{{ textentrylimited {0} {1} {2} {3} {4} {5} {6} {7} }}", this.m_X, this.m_Y, this.m_Width, this.m_Height, this.m_Hue, this.m_EntryID, this.Parent.Intern(this.m_InitialText), this.m_Size);
        }

        private static readonly byte[] m_LayoutName = Gump.StringToBuffer("textentrylimited");

        public override void AppendTo(IGumpWriter disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(this.m_X);
            disp.AppendLayout(this.m_Y);
            disp.AppendLayout(this.m_Width);
            disp.AppendLayout(this.m_Height);
            disp.AppendLayout(this.m_Hue);
            disp.AppendLayout(this.m_EntryID);
            disp.AppendLayout(this.Parent.Intern(this.m_InitialText));
            disp.AppendLayout(this.m_Size);

            disp.TextEntries++;
        }
    }
}