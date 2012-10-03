using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class CurtainsComponent : AddonComponent, IDyable
    {
        public override int LabelNumber
        {
            get
            {
                return 1076280;
            }
        }// Curtains
        public override bool DisplayWeight
        {
            get
            {
                return false;
            }
        }

        private int m_ClosedID;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ClosedID
        {
            get
            {
                return this.m_ClosedID;
            }
            set
            {
                this.m_ClosedID = value;
            }
        }

        public CurtainsComponent(int itemID, int closedID) : base(itemID)
        {
            this.m_ClosedID = closedID;
        }

        public CurtainsComponent(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);

            if (this.Addon != null)
            {
                if (from.InRange(this.Location, 1))
                {
                    foreach (AddonComponent c in this.Addon.Components)
                    {
                        if (c is CurtainsComponent)
                        {
                            CurtainsComponent curtain = (CurtainsComponent)c;

                            int temp = curtain.ItemID;
                            curtain.ItemID = curtain.ClosedID;
                            curtain.ClosedID = temp;
                        }
                    }
                }
                else
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((int)this.m_ClosedID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            this.m_ClosedID = reader.ReadInt();
        }

        #region IDyable
        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (this.Deleted)
                return false;

            this.Hue = sender.DyedHue;
            return true;
        }
        #endregion
    }

    public class CurtainsAddon : BaseAddon
    {
        public override BaseAddonDeed Deed
        {
            get
            {
                return new CurtainsDeed();
            }
        }
        public override bool RetainDeedHue
        {
            get
            {
                return true;
            }
        }

        [Constructable]
        public CurtainsAddon(bool east) : base()
        {
            if (east) // east
            {
                this.AddComponent(new CurtainsComponent(0x3D9E, 0x3DA8), 0, -1, 0);
                this.AddComponent(new CurtainsComponent(0x3DAC, 0x3DAE), 0, 0, 0);
                this.AddComponent(new CurtainsComponent(0x3DA0, 0x3DA6), 0, 2, 0);
                this.AddComponent(new CurtainsComponent(0x3D9F, 0x3DA7), 0, 1, 0);
            }
            else // south
            {
                this.AddComponent(new CurtainsComponent(0x3D9C, 0x3DAD), 0, 0, 0);
                this.AddComponent(new CurtainsComponent(0x3D9D, 0x3DA3), -1, 0, 0);
                this.AddComponent(new CurtainsComponent(0x3DA1, 0x3DA5), 2, 0, 0);
                this.AddComponent(new CurtainsComponent(0x3DAB, 0x3DA4), 1, 0, 0);
            }
        }

        public CurtainsAddon(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class CurtainsDeed : BaseAddonDeed
    {
        public override BaseAddon Addon
        {
            get
            {
                return new CurtainsAddon(this.m_East);
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1076280;
            }
        }// Curtains

        private bool m_East;

        [Constructable]
        public CurtainsDeed() : base()
        {
            this.LootType = LootType.Blessed;
        }

        public CurtainsDeed(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this.IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(InternalGump));
                from.SendGump(new InternalGump(this));
            }
            else
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
        }

        private void SendTarget(Mobile m)
        {
            base.OnDoubleClick(m);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }

        private class InternalGump : Gump
        {
            private readonly CurtainsDeed m_Deed;

            public InternalGump(CurtainsDeed deed) : base(60, 36)
            {
                this.m_Deed = deed;

                this.AddPage(0);

                this.AddBackground(0, 0, 273, 324, 0x13BE);
                this.AddImageTiled(10, 10, 253, 20, 0xA40);
                this.AddImageTiled(10, 40, 253, 244, 0xA40);
                this.AddImageTiled(10, 294, 253, 20, 0xA40);
                this.AddAlphaRegion(10, 10, 253, 304);
                this.AddButton(10, 294, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                this.AddHtmlLocalized(45, 296, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL
                this.AddHtmlLocalized(14, 12, 273, 20, 1076581, 0x7FFF, false, false); // Please select your curtain position

                this.AddPage(1);

                this.AddButton(19, 49, 0x845, 0x846, 1, GumpButtonType.Reply, 0);
                this.AddHtmlLocalized(44, 47, 213, 20, 1075386, 0x7FFF, false, false); // South
                this.AddButton(19, 73, 0x845, 0x846, 2, GumpButtonType.Reply, 0);
                this.AddHtmlLocalized(44, 71, 213, 20, 1075387, 0x7FFF, false, false); // East
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (this.m_Deed == null || this.m_Deed.Deleted || info.ButtonID == 0)
                    return;

                this.m_Deed.m_East = (info.ButtonID != 1);
                this.m_Deed.SendTarget(sender.Mobile);
            }
        }
    }
}