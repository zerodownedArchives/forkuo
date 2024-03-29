using System;

namespace Server.Items
{
    public class MelisandesCorrodedHatchet : Hatchet
    {
        public override int LabelNumber
        {
            get
            {
                return 1072115;
            }
        }// Melisande's Corroded Hatchet

        [Constructable]
        public MelisandesCorrodedHatchet()
        {
            this.Hue = 0x494;

            this.SkillBonuses.SetValues(0, SkillName.Lumberjacking, 5.0);

            this.Attributes.SpellChanneling = 1;
            this.Attributes.WeaponSpeed = 15;
            this.Attributes.WeaponDamage = -50;

            this.WeaponAttributes.SelfRepair = 4;
        }

        public MelisandesCorrodedHatchet(Serial serial) : base(serial)
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
}