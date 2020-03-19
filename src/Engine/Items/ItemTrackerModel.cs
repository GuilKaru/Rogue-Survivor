using RogueSurvivor.Data;
using System;

namespace RogueSurvivor.Engine.Items
{
    class ItemTrackerModel : ItemModel
    {
        [Flags]
        public enum TrackingFlags
        {
            /// <summary>
            /// Followers and Leaders can track each other.
            /// </summary>
            FOLLOWER_AND_LEADER = (1 << 0),

            /// <summary>
            /// Can track undeads within close range.
            /// </summary>
            UNDEADS = (1 << 1),

            /// <summary>
            /// Can track all BlackOps faction members on the map.
            /// </summary>
            BLACKOPS_FACTION = (1 << 2),

            /// <summary>
            /// Can track all Police faction members on the map.
            /// </summary>
            POLICE_FACTION = (1 << 3)
        }

        TrackingFlags m_Tracking;
        int m_MaxBatteries;
        bool m_HasClock;

        public TrackingFlags Tracking
        {
            get { return m_Tracking; }
        }

        public int MaxBatteries
        {
            get { return m_MaxBatteries; }
        }

        public bool HasClock
        {
            get { return m_HasClock; }
        }

        public ItemTrackerModel(string aName, string theNames, string imageID, TrackingFlags tracking, int maxBatteries, bool hasClock)
            : base(aName, theNames, imageID)
        {
            m_Tracking = tracking;
            m_MaxBatteries = maxBatteries;
            m_HasClock = hasClock;
            this.DontAutoEquip = true;
        }
    }
}
