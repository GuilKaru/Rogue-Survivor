using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RogueSurvivor.Engine
{
    [Serializable]
    enum AdvisorHint
    {
        _FIRST = 0,

        /// <summary>
        /// Basic movement directions.
        /// </summary>
        MOVE_BASIC = _FIRST,

        /// <summary>
        /// Looking with the mouse.
        /// </summary>
        MOUSE_LOOK,

        /// <summary>
        /// Redefining keys & options.
        /// </summary>
        KEYS_OPTIONS,

        /// <summary>
        /// Night effects.
        /// </summary>
        NIGHT,

        /// <summary>
        /// Rainy weather effects.
        /// </summary>
        RAIN,

        /// <summary>
        /// Attacking in melee.
        /// </summary>
        ACTOR_MELEE,

        /// <summary>
        /// Running.
        /// </summary>
        MOVE_RUN,

        /// <summary>
        /// Resting.
        /// </summary>
        MOVE_RESTING,

        /// <summary>
        /// Jumping.
        /// </summary>
        MOVE_JUMP,

        /// <summary>
        /// Grabbing an item from a container.
        /// </summary>
        ITEM_GRAB_CONTAINER,

        /// <summary>
        /// Grabbing an item from the floor.
        /// </summary>
        ITEM_GRAB_FLOOR,

        /// <summary>
        /// Unequiping an item.
        /// </summary>
        ITEM_UNEQUIP,

        /// <summary>
        /// Equiping an item.
        /// </summary>
        ITEM_EQUIP,

        /// <summary>
        /// Barricading material.
        /// </summary>
        ITEM_TYPE_BARRICADING,

        /// <summary>
        /// Dropping an item.
        /// </summary>
        ITEM_DROP,

        /// <summary>
        /// Using an item.
        /// </summary>
        ITEM_USE,

        /// <summary>
        /// Flashlights.
        /// </summary>
        FLASHLIGHT,

        /// <summary>
        /// Cellphones.
        /// </summary>
        CELLPHONES,

        /// <summary>
        /// Using spraypaint.
        /// </summary>
        SPRAYS_PAINT,

        /// <summary>
        /// Using scent sprays.
        /// </summary>
        SPRAYS_SCENT,

        /// <summary>
        /// Firing a weapon.
        /// </summary>
        WEAPON_FIRE,

        /// <summary>
        /// Reloading a weapon.
        /// </summary>
        WEAPON_RELOAD,

        /// <summary>
        /// Using grenades.
        /// </summary>
        GRENADE,

        /// <summary>
        /// Opening a door/window.
        /// </summary>
        DOORWINDOW_OPEN,

        /// <summary>
        /// Closing a door/window.
        /// </summary>
        DOORWINDOW_CLOSE,

        /// <summary>
        /// Pushing/Pulling objects/actors.
        /// </summary>
        OBJECT_PUSH,

        /// <summary>
        /// Breaking objects.
        /// </summary>
        OBJECT_BREAK,

        /// <summary>
        /// Barricading.
        /// </summary>
        BARRICADE,

        /// <summary>
        /// Using an exit such as ladders, stairs.
        /// </summary>
        EXIT_STAIRS_LADDERS,

        /// <summary>
        /// Using an exit to leave the district.
        /// </summary>
        EXIT_LEAVING_DISTRICT,

        /// <summary>
        /// Sleeping.
        /// </summary>
        STATE_SLEEPY,

        /// <summary>
        /// Eating.
        /// </summary>
        STATE_HUNGRY,

        /// <summary>
        /// Trading with NPCs.
        /// </summary>
        NPC_TRADE,

        /// <summary>
        /// Giving items.
        /// </summary>
        NPC_GIVING_ITEM,

        /// <summary>
        /// Shouting.
        /// </summary>
        NPC_SHOUTING,

        /// <summary>
        /// Building fortifications.
        /// </summary>
        BUILD_FORTIFICATION,

        /// <summary>
        /// Leading : need Leadership skill.
        /// </summary>
        LEADING_NEED_SKILL,

        /// <summary>
        /// Leading : can recruit someone.
        /// </summary>
        LEADING_CAN_RECRUIT,

        /// <summary>
        /// Leading : give orders.
        /// </summary>
        LEADING_GIVE_ORDERS,

        /// <summary>
        /// Leading : switching place.
        /// </summary>
        LEADING_SWITCH_PLACE,

        /// <summary>
        /// Saving/Loading.
        /// </summary>
        GAME_SAVE_LOAD,

        /// <summary>
        /// City Information.
        /// </summary>
        CITY_INFORMATION,

        // alpha10 merge corpse hints
        /// <summary>
        /// Corpse actions.
        /// </summary>
        CORPSE,

        /// <summary>
        /// Eating corpses (undead).
        /// </summary>
        CORPSE_EAT,

        // alpha10 new hints

        /// <summary>
        /// Sanity.
        /// </summary>
        SANITY,

        /// <summary>
        /// Infection.
        /// </summary>
        INFECTION,

        /// <summary>
        /// Traps.
        /// </summary>
        TRAPS,

        _COUNT
    }

    [Serializable]
    class GameHintsStatus
    {
        bool[] m_AdvisorHints = new bool[(int)AdvisorHint._COUNT];

        public void ResetAllHints()
        {
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
                m_AdvisorHints[i] = false;
        }

        public bool IsAdvisorHintGiven(AdvisorHint hint)
        {
            return m_AdvisorHints[(int)hint];
        }

        public void SetAdvisorHintAsGiven(AdvisorHint hint)
        {
            m_AdvisorHints[(int)hint] = true;
        }

        public int CountAdvisorHintsGiven()
        {
            int count = 0;
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
                if (m_AdvisorHints[i])
                    ++count;

            return count;
        }

        public bool HasAdvisorGivenAllHints()
        {
            return CountAdvisorHintsGiven() >= (int)AdvisorHint._COUNT;
        }

        public static void Save(GameHintsStatus hints, string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN, "saving hints...");

            IFormatter formatter = CreateFormatter();
            Stream stream = CreateStream(filepath, true);

            formatter.Serialize(stream, hints);
            stream.Flush();
            stream.Close();

            Logger.WriteLine(Logger.Stage.RUN, "saving hints... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        public static GameHintsStatus Load(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN, "loading hints...");

            GameHintsStatus hints;
            try
            {
                IFormatter formatter = CreateFormatter();
                Stream stream = CreateStream(filepath, false);

                hints = (GameHintsStatus)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN, "failed to load hints (first run?).");
                Logger.WriteLine(Logger.Stage.RUN, string.Format("load exception : {0}.", e.ToString()));
                Logger.WriteLine(Logger.Stage.RUN, "resetting.");
                hints = new GameHintsStatus();
                hints.ResetAllHints();
            }

            Logger.WriteLine(Logger.Stage.RUN, "loading options... done!");
            return hints;
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
    }
}
