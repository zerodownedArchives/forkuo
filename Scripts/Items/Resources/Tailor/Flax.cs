using System;
using Server.Targeting;

namespace Server.Items
{
    public class Flax : Item
    {
        [Constructable]
        public Flax() : this(1)
        {
        }

        [Constructable]
        public Flax(int amount) : base(0x1A9C)
        {
            this.Stackable = true;
            this.Weight = 1.0;
            this.Amount = amount;
        }

        public Flax(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(502655); // What spinning wheel do you wish to spin this on?
                from.Target = new PickWheelTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        public static void OnSpun(ISpinningWheel wheel, Mobile from, int hue)
        {
            Item item = new SpoolOfThread(6);
            item.Hue = hue;

            from.AddToBackpack(item);
            from.SendLocalizedMessage(1010577); // You put the spools of thread in your backpack.
        }

        private class PickWheelTarget : Target
        {
            private readonly Flax m_Flax;

            public PickWheelTarget(Flax flax) : base(3, false, TargetFlags.None)
            {
                this.m_Flax = flax;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (this.m_Flax.Deleted)
                    return;

                ISpinningWheel wheel = targeted as ISpinningWheel;

                if (wheel == null && targeted is AddonComponent)
                    wheel = ((AddonComponent)targeted).Addon as ISpinningWheel;

                if (wheel is Item)
                {
                    Item item = (Item)wheel;

                    if (!this.m_Flax.IsChildOf(from.Backpack))
                    {
                        from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                    }
                    else if (wheel.Spinning)
                    {
                        from.SendLocalizedMessage(502656); // That spinning wheel is being used.
                    }
                    else
                    {
                        this.m_Flax.Consume();
                        wheel.BeginSpin(new SpinCallback(Flax.OnSpun), from, this.m_Flax.Hue);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(502658); // Use that on a spinning wheel.
                }
            }
        }
    }
}