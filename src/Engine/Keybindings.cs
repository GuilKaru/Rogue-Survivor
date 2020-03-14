using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RogueSurvivor.Engine
{
    [Serializable]
    class Keybindings
    {
        Dictionary<PlayerCommand, Key> m_CommandToKeyData;
        Dictionary<Key, PlayerCommand> m_KeyToCommand;

        public Keybindings()
        {
            m_CommandToKeyData = new Dictionary<PlayerCommand, Key>();
            m_KeyToCommand = new Dictionary<Key, PlayerCommand>();
            ResetToDefaults();
        }

        public void ResetToDefaults()
        {
            m_CommandToKeyData.Clear();
            m_KeyToCommand.Clear();

            Set(PlayerCommand.BARRICADE_MODE, Key.B);
            Set(PlayerCommand.BREAK_MODE, Key.K);
            Set(PlayerCommand.CLOSE_DOOR, Key.C);
            Set(PlayerCommand.FIRE_MODE, Key.F);
            Set(PlayerCommand.HELP_MODE, Key.H);
            Set(PlayerCommand.KEYBINDING_MODE, Key.K | Key.Shift);

            Set(PlayerCommand.ITEM_SLOT_0, Key.D1);
            Set(PlayerCommand.ITEM_SLOT_1, Key.D2);
            Set(PlayerCommand.ITEM_SLOT_2, Key.D3);
            Set(PlayerCommand.ITEM_SLOT_3, Key.D4);
            Set(PlayerCommand.ITEM_SLOT_4, Key.D5);
            Set(PlayerCommand.ITEM_SLOT_5, Key.D6);
            Set(PlayerCommand.ITEM_SLOT_6, Key.D7);
            Set(PlayerCommand.ITEM_SLOT_7, Key.D8);
            Set(PlayerCommand.ITEM_SLOT_8, Key.D9);
            Set(PlayerCommand.ITEM_SLOT_9, Key.D0);

            Set(PlayerCommand.ABANDON_GAME, Key.A | Key.Shift);
            Set(PlayerCommand.ADVISOR, Key.H | Key.Shift);
            Set(PlayerCommand.BUILD_LARGE_FORTIFICATION, Key.N | Key.Control);
            Set(PlayerCommand.BUILD_SMALL_FORTIFICATION, Key.N);
            Set(PlayerCommand.CITY_INFO, Key.I);
            Set(PlayerCommand.EAT_CORPSE, Key.E | Key.Shift);
            Set(PlayerCommand.GIVE_ITEM, Key.G);
            Set(PlayerCommand.HINTS_SCREEN_MODE, Key.H | Key.Control);
            Set(PlayerCommand.NEGOCIATE_TRADE, Key.E);
            Set(PlayerCommand.LOAD_GAME, Key.L | Key.Shift);
            Set(PlayerCommand.MARK_ENEMIES_MODE, Key.E | Key.Control);
            Set(PlayerCommand.MESSAGE_LOG, Key.M | Key.Shift);
            Set(PlayerCommand.MOVE_E, Key.NumPad6);
            Set(PlayerCommand.MOVE_N, Key.NumPad8);
            Set(PlayerCommand.MOVE_NE, Key.NumPad9);
            Set(PlayerCommand.MOVE_NW, Key.NumPad7);
            Set(PlayerCommand.MOVE_S, Key.NumPad2);
            Set(PlayerCommand.MOVE_SE, Key.NumPad3);
            Set(PlayerCommand.MOVE_SW, Key.NumPad1);
            Set(PlayerCommand.MOVE_W, Key.NumPad4);
            Set(PlayerCommand.OPTIONS_MODE, Key.O | Key.Shift);
            Set(PlayerCommand.ORDER_MODE, Key.O);
            Set(PlayerCommand.PULL_MODE, Key.P | Key.Control); // alpha10
            Set(PlayerCommand.PUSH_MODE, Key.P);
            Set(PlayerCommand.QUIT_GAME, Key.Q | Key.Shift);
            Set(PlayerCommand.REVIVE_CORPSE, Key.R | Key.Shift);
            Set(PlayerCommand.RUN_TOGGLE, Key.R);
            Set(PlayerCommand.SAVE_GAME, Key.S | Key.Shift);
            Set(PlayerCommand.SCREENSHOT, Key.N | Key.Shift);
            Set(PlayerCommand.SHOUT, Key.S);
            Set(PlayerCommand.SLEEP, Key.Z);
            Set(PlayerCommand.SWITCH_PLACE, Key.S | Key.Control);
            Set(PlayerCommand.LEAD_MODE, Key.T);
            Set(PlayerCommand.USE_SPRAY, Key.A);
            Set(PlayerCommand.USE_EXIT, Key.X);
            Set(PlayerCommand.WAIT_OR_SELF, Key.NumPad5);
            Set(PlayerCommand.WAIT_LONG, Key.W);
        }

        /// <summary>
        /// Get KeyData (key code & modifiers).
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Key Get(PlayerCommand command)
        {
            Key key;
            if (m_CommandToKeyData.TryGetValue(command, out key))
                return key;
            else
                return Key.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">KeyData</param>
        /// <returns></returns>
        public PlayerCommand Get(Key key)
        {
            PlayerCommand cmd;
            if (m_KeyToCommand.TryGetValue(key, out cmd))
                return cmd;
            return PlayerCommand.NONE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key">KeyData</param>
        public void Set(PlayerCommand command, Key key)
        {
            // remove previous bind.
            PlayerCommand prevCommand = Get(key);
            if (prevCommand != PlayerCommand.NONE)
            {
                m_CommandToKeyData.Remove(prevCommand);
            }
            Key prevKey = Get(command);
            if (prevKey != Key.None)
            {
                m_KeyToCommand.Remove(prevKey);
            }

            // rebind.
            m_CommandToKeyData[command] = key;
            m_KeyToCommand[key] = command;
        }

        public bool CheckForConflict()
        {
            foreach (Key key1 in m_CommandToKeyData.Values)
            {
                int bound = m_KeyToCommand.Keys.Count((k) => k == key1);
                if (bound > 1)
                    return true;
            }

            return false;
        }

        public static void Save(Keybindings kb, string filepath)
        {
            if (kb == null)
                throw new ArgumentNullException("kb");

            Logger.WriteLine(Logger.Stage.RUN, "saving keybindings...");

            IFormatter formatter = CreateFormatter();
            Stream stream = CreateStream(filepath, true);

            formatter.Serialize(stream, kb);
            stream.Flush();
            stream.Close();

            Logger.WriteLine(Logger.Stage.RUN, "saving keybindings... done!");
        }

        /// <summary>
        /// Attempt to load, if failed return bindings with defaults.
        /// </summary>
        /// <returns></returns>
        public static Keybindings Load(string filepath)
        {
            Logger.WriteLine(Logger.Stage.RUN, "loading keybindings...");

            Keybindings kb;

            try
            {
                IFormatter formatter = CreateFormatter();
                Stream stream = CreateStream(filepath, false);

                kb = (Keybindings)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN, "failed to load keybindings (first run?), using defaults.");
                Logger.WriteLine(Logger.Stage.RUN, string.Format("load exception : {0}.", e.ToString()));
                kb = new Keybindings();
                kb.ResetToDefaults();
            }

            Logger.WriteLine(Logger.Stage.RUN, "loading keybindings... done!");

            return kb;
        }

        static IFormatter CreateFormatter()
        {
            return new BinaryFormatter();
        }

        static Stream CreateStream(string saveName, bool save)
        {
            return new FileStream(saveName,
                save ? FileMode.Create : FileMode.Open,
                save ? FileAccess.Write : FileAccess.Read,
                FileShare.None);
        }
    }
}
