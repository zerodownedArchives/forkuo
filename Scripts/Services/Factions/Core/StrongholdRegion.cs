using System;
using Server.Mobiles;
using Server.Regions;

namespace Server.Factions
{
    public class StrongholdRegion : BaseRegion
    {
        private Faction m_Faction;

        public Faction Faction
        {
            get
            {
                return this.m_Faction;
            }
            set
            {
                this.m_Faction = value;
            }
        }

        public StrongholdRegion(Faction faction) : base(faction.Definition.FriendlyName, Faction.Facet, Region.DefaultPriority, faction.Definition.Stronghold.Area)
        {
            this.m_Faction = faction;

            this.Register();
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (!base.OnMoveInto(m, d, newLocation, oldLocation))
                return false;

            if (m.IsStaff() || this.Contains(oldLocation))
                return true;
			
            if (m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                if (pm.DuelContext != null)
                {
                    m.SendMessage("You may not enter this area while participating in a duel or a tournament.");
                    return false;
                }
            }

            return (Faction.Find(m, true, true) != null);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }
    }
}