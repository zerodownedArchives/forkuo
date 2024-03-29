using System;

namespace Server.Items
{
    public class Candelabra : BaseLight, IShipwreckedItem
    {
        public override int LitItemID
        {
            get
            {
                return 0xB1D;
            }
        }
        public override int UnlitItemID
        {
            get
            {
                return 0xA27;
            }
        }

        [Constructable]
        public Candelabra() : base(0xA27)
        {
            this.Duration = TimeSpan.Zero; // Never burnt out
            this.Burning = false;
            this.Light = LightType.Circle225;
            this.Weight = 3.0;
        }

        public Candelabra(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);

            writer.Write(this.m_IsShipwreckedItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch ( version )
            {
                case 1:
                    {
                        this.m_IsShipwreckedItem = reader.ReadBool();
                        break;
                    }
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (this.m_IsShipwreckedItem)
                list.Add(1041645); // recovered from a shipwreck
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (this.m_IsShipwreckedItem)
                this.LabelTo(from, 1041645);	//recovered from a shipwreck
        }

        #region IShipwreckedItem Members

        private bool m_IsShipwreckedItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem
        {
            get
            {
                return this.m_IsShipwreckedItem;
            }
            set
            {
                this.m_IsShipwreckedItem = value;
            }
        }
        #endregion
    }
}