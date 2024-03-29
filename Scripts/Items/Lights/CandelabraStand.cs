using System;

namespace Server.Items
{
    public class CandelabraStand : BaseLight
    {
        public override int LitItemID
        {
            get
            {
                return 0xB26;
            }
        }
        public override int UnlitItemID
        {
            get
            {
                return 0xA29;
            }
        }

        [Constructable]
        public CandelabraStand() : base(0xA29)
        {
            this.Duration = TimeSpan.Zero; // Never burnt out
            this.Burning = false;
            this.Light = LightType.Circle225;
            this.Weight = 20.0;
        }

        public CandelabraStand(Serial serial) : base(serial)
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