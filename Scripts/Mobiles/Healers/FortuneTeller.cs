using System;
using Server.Items;

namespace Server.Mobiles
{
    public class FortuneTeller : BaseHealer
    {
        public override bool CanTeach
        {
            get
            {
                return true;
            }
        }

        public override bool CheckTeach(SkillName skill, Mobile from)
        {
            if (!base.CheckTeach(skill, from))
                return false;

            return (skill == SkillName.Anatomy) ||
                   (skill == SkillName.Healing) ||
                   (skill == SkillName.Forensics) ||
                   (skill == SkillName.SpiritSpeak);
        }

        [Constructable]
        public FortuneTeller()
        {
            this.Title = "the fortune teller";

            this.SetSkill(SkillName.Anatomy, 85.0, 100.0);
            this.SetSkill(SkillName.Healing, 90.0, 100.0);
            this.SetSkill(SkillName.Forensics, 75.0, 98.0);
            this.SetSkill(SkillName.SpiritSpeak, 65.0, 88.0);
        }

        public override bool IsActiveVendor
        {
            get
            {
                return true;
            }
        }
        public override bool IsInvulnerable
        {
            get
            {
                return true;
            }
        }

        public override void InitSBInfo()
        {
            this.SBInfos.Add(new SBMage());
            this.SBInfos.Add(new SBFortuneTeller());
        }

        public override int GetRobeColor()
        {
            return this.RandomBrightHue();
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            switch ( Utility.Random(3) )
            {
                case 0:
                    this.AddItem(new SkullCap(this.RandomBrightHue()));
                    break;
                case 1:
                    this.AddItem(new WizardsHat(this.RandomBrightHue()));
                    break;
                case 2:
                    this.AddItem(new Bandana(this.RandomBrightHue()));
                    break;
            }

            this.AddItem(new Spellbook());
        }

        public FortuneTeller(Serial serial) : base(serial)
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