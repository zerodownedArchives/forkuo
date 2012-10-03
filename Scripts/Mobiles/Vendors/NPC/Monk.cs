using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class Monk : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos
        {
            get
            {
                return this.m_SBInfos;
            }
        }
		
        [Constructable]
        public Monk() : base("the Monk")
        {
            this.SetSkill(SkillName.EvalInt, 100.0);
            this.SetSkill(SkillName.Tactics, 70.0, 90.0);
            this.SetSkill(SkillName.Wrestling, 70.0, 90.0);
            this.SetSkill(SkillName.MagicResist, 70.0, 90.0);
            this.SetSkill(SkillName.Macing, 70.0, 90.0);
        }
		
        public override void InitSBInfo()
        {
            this.m_SBInfos.Add(new SBMonk());
        }

        public override void InitOutfit()
        {
            this.AddItem(new Sandals());
            this.AddItem(new MonkRobe());
        }

        public Monk(Serial serial) : base(serial)
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