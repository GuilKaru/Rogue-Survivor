using RogueSurvivor.Data;
using System;
using System.Collections.Generic;

namespace RogueSurvivor.Gameplay.AI
{
    [Serializable]
    class ExplorationData
    {
        int m_LocationsQueueSize;
        List<Location> m_LocationsQueue;  // from oldest to most recent
        int m_ZonesQueueSize;
        List<Zone> m_ZonesQueue;  // from oldest to most recent

        public ExplorationData(int locationsToRemember, int zonesToRemember)
        {
            if (locationsToRemember < 1)
                throw new ArgumentOutOfRangeException("locationsQueueSize < 1");
            if (zonesToRemember < 1)
                throw new ArgumentOutOfRangeException("zonesQueueSize < 1");

            m_LocationsQueueSize = locationsToRemember;
            m_LocationsQueue = new List<Location>(locationsToRemember);
            m_ZonesQueueSize = zonesToRemember;
            m_ZonesQueue = new List<Zone>(zonesToRemember);
        }

        public void Clear()
        {
            m_LocationsQueue.Clear();
            m_ZonesQueue.Clear();
        }

        public bool HasExplored(Location loc)
        {
            return m_LocationsQueue.Contains(loc);
        }

        public void AddExplored(Location loc)
        {
            if (m_LocationsQueue.Count >= m_LocationsQueueSize)
                m_LocationsQueue.RemoveAt(0);
            m_LocationsQueue.Add(loc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns>0 for locs not explored</returns>
        public int GetExploredAge(Location loc)
        {
            int n = m_LocationsQueue.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (m_LocationsQueue[i].Equals(loc))
                    return n - i;
            }
            return 0;
        }

        public bool HasExplored(Zone zone)
        {
            return m_ZonesQueue.Contains(zone);
        }

        /// <summary>
        /// Check if has explored all the zones. Null/empty zones are considered as explored.
        /// </summary>
        /// <param name="zones">can be null or empty</param>
        /// <returns></returns>
        public bool HasExplored(List<Zone> zones)
        {
            if (zones == null || zones.Count == 0)
                return true;
            foreach (Zone z in zones)
                if (!m_ZonesQueue.Contains(z))
                    return false;
            return true;
        }

        public void AddExplored(Zone zone)
        {
            if (m_ZonesQueue.Count >= m_ZonesQueueSize)
                m_ZonesQueue.RemoveAt(0);
            m_ZonesQueue.Add(zone);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>0 for zones not explored</returns>
        public int GetExploredAge(Zone zone)
        {
            int n = m_ZonesQueue.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                if (m_ZonesQueue[i] == zone)
                    return n - i;
            }
            return 0;
        }

        /// <summary>
        /// Get age of most recently explored from list ("youngest")
        /// </summary>
        /// <param name="zones">can be null or empty, will return 0</param>
        /// <returns></returns>
        public int GetExploredAge(List<Zone> zones)
        {
            if (zones == null)
                return 0;
            if (zones.Count == 0)
                return 0;

            int youngestAge = int.MaxValue;  // its bad hey but it works
            foreach (Zone z in zones)
            {
                int age = GetExploredAge(z);
                if (age < youngestAge)
                    youngestAge = age;
            }
            return youngestAge;
        }

        public void Update(Location location)
        {
            // location.
            if (!HasExplored(location))
                AddExplored(location);

            // zones.
            List<Zone> zones = location.Map.GetZonesAt(location.Position.X, location.Position.Y);
            if (zones != null && zones.Count > 0)
            {
                foreach (Zone z in zones)
                {
                    if (HasExplored(z))
                        continue;
                    AddExplored(z);
                }
            }
        }
    }
}
