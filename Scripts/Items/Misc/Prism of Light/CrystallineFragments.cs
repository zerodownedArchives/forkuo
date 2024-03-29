using System;

namespace Server.Items
{
    public class CrystallineFragments : Item
    {
        public override int LabelNumber
        {
            get
            {
                return 1073160;
            }
        }// Crystalline Fragments

        [Constructable]
        public CrystallineFragments() : base(0x223B)
        {
            this.LootType = LootType.Blessed;
            this.Hue = 0x47E;
        }

        public CrystallineFragments(Serial serial) : base(serial)
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
    }
}