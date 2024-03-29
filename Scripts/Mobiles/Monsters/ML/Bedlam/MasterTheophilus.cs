using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a Master Theophilus corpse")]
    public class MasterTheophilus : EvilMageLord
    {
        [Constructable]
        public MasterTheophilus()
        {
            this.IsParagon = true;

            this.Name = "Master Theophilus";
            this.Title = "the necromancer";
            this.Hue = 0;

            this.SetStr(137, 187);
            this.SetDex(253, 301);
            this.SetInt(393, 444);

            this.SetHits(663, 876);

            this.SetDamage(15, 20);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 55, 60);
            this.SetResistance(ResistanceType.Fire, 50, 58);
            this.SetResistance(ResistanceType.Cold, 50, 60);
            this.SetResistance(ResistanceType.Poison, 50, 60);
            this.SetResistance(ResistanceType.Energy, 50, 60);

            this.SetSkill(SkillName.Wrestling, 69.9, 105.3);
            this.SetSkill(SkillName.Tactics, 113.0, 117.9);
            this.SetSkill(SkillName.MagicResist, 127.0, 132.8);
            this.SetSkill(SkillName.Magery, 138.1, 143.7);
            this.SetSkill(SkillName.EvalInt, 125.6, 133.8);
            this.SetSkill(SkillName.Necromancy, 125.6, 133.8);
            this.SetSkill(SkillName.SpiritSpeak, 125.6, 133.8);
            this.SetSkill(SkillName.Meditation, 128.8, 132.9);

            this.Fame = 18000;
            this.Karma = -18000;

            this.AddItem(new Shoes(0x537));
            this.AddItem(new Robe(0x452));

            for (int i = 0; i < 2; ++i)
            {
                if (Utility.RandomBool())
                    this.PackNecroScroll(Utility.RandomMinMax(5, 9));
                else
                    this.PackScroll(4, 7);
            }

            this.PackReg(7);
            this.PackReg(7);
            this.PackReg(8);
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.UltraRich, 3);
        }

        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.ParalyzingBlow;
        }

        public override bool GivesMLMinorArtifact
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }

        public MasterTheophilus(Serial serial) : base(serial)
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