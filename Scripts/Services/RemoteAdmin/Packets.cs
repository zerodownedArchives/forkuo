using System;
using System.Collections;
using Server.Accounting;
using Server.Items;
using Server.Network;

namespace Server.RemoteAdmin
{
    public enum LoginResponse : byte
    {
        NoUser = 0,
        BadIP,
        BadPass,
        NoAccess,
        OK
    }

    public sealed class AdminCompressedPacket : Packet
    {
        public AdminCompressedPacket(byte[] CompData, int CDLen, int unCompSize) : base(0x01)
        {
            this.EnsureCapacity(1 + 2 + 2 + CDLen);
            this.m_Stream.Write((ushort)unCompSize);
            this.m_Stream.Write(CompData, 0, CDLen);
        }
    }
	
    public sealed class Login : Packet
    {
        public Login(LoginResponse resp) : base(0x02, 2)
        {
            this.m_Stream.Write((byte)resp);
        }
    }

    public sealed class ConsoleData : Packet
    {
        public ConsoleData(string str) : base(0x03)
        {
            this.EnsureCapacity(1 + 2 + 1 + str.Length + 1);
            this.m_Stream.Write((byte)2);

            this.m_Stream.WriteAsciiNull(str);
        }

        public ConsoleData(char ch) : base(0x03)
        {
            this.EnsureCapacity(1 + 2 + 1 + 1);
            this.m_Stream.Write((byte)3);

            this.m_Stream.Write((byte)ch);
        }
    }

    public sealed class ServerInfo : Packet
    {
        public ServerInfo() : base(0x04)
        {
            string netVer = Environment.Version.ToString();
            string os = Environment.OSVersion.ToString();

            this.EnsureCapacity(1 + 2 + (10 * 4) + netVer.Length + 1 + os.Length + 1);
            int banned = 0;
            int active = 0;

            foreach (Account acct in Accounts.GetAccounts())
            {
                if (acct.Banned)
                    ++banned;
                else
                    ++active;
            }

            this.m_Stream.Write((int)active);
            this.m_Stream.Write((int)banned);
            this.m_Stream.Write((int)Firewall.List.Count);
            this.m_Stream.Write((int)NetState.Instances.Count);

            this.m_Stream.Write((int)World.Mobiles.Count);
            this.m_Stream.Write((int)Core.ScriptMobiles);
            this.m_Stream.Write((int)World.Items.Count);
            this.m_Stream.Write((int)Core.ScriptItems);

            this.m_Stream.Write((uint)(DateTime.Now - Clock.ServerStart).TotalSeconds);
            this.m_Stream.Write((uint)GC.GetTotalMemory(false));                        // TODO: uint not sufficient for TotalMemory (long). Fix protocol.
            this.m_Stream.WriteAsciiNull(netVer);
            this.m_Stream.WriteAsciiNull(os);
        }
    }

    public sealed class AccountSearchResults : Packet
    {
        public AccountSearchResults(ArrayList results) : base(0x05)
        {
            this.EnsureCapacity(1 + 2 + 2);

            this.m_Stream.Write((byte)results.Count);
			
            foreach (Account a in results)
            {
                this.m_Stream.WriteAsciiNull(a.Username);

                string pwToSend = a.PlainPassword;

                if (pwToSend == null)
                    pwToSend = "(hidden)";

                this.m_Stream.WriteAsciiNull(pwToSend);
                this.m_Stream.Write((byte)a.AccessLevel);
                this.m_Stream.Write(a.Banned);
                unchecked
                {
                    this.m_Stream.Write((uint)a.LastLogin.Ticks);
                }// TODO: This doesn't work, uint.MaxValue is only 7 minutes of ticks. Fix protocol.
				
                this.m_Stream.Write((ushort)a.LoginIPs.Length);
                for (int i = 0; i < a.LoginIPs.Length; i++)
                    this.m_Stream.WriteAsciiNull(a.LoginIPs[i].ToString());

                this.m_Stream.Write((ushort)a.IPRestrictions.Length);
                for (int i = 0; i < a.IPRestrictions.Length; i++)
                    this.m_Stream.WriteAsciiNull(a.IPRestrictions[i]);
            }
        }
    }

    public sealed class CompactServerInfo : Packet
    {
        public CompactServerInfo() : base(0x51)
        {
            this.EnsureCapacity(1 + 2 + (4 * 4) + 8);

            this.m_Stream.Write((int)NetState.Instances.Count - 1);                      // Clients
            this.m_Stream.Write((int)World.Items.Count);                                 // Items
            this.m_Stream.Write((int)World.Mobiles.Count);                               // Mobiles
            this.m_Stream.Write((uint)(DateTime.Now - Clock.ServerStart).TotalSeconds);  // Age (seconds)

            long memory = GC.GetTotalMemory(false);
            this.m_Stream.Write((uint)(memory >> 32));                                   // Memory high bytes
            this.m_Stream.Write((uint)memory);                                           // Memory low bytes
        }
    }

    public sealed class UOGInfo : Packet
    {
        public UOGInfo(string str) : base(0x52, str.Length + 6)// 'R'
        {
            this.m_Stream.WriteAsciiFixed("unUO", 4);
            this.m_Stream.WriteAsciiNull(str);
        }
    }

    public sealed class MessageBoxMessage : Packet
    {
        public MessageBoxMessage(string msg, string caption) : base(0x08)
        {
            this.EnsureCapacity(1 + 2 + msg.Length + 1 + caption.Length + 1);

            this.m_Stream.WriteAsciiNull(msg);
            this.m_Stream.WriteAsciiNull(caption);
        }
    }
}