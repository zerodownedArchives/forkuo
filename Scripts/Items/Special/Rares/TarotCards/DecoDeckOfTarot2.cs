using System;

namespace Server.Items
{
    public class DecoDeckOfTarot2 : Item
    {
        [Constructable]
        public DecoDeckOfTarot2() : base(0x12Ac)
        {
            this.Movable = true;
            this.Stackable = false;
        }

        public DecoDeckOfTarot2(Serial serial) : base(serial)
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