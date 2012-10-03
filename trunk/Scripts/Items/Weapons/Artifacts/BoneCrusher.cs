using System;

namespace Server.Items
{
    public class BoneCrusher : WarMace
    {
        public override int LabelNumber
        {
            get
            {
                return 1061596;
            }
        }// Bone Crusher
        public override int ArtifactRarity
        {
            get
            {
                return 11;
            }
        }

        public override int InitMinHits
        {
            get
            {
                return 255;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 255;
            }
        }

        [Constructable]
        public BoneCrusher()
        {
            this.ItemID = 0x1406;
            this.Hue = 0x60C;
            this.WeaponAttributes.HitLowerDefend = 50;
            this.Attributes.BonusStr = 10;
            this.Attributes.WeaponDamage = 75;
        }

        public BoneCrusher(Serial serial) : base(serial)
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

            if (this.Hue == 0x604)
                this.Hue = 0x60C;

            if (this.ItemID == 0x1407)
                this.ItemID = 0x1406;
        }
    }
}