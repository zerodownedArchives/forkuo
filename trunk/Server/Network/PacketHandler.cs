using System;

namespace Server.Network
{
	public delegate void OnPacketReceive( NetState state, PacketReader pvSrc );
	public delegate bool ThrottlePacketCallback( NetState state );

	public class PacketHandler
	{
		private int m_PacketID;
		private int m_Length;
		private bool m_Ingame;
		private OnPacketReceive m_OnReceive;
		private ThrottlePacketCallback m_ThrottleCallback;

		public PacketHandler( int packetID, int length, bool ingame, OnPacketReceive onReceive )
		{
			m_PacketID = packetID;
			m_Length = length;
			m_Ingame = ingame;
			m_OnReceive = onReceive;
		}

		public int PacketID
		{
			get
			{
				return m_PacketID;
			}
		}

		public int Length
		{
			get
			{
				return m_Length;
			}
		}

		public OnPacketReceive OnReceive
		{
			get
			{
				return m_OnReceive;
			}
		}

		public ThrottlePacketCallback ThrottleCallback
		{
			get{ return m_ThrottleCallback; }
			set{ m_ThrottleCallback = value; }
		}

		public bool Ingame
		{
			get
			{
				return m_Ingame;
			}
		}
	}
}