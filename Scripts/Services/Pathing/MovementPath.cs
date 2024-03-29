using System;
using Server.Commands;
using Server.PathAlgorithms;
using Server.PathAlgorithms.FastAStar;
using Server.PathAlgorithms.SlowAStar;
using Server.Targeting;

namespace Server
{
    public sealed class MovementPath
    {
        private readonly Map m_Map;
        private readonly Point3D m_Start;
        private readonly Point3D m_Goal;
        private readonly Direction[] m_Directions;

        public Map Map
        {
            get
            {
                return this.m_Map;
            }
        }
        public Point3D Start
        {
            get
            {
                return this.m_Start;
            }
        }
        public Point3D Goal
        {
            get
            {
                return this.m_Goal;
            }
        }
        public Direction[] Directions
        {
            get
            {
                return this.m_Directions;
            }
        }
        public bool Success
        {
            get
            {
                return (this.m_Directions != null && this.m_Directions.Length > 0);
            }
        }

        public static void Initialize()
        {
            CommandSystem.Register("Path", AccessLevel.GameMaster, new CommandEventHandler(Path_OnCommand));
        }

        public static void Path_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, true, TargetFlags.None, new TargetCallback(Path_OnTarget));
            e.Mobile.SendMessage("Target a location and a path will be drawn there.");
        }

        private static void Path(Mobile from, IPoint3D p, PathAlgorithm alg, string name, int zOffset)
        {
            m_OverrideAlgorithm = alg;

            long start = DateTime.Now.Ticks;
            MovementPath path = new MovementPath(from, new Point3D(p));
            long end = DateTime.Now.Ticks;
            double len = Math.Round((end - start) / 10000.0, 2);

            if (!path.Success)
            {
                from.SendMessage("{0} path failed: {1}ms", name, len);
            }
            else
            {
                from.SendMessage("{0} path success: {1}ms", name, len);

                int x = from.X;
                int y = from.Y;
                int z = from.Z;

                for (int i = 0; i < path.Directions.Length; ++i)
                {
                    Movement.Movement.Offset(path.Directions[i], ref x, ref y);

                    new Items.RecallRune().MoveToWorld(new Point3D(x, y, z + zOffset), from.Map);
                }
            }
        }

        public static void Path_OnTarget(Mobile from, object obj)
        {
            IPoint3D p = obj as IPoint3D;

            if (p == null)
                return;

            Spells.SpellHelper.GetSurfaceTop(ref p);

            Path(from, p, FastAStarAlgorithm.Instance, "Fast", 0);
            Path(from, p, SlowAStarAlgorithm.Instance, "Slow", 2);
            m_OverrideAlgorithm = null;
            /*MovementPath path = new MovementPath( from, new Point3D( p ) );
            if ( !path.Success )
            {
            from.SendMessage( "No path to there could be found." );
            }
            else
            {
            //for ( int i = 0; i < path.Directions.Length; ++i )
            //	Timer.DelayCall( TimeSpan.FromSeconds( 0.1 + (i * 0.3) ), new TimerStateCallback( Pathfind ), new object[]{ from, path.Directions[i] } );
            int x = from.X;
            int y = from.Y;
            int z = from.Z;
            for ( int i = 0; i < path.Directions.Length; ++i )
            {
            Movement.Movement.Offset( path.Directions[i], ref x, ref y );
            new Items.RecallRune().MoveToWorld( new Point3D( x, y, z ), from.Map );
            }
            }*/
        }

        public static void Pathfind(object state)
        {
            object[] states = (object[])state;
            Mobile from = (Mobile)states[0];
            Direction d = (Direction)states[1];

            try
            {
                from.Direction = d;
                from.NetState.BlockAllPackets = true;
                from.Move(d);
                from.NetState.BlockAllPackets = false;
                from.ProcessDelta();
            }
            catch
            {
            }
        }

        private static PathAlgorithm m_OverrideAlgorithm;

        public static PathAlgorithm OverrideAlgorithm
        {
            get
            {
                return m_OverrideAlgorithm;
            }
            set
            {
                m_OverrideAlgorithm = value;
            }
        }

        public MovementPath(Mobile m, Point3D goal)
        {
            Point3D start = m.Location;
            Map map = m.Map;

            this.m_Map = map;
            this.m_Start = start;
            this.m_Goal = goal;

            if (map == null || map == Map.Internal)
                return;

            if (Utility.InRange(start, goal, 1))
                return;

            try
            {
                PathAlgorithm alg = m_OverrideAlgorithm;

                if (alg == null)
                {
                    alg = FastAStarAlgorithm.Instance;
                    //if ( !alg.CheckCondition( m, map, start, goal ) )	// SlowAstar is still broken
                    //	alg = SlowAStarAlgorithm.Instance;		// TODO: Fix SlowAstar
                }

                if (alg != null && alg.CheckCondition(m, map, start, goal))
                    this.m_Directions = alg.Find(m, map, start, goal);
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning: {0}: Pathing error from {1} to {2}", e.GetType().Name, start, goal);
            }
        }
    }
}