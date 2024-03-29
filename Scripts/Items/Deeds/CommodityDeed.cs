using System;
using Server.Targeting;

namespace Server.Items
{
    public interface ICommodity /* added IsDeedable prop so expansion-based deedables can determine true/false */
    {
        int DescriptionNumber { get; }
        bool IsDeedable { get; }
    }

    public class CommodityDeed : Item
    {
        private Item m_Commodity;

        [CommandProperty(AccessLevel.GameMaster)]
        public Item Commodity
        {
            get
            {
                return this.m_Commodity;
            }
        }

        public bool SetCommodity(Item item)
        {
            this.InvalidateProperties();

            if (this.m_Commodity == null && item is ICommodity && ((ICommodity)item).IsDeedable)
            {
                this.m_Commodity = item;
                this.m_Commodity.Internalize();
                this.InvalidateProperties();

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(this.m_Commodity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            this.m_Commodity = reader.ReadItem();

            switch ( version )
            {
                case 0:
                    {
                        if (this.m_Commodity != null)
                        {
                            this.Hue = 0x592;
                        }
                        break;
                    }
            }
        }

        public CommodityDeed(Item commodity) : base(0x14F0)
        {
            this.Weight = 1.0;
            this.Hue = 0x47;

            this.m_Commodity = commodity;

            this.LootType = LootType.Blessed;
        }

        [Constructable]
        public CommodityDeed() : this(null)
        {
        }

        public CommodityDeed(Serial serial) : base(serial)
        {
        }

        public override void OnDelete()
        {
            if (this.m_Commodity != null)
                this.m_Commodity.Delete();

            base.OnDelete();
        }

        public override int LabelNumber
        {
            get
            {
                return this.m_Commodity == null ? 1047016 : 1047017;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (this.m_Commodity != null)
            {
                string args;

                if (this.m_Commodity.Name == null)
                    args = String.Format("#{0}\t{1}", (this.m_Commodity is ICommodity) ? ((ICommodity)this.m_Commodity).DescriptionNumber : this.m_Commodity.LabelNumber, this.m_Commodity.Amount);
                else
                    args = String.Format("{0}\t{1}", this.m_Commodity.Name, this.m_Commodity.Amount);

                list.Add(1060658, args); // ~1_val~: ~2_val~
            }
            else
            {
                list.Add(1060748); // unfilled
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (this.m_Commodity != null)
            {
                string args;

                if (this.m_Commodity.Name == null)
                    args = String.Format("#{0}\t{1}", (this.m_Commodity is ICommodity) ? ((ICommodity)this.m_Commodity).DescriptionNumber : this.m_Commodity.LabelNumber, this.m_Commodity.Amount);
                else
                    args = String.Format("{0}\t{1}", this.m_Commodity.Name, this.m_Commodity.Amount);

                this.LabelTo(from, 1060658, args); // ~1_val~: ~2_val~
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            int number;

            BankBox box = from.FindBankNoCreate();
            CommodityDeedBox cox = CommodityDeedBox.Find(this);
			
            // Veteran Rewards mods
            if (this.m_Commodity != null)
            {
                if (box != null && this.IsChildOf(box))
                {
                    number = 1047031; // The commodity has been redeemed.

                    box.DropItem(this.m_Commodity);

                    this.m_Commodity = null;
                    this.Delete();
                }
                else if (cox != null)
                {
                    if (cox.IsSecure)
                    {
                        number = 1047031; // The commodity has been redeemed.

                        cox.DropItem(this.m_Commodity);

                        this.m_Commodity = null;
                        this.Delete();
                    }
                    else
                        number = 1080525; // The commodity deed box must be secured before you can use it.
                }
                else
                {
                    if (Core.ML)
                    {
                        number = 1080526; // That must be in your bank box or commodity deed box to use it.
                    }
                    else
                    {
                        number = 1047024; // To claim the resources ....
                    }
                }
            }
            else if (cox != null && !cox.IsSecure)
            {
                number = 1080525; // The commodity deed box must be secured before you can use it.
            }
            else if ((box == null || !this.IsChildOf(box)) && cox == null)
            {
                if (Core.ML)
                {
                    number = 1080526; // That must be in your bank box or commodity deed box to use it.
                }
                else
                {
                    number = 1047026; // That must be in your bank box to use it.
                }
            }
            else
            {
                number = 1047029; // Target the commodity to fill this deed with.

                from.Target = new InternalTarget(this);
            }

            from.SendLocalizedMessage(number);
        }

        private class InternalTarget : Target
        {
            private readonly CommodityDeed m_Deed;

            public InternalTarget(CommodityDeed deed) : base(3, false, TargetFlags.None)
            {
                this.m_Deed = deed;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (this.m_Deed.Deleted)
                    return;

                int number;

                if (this.m_Deed.Commodity != null)
                {
                    number = 1047028; // The commodity deed has already been filled.
                }
                else if (targeted is Item)
                {
                    BankBox box = from.FindBankNoCreate();
                    CommodityDeedBox cox = CommodityDeedBox.Find(this.m_Deed);

                    // Veteran Rewards mods
                    if (box != null && this.m_Deed.IsChildOf(box) && ((Item)targeted).IsChildOf(box) ||
                        cox != null && cox.IsSecure && ((Item)targeted).IsChildOf(cox))
                    {
                        if (this.m_Deed.SetCommodity((Item)targeted))
                        {
                            this.m_Deed.Hue = 0x592;
                            number = 1047030; // The commodity deed has been filled.
                        }
                        else
                        {
                            number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
                        }
                    }
                    else
                    {
                        if (Core.ML)
                        {
                            number = 1080526; // That must be in your bank box or commodity deed box to use it.
                        }
                        else
                        {
                            number = 1047026; // That must be in your bank box to use it.
                        }
                    }
                }
                else
                {
                    number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
                }

                from.SendLocalizedMessage(number);
            }
        }
    }
}