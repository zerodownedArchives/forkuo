using System;

namespace Server.Items
{
    public class PyramidAddon : BaseAddon
    {
        public override bool ShareHue
        {
            get
            {
                return false;
            }
        }

        [Constructable]
        public PyramidAddon()
        {
            this.AddComponent(new AddonComponent(1006), 0, 0, 5);

            for (int o = 1; o <= 2; ++o)
            {
                this.AddComponent(new AddonComponent(1011), -o, -o, (2 - o) * 5);
                this.AddComponent(new AddonComponent(1012), +o, +o, (2 - o) * 5);
                this.AddComponent(new AddonComponent(1013), +o, -o, (2 - o) * 5);
                this.AddComponent(new AddonComponent(1014), -o, +o, (2 - o) * 5);
            }

            for (int o = -1; o <= 1; ++o)
            {
                this.AddComponent(new AddonComponent(1007), o, 2, 0);
                this.AddComponent(new AddonComponent(1008), 2, o, 0);
                this.AddComponent(new AddonComponent(1009), o, -2, 0);
                this.AddComponent(new AddonComponent(1010), -2, o, 0);
            }

            this.AddComponent(new AddonComponent(1007), 0, 1, 5);
            this.AddComponent(new AddonComponent(1008), 1, 0, 5);
            this.AddComponent(new AddonComponent(1009), 0, -1, 5);
            this.AddComponent(new AddonComponent(1010), -1, 0, 5);
        }

        public PyramidAddon(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((byte)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadByte();
        }
    }
}