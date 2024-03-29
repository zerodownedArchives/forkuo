using System;

namespace Server.Items
{
    public class DecoGarlic : Item
    {
        [Constructable]
        public DecoGarlic() : base(0x18E1)
        {
            this.Movable = true;
            this.Stackable = false;
        }

        public DecoGarlic(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}