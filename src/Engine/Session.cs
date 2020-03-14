using RogueSurvivor.Data;
using RogueSurvivor.Gameplay;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#if DEBUG_STATS
using System.Collections.Generic;
#endif

namespace RogueSurvivor.Engine
{
    [Serializable]
    enum GameMode
    {
        GM_STANDARD,
        GM_CORPSES_INFECTION,
        GM_VINTAGE
    }

    [Serializable]
    enum ScriptStage
    {
        STAGE_0,
        STAGE_1,
        STAGE_2,
        STAGE_3,
        STAGE_4,
        STAGE_5
    }

    [Serializable]
    enum RaidType
    {
        _FIRST = 0,

        BIKERS = _FIRST,
        GANGSTA,
        BLACKOPS,
        SURVIVORS,

        /// <summary>
        /// "Fake" raid for AIs.
        /// </summary>
        NATGUARD,

        /// <summary>
        /// "Fake" raid for AIs.
        /// </summary>
        ARMY_SUPLLIES,

        _COUNT
    }

    class CharGen
    {
        public bool IsUndead { get; set; }
        public GameActors.IDs UndeadModel { get; set; }
        public bool IsMale { get; set; }
        public Skills.IDs StartingSkill { get; set; }
    }

    [Serializable]
    class UniqueActor
    {
        public bool IsSpawned { get; set; }
        public Actor TheActor { get; set; }
        public bool IsWithRefugees { get; set; }
        public string EventThemeMusic { get; set; }
        public string EventMessage { get; set; }
    }

    [Serializable]
    class UniqueActors
    {
        public UniqueActor BigBear { get; set; }
        public UniqueActor Duckman { get; set; }
        public UniqueActor FamuFataru { get; set; }
        public UniqueActor HansVonHanz { get; set; }
        public UniqueActor JasonMyers { get; set; }
        public UniqueActor PoliceStationPrisoner { get; set; }
        public UniqueActor Roguedjack { get; set; }
        public UniqueActor Santaman { get; set; }
        public UniqueActor TheSewersThing { get; set; }

        /// <summary>
        /// Allocate a new array each call don't overuse it...
        /// TODO -- consider caching it.
        /// </summary>
        /// <returns></returns>
        public UniqueActor[] ToArray()
        {
            return new UniqueActor[] {
                BigBear, Duckman, FamuFataru, HansVonHanz, Roguedjack, Santaman,
                PoliceStationPrisoner,  TheSewersThing,
                JasonMyers  // alpha10
            };
        }
    }

    [Serializable]
    class UniqueItem
    {
        public bool IsSpawned { get; set; }
        public Item TheItem { get; set; }
    }

    [Serializable]
    class UniqueItems
    {
        public UniqueItem TheSubwayWorkerBadge { get; set; }
    }

    [Serializable]
    class UniqueMap
    {
        public Map TheMap { get; set; }
    }

    [Serializable]
    class UniqueMaps
    {
        public UniqueMap CHARUndergroundFacility { get; set; }
        public UniqueMap PoliceStation_OfficesLevel { get; set; }
        public UniqueMap PoliceStation_JailsLevel { get; set; }
        public UniqueMap Hospital_Admissions { get; set; }
        public UniqueMap Hospital_Offices { get; set; }
        public UniqueMap Hospital_Patients { get; set; }
        public UniqueMap Hospital_Storage { get; set; }
        public UniqueMap Hospital_Power { get; set; }
    }

    /// <summary>
    /// All the data that is needed to represent the game state, or in other words everything that need to be saved and loaded.
    /// </summary>
    [Serializable]
    class Session
    {
        GameMode m_GameMode;

        WorldTime m_WorldTime;
        World m_World;
        Map m_CurrentMap;

        Scoring m_Scoring;
        [NonSerialized]
        public CharGen charGen = new CharGen();
        [NonSerialized]
        public DiceRoller charGenRoller = new DiceRoller();

        /// <summary>
        /// [RaidType, District.WorldPosition.X, District.WorldPosition.Y] -> turnCounter
        /// </summary>
        int[,,] m_Event_Raids;

        // alpha10.1
        int m_NextAutoSaveTime;

        [NonSerialized]
        static Session s_TheSession;

        /// <summary>
        /// Gets the curent Session (singleton).
        /// </summary>
        public static Session Get
        {
            get
            {
                if (s_TheSession == null)
                    s_TheSession = new Session();
                return s_TheSession;
            }
        }

        public GameMode GameMode
        {
            get { return m_GameMode; }
            set { m_GameMode = value; }
        }

        public int Seed { get; set; }
        public WorldTime WorldTime { get { return m_WorldTime; } }
        public int LastTurnPlayerActed { get; set; }

        public World World
        {
            get { return m_World; }
            set { m_World = value; }
        }

        public Map CurrentMap
        {
            get { return m_CurrentMap; }
            set { m_CurrentMap = value; }
        }

        public Scoring Scoring
        {
            get { return m_Scoring; }
        }

        // alpha10.01
        public int NextAutoSaveTime
        {
            get { return m_NextAutoSaveTime; }
            set { m_NextAutoSaveTime = value; }
        }

        public UniqueActors UniqueActors { get; set; }
        public UniqueItems UniqueItems { get; set; }
        public UniqueMaps UniqueMaps { get; set; }

        public bool PlayerKnows_CHARUndergroundFacilityLocation
        {
            get;
            set;
        }

        public bool PlayerKnows_TheSewersThingLocation
        {
            get;
            set;
        }

        public bool CHARUndergroundFacility_Activated
        {
            get;
            set;
        }

        public ScriptStage ScriptStage_PoliceStationPrisoner
        {
            get;
            set;
        }

        // alpha10
        public FireMode Player_CurrentFireMode { get; set; }

        public int Player_TurnCharismaRoll { get; set; }

        Session()
        {
            Reset();
        }

        public void Reset()
        {
            Seed = (int)DateTime.UtcNow.TimeOfDay.Ticks;
            m_CurrentMap = null;
            m_Scoring = new Scoring();
            m_World = null;
            m_WorldTime = new WorldTime();
            this.LastTurnPlayerActed = 0;

            m_Event_Raids = new int[(int)RaidType._COUNT, RogueGame.Options.CitySize, RogueGame.Options.CitySize];
            for (int i = (int)RaidType._FIRST; i < (int)RaidType._COUNT; i++)
            {
                for (int x = 0; x < RogueGame.Options.CitySize; x++)
                    for (int y = 0; y < RogueGame.Options.CitySize; y++)
                    {
                        m_Event_Raids[i, x, y] = -1;
                    }
            }

            ////////////////////////////
            // Reset special properties.
            ////////////////////////////
            this.CHARUndergroundFacility_Activated = false;
            this.PlayerKnows_CHARUndergroundFacilityLocation = false;
            this.PlayerKnows_TheSewersThingLocation = false;
            this.ScriptStage_PoliceStationPrisoner = ScriptStage.STAGE_0;
            this.UniqueActors = new UniqueActors();
            this.UniqueItems = new UniqueItems();
            this.UniqueMaps = new UniqueMaps();
            // alpha10
            this.Player_CurrentFireMode = FireMode.DEFAULT;
            this.Player_TurnCharismaRoll = 0;
            // alpha10.1
            m_NextAutoSaveTime = 0;
        }

        public bool HasRaidHappened(RaidType raid, District district)
        {
            if (district == null)
                throw new ArgumentNullException("district");

            return m_Event_Raids[(int)raid, district.WorldPosition.X, district.WorldPosition.Y] > -1;
        }

        public int LastRaidTime(RaidType raid, District district)
        {
            if (district == null)
                throw new ArgumentNullException("district");

            return m_Event_Raids[(int)raid, district.WorldPosition.X, district.WorldPosition.Y];
        }

        public void SetLastRaidTime(RaidType raid, District district, int turnCounter)
        {
            if (district == null)
                throw new ArgumentNullException("district");

            lock (m_Event_Raids) // thread safe.
            {
                m_Event_Raids[(int)raid, district.WorldPosition.X, district.WorldPosition.Y] = turnCounter;
            }
        }

        public static void Save(Session session, string filepath)
        {
            // optimize.
            session.World.OptimizeBeforeSaving();

            // save.
            SaveBin(session, filepath);
        }

        public static bool Load(string filepath)
        {
            return LoadBin(filepath);
        }

        static void SaveBin(Session session, string filepath)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN, "saving session...");

            IFormatter formatter = CreateFormatter();
            using (Stream stream = CreateStream(filepath, true))
            {
                formatter.Serialize(stream, session);
                stream.Flush();
                stream.Close();
            }

            Logger.WriteLine(Logger.Stage.RUN, "saving session... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        static bool LoadBin(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN, "loading session...");

            try
            {
                // deserialize.
                IFormatter formatter = CreateFormatter();
                using (Stream stream = CreateStream(filepath, false))
                {
                    s_TheSession = (Session)formatter.Deserialize(stream);
                    stream.Close();
                }

                // reconstruct auxiliary fields.
                s_TheSession.ReconstructAuxiliaryFields();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN, "failed to load session (no save game?).");
                Logger.WriteLine(Logger.Stage.RUN, string.Format("load exception : {0}.", e.ToString()));
                s_TheSession = null;
                return false;
            }


            Logger.WriteLine(Logger.Stage.RUN, "loading session... done!");
            return true;
        }

        public static bool Delete(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN, "deleting saved game...");

            bool hasDeleted = false;
            try
            {
                File.Delete(filepath);
                hasDeleted = true;
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN, "failed to delete saved game (no save?)");
                Logger.WriteLine(Logger.Stage.RUN, "exception :");
                Logger.WriteLine(Logger.Stage.RUN, e.ToString());
                Logger.WriteLine(Logger.Stage.RUN, "failing silently.");
            }

            Logger.WriteLine(Logger.Stage.RUN, "deleting saved game... done!");

            return hasDeleted;
        }

        static IFormatter CreateFormatter()
        {
            return new BinaryFormatter();
        }

        static Stream CreateStream(string saveFileName, bool save)
        {
            return new FileStream(saveFileName,
                save ? FileMode.Create : FileMode.Open,
                save ? FileAccess.Write : FileAccess.Read,
                FileShare.None);
        }

        void ReconstructAuxiliaryFields()
        {
            // reconstruct all maps auxiliary fields.
            for (int x = 0; x < m_World.Size; x++)
                for (int y = 0; y < m_World.Size; y++)
                {
                    foreach (Map map in m_World[x, y].Maps)
                        map.ReconstructAuxiliaryFields();
                }
        }

        public static string DescGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.GM_STANDARD: return "STD - Standard Game";
                case GameMode.GM_CORPSES_INFECTION: return "C&I - Corpses & Infection";
                case GameMode.GM_VINTAGE: return "VTG - Vintage Zombies";
                default: throw new Exception("unhandled game mode");
            }
        }

        public static string DescShortGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.GM_STANDARD: return "STD";
                case GameMode.GM_CORPSES_INFECTION: return "C&I";
                case GameMode.GM_VINTAGE: return "VTG";
                default: throw new Exception("unhandled game mode");
            }
        }

        // alpha10
        public UniqueActor ActorToUniqueActor(Actor a)
        {
            if (!a.IsUnique)
                throw new ArgumentException("actor is not unique");
            foreach (UniqueActor unique in UniqueActors.ToArray())
            {
                if (unique.TheActor == a)
                    return unique;
            }
            throw new ArgumentException("actor is flaged as unique but did not find it!");
        }

#if DEBUG_STATS
        [Serializable]
        public class DistrictStat
        {
            [Serializable]
            public struct Record
            {
                public int livings;
                public int undeads;
            }

            public Dictionary<int, Record> TurnRecords = new Dictionary<int, Record>();
        }

        DistrictStat[,] m_Stats;

        public void UpdateStats(District d)
        {
            if (m_Stats == null)
            {
                m_Stats = new DistrictStat[World.Size, World.Size];
                for (int x = 0; x < World.Size; x++)
                    for (int y = 0; y < World.Size; y++)
                        m_Stats[x, y] = new DistrictStat();
            }

            if (m_Stats[d.WorldPosition.X, d.WorldPosition.Y].TurnRecords.ContainsKey(d.EntryMap.LocalTime.TurnCounter))
                return;

            int l = 0;
            int u = 0;
            foreach (Map m in d.Maps)
            {
                foreach (Actor a in m.Actors)
                {
                    if (a.IsDead) continue;
                    if (a.Model.Abilities.IsUndead)
                        ++u;
                    else
                        ++l;
                }
            }
            m_Stats[d.WorldPosition.X, d.WorldPosition.Y].TurnRecords.Add(d.EntryMap.LocalTime.TurnCounter,
                new DistrictStat.Record()
                {
                    livings = l,
                    undeads = u
                });
        }

        public DistrictStat.Record? GetStatRecord(District d, int turn)
        {
            DistrictStat.Record record;
            if (!m_Stats[d.WorldPosition.X, d.WorldPosition.Y].TurnRecords.TryGetValue(turn, out record))
                return null;
            return record;
        }
#endif
    }
}
