using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.UI;
using System;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class RedefineKeysState : GameState
    {
        const int O_MOVE_N = 0;
        const int O_MOVE_NE = 1;
        const int O_MOVE_E = 2;
        const int O_MOVE_SE = 3;
        const int O_MOVE_S = 4;
        const int O_MOVE_SW = 5;
        const int O_MOVE_W = 6;
        const int O_MOVE_NW = 7;
        const int O_WAIT = 8;
        const int O_WAIT_LONG = 9;
        const int O_ABANDON = 10;
        const int O_ADVISOR = 11;
        const int O_BARRICADE = 12;
        const int O_BREAK = 13;
        const int O_BUILD_LARGE_F = 14;
        const int O_BUILD_SMALL_F = 15;
        const int O_CITYINFO = 16;
        const int O_CLOSE = 17;
        const int O_FIRE = 18;
        const int O_GIVE = 19;
        const int O_HELP = 20;
        const int O_HINTS_SCREEN = 21;
        const int O_INIT_TRADE = 22;
        const int O_ITEM_1 = 23;
        const int O_ITEM_2 = 24;
        const int O_ITEM_3 = 25;
        const int O_ITEM_4 = 26;
        const int O_ITEM_5 = 27;
        const int O_ITEM_6 = 28;
        const int O_ITEM_7 = 29;
        const int O_ITEM_8 = 30;
        const int O_ITEM_9 = 31;
        const int O_ITEM_10 = 32;
        const int O_LEAD = 33;
        const int O_LOAD = 34;
        const int O_MARKENEMY = 35;
        const int O_LOG = 36;
        const int O_OPTIONS = 37;
        const int O_ORDER = 38;
        const int O_PULL = 39;
        const int O_PUSH = 40;
        const int O_QUIT = 41;
        const int O_REDEFKEYS = 42;
        const int O_RUN = 43;
        const int O_SAVE = 44;
        const int O_SCREENSHOT = 45;
        const int O_SHOUT = 46;
        const int O_SLEEP = 47;
        const int O_SWITCH = 48;
        const int O_USE_EXIT = 49;
        const int O_USE_SPRAY = 50;

        readonly string[] menuEntries = new string[]
        {
            "Move N",
            "Move NE",
            "Move E",
            "Move SE",
            "Move S",
            "Move SW",
            "Move W",
            "Move NW",
            "Wait",
            "Wait 1 hour",
            "Abandon Game",
            "Advisor Hint",
            "Barricade",
            "Break",
            "Build Large Fortification",
            "Build Small Fortification",
            "City Info",
            "Close",
            "Fire",
            "Give",
            "Help",
            "Hints screen",
            "Negociate Trade",
            "Item 1 slot",
            "Item 2 slot",
            "Item 3 slot",
            "Item 4 slot",
            "Item 5 slot",
            "Item 6 slot",
            "Item 7 slot",
            "Item 8 slot",
            "Item 9 slot",
            "Item 10 slot",
            "Lead",
            "Load Game",
            "Mark Enemies",
            "Messages Log",
            "Options",
            "Order",
            "Pull",
            "Push",
            "Quit Game",
            "Redefine Key",
            "Run",
            "Save Game",
            "Screenshot",
            "Shout",
            "Sleep",
            "Switch Place",
            "Use Exit",
            "Use Spray",
        };

        string[] values;
        int selected;
        bool conflict, rebind;

        public override void Enter()
        {
            RefreshValues();
            selected = 0;
            rebind = false;
        }

        void RefreshValues()
        {
            Keybindings keybindings = game.KeyBindings;

            values = new string[]
            {
                keybindings.Get(PlayerCommand.MOVE_N).ToString(),
                keybindings.Get(PlayerCommand.MOVE_NE).ToString(),
                keybindings.Get(PlayerCommand.MOVE_E).ToString(),
                keybindings.Get(PlayerCommand.MOVE_SE).ToString(),
                keybindings.Get(PlayerCommand.MOVE_S).ToString(),
                keybindings.Get(PlayerCommand.MOVE_SW).ToString(),
                keybindings.Get(PlayerCommand.MOVE_W).ToString(),
                keybindings.Get(PlayerCommand.MOVE_NW).ToString(),
                keybindings.Get(PlayerCommand.WAIT_OR_SELF).ToString(),
                keybindings.Get(PlayerCommand.WAIT_LONG).ToString(),
                keybindings.Get(PlayerCommand.ABANDON_GAME).ToString(),
                keybindings.Get(PlayerCommand.ADVISOR).ToString(),
                keybindings.Get(PlayerCommand.BARRICADE_MODE).ToString(),
                keybindings.Get(PlayerCommand.BREAK_MODE).ToString(),
                keybindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString(),
                keybindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString(),
                keybindings.Get(PlayerCommand.CITY_INFO).ToString(),
                keybindings.Get(PlayerCommand.CLOSE_DOOR).ToString(),
                keybindings.Get(PlayerCommand.FIRE_MODE).ToString(),
                keybindings.Get(PlayerCommand.GIVE_ITEM).ToString(),
                keybindings.Get(PlayerCommand.HELP_MODE).ToString(),
                keybindings.Get(PlayerCommand.HINTS_SCREEN_MODE).ToString(),
                keybindings.Get(PlayerCommand.NEGOCIATE_TRADE).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_0).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_1).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_2).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_3).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_4).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_5).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_6).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_7).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_8).ToString(),
                keybindings.Get(PlayerCommand.ITEM_SLOT_9).ToString(),
                keybindings.Get(PlayerCommand.LEAD_MODE).ToString(),
                keybindings.Get(PlayerCommand.LOAD_GAME).ToString(),
                keybindings.Get(PlayerCommand.MARK_ENEMIES_MODE).ToString(),
                keybindings.Get(PlayerCommand.MESSAGE_LOG).ToString(),
                keybindings.Get(PlayerCommand.OPTIONS_MODE).ToString(),
                keybindings.Get(PlayerCommand.ORDER_MODE).ToString(),
                keybindings.Get(PlayerCommand.PULL_MODE).ToString(),
                keybindings.Get(PlayerCommand.PUSH_MODE).ToString(),
                keybindings.Get(PlayerCommand.QUIT_GAME).ToString(),
                keybindings.Get(PlayerCommand.KEYBINDING_MODE).ToString(),
                keybindings.Get(PlayerCommand.RUN_TOGGLE).ToString(),
                keybindings.Get(PlayerCommand.SAVE_GAME).ToString(),
                keybindings.Get(PlayerCommand.SCREENSHOT).ToString(),
                keybindings.Get(PlayerCommand.SHOUT).ToString(),
                keybindings.Get(PlayerCommand.SLEEP).ToString(),
                keybindings.Get(PlayerCommand.SWITCH_PLACE).ToString(),
                keybindings.Get(PlayerCommand.USE_EXIT).ToString(),
                keybindings.Get(PlayerCommand.USE_SPRAY).ToString(),
            };

            conflict = keybindings.CheckForConflict();
        }

        public override void Draw()
        {
            int gx, gy;
            gx = gy = 0;
            ui.Clear(Color.Black);
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Redefine keys", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGreen, values, gx, ref gy);
            if (conflict)
            {
                ui.DrawStringBold(Color.Red, "Conflicting keys. Please redefine the keys so the commands don't overlap.", gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
            }
            ui.DrawFootnote(Color.White, "cursor to move, ENTER to rebind a key, ESC to save and leave");
            if (rebind)
                ui.DrawStringBold(Color.Yellow, string.Format("rebinding {0}, press the new key.", menuEntries[selected]), gx, gy);
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();

            if (rebind)
            {
                if (key == Key.Escape)
                {
                    rebind = false;
                    return;
                }
                if (key == Key.None)
                    return;

                // get command.
                PlayerCommand command;
                switch (selected)
                {
                    case O_MOVE_N: command = PlayerCommand.MOVE_N; break;
                    case O_MOVE_NE: command = PlayerCommand.MOVE_NE; break;
                    case O_MOVE_E: command = PlayerCommand.MOVE_E; break;
                    case O_MOVE_SE: command = PlayerCommand.MOVE_SE; break;
                    case O_MOVE_S: command = PlayerCommand.MOVE_S; break;
                    case O_MOVE_SW: command = PlayerCommand.MOVE_SW; break;
                    case O_MOVE_W: command = PlayerCommand.MOVE_W; break;
                    case O_MOVE_NW: command = PlayerCommand.MOVE_NW; break;
                    case O_WAIT: command = PlayerCommand.WAIT_OR_SELF; break;
                    case O_WAIT_LONG: command = PlayerCommand.WAIT_LONG; break;
                    case O_ABANDON: command = PlayerCommand.ABANDON_GAME; break;
                    case O_ADVISOR: command = PlayerCommand.ADVISOR; break;
                    case O_BARRICADE: command = PlayerCommand.BARRICADE_MODE; break;
                    case O_BREAK: command = PlayerCommand.BREAK_MODE; break;
                    case O_BUILD_LARGE_F: command = PlayerCommand.BUILD_LARGE_FORTIFICATION; break;
                    case O_BUILD_SMALL_F: command = PlayerCommand.BUILD_SMALL_FORTIFICATION; break;
                    case O_CITYINFO: command = PlayerCommand.CITY_INFO; break;
                    case O_CLOSE: command = PlayerCommand.CLOSE_DOOR; break;
                    case O_FIRE: command = PlayerCommand.FIRE_MODE; break;
                    case O_GIVE: command = PlayerCommand.GIVE_ITEM; break;
                    case O_HELP: command = PlayerCommand.HELP_MODE; break;
                    case O_HINTS_SCREEN: command = PlayerCommand.HINTS_SCREEN_MODE; break;
                    case O_INIT_TRADE: command = PlayerCommand.NEGOCIATE_TRADE; break;
                    case O_ITEM_1: command = PlayerCommand.ITEM_SLOT_0; break;
                    case O_ITEM_2: command = PlayerCommand.ITEM_SLOT_1; break;
                    case O_ITEM_3: command = PlayerCommand.ITEM_SLOT_2; break;
                    case O_ITEM_4: command = PlayerCommand.ITEM_SLOT_3; break;
                    case O_ITEM_5: command = PlayerCommand.ITEM_SLOT_4; break;
                    case O_ITEM_6: command = PlayerCommand.ITEM_SLOT_5; break;
                    case O_ITEM_7: command = PlayerCommand.ITEM_SLOT_6; break;
                    case O_ITEM_8: command = PlayerCommand.ITEM_SLOT_7; break;
                    case O_ITEM_9: command = PlayerCommand.ITEM_SLOT_8; break;
                    case O_ITEM_10: command = PlayerCommand.ITEM_SLOT_9; break;
                    case O_LEAD: command = PlayerCommand.LEAD_MODE; break;
                    case O_LOAD: command = PlayerCommand.LOAD_GAME; break;
                    case O_MARKENEMY: command = PlayerCommand.MARK_ENEMIES_MODE; break;
                    case O_LOG: command = PlayerCommand.MESSAGE_LOG; break;
                    case O_OPTIONS: command = PlayerCommand.OPTIONS_MODE; break;
                    case O_ORDER: command = PlayerCommand.ORDER_MODE; break;
                    case O_PULL: command = PlayerCommand.PULL_MODE; break;
                    case O_PUSH: command = PlayerCommand.PUSH_MODE; break;
                    case O_QUIT: command = PlayerCommand.QUIT_GAME; break;
                    case O_REDEFKEYS: command = PlayerCommand.KEYBINDING_MODE; break;
                    case O_RUN: command = PlayerCommand.RUN_TOGGLE; break;
                    case O_SAVE: command = PlayerCommand.SAVE_GAME; break;
                    case O_SCREENSHOT: command = PlayerCommand.SCREENSHOT; break;
                    case O_SHOUT: command = PlayerCommand.SHOUT; break;
                    case O_SLEEP: command = PlayerCommand.SLEEP; break;
                    case O_SWITCH: command = PlayerCommand.SWITCH_PLACE; break;
                    case O_USE_EXIT: command = PlayerCommand.USE_EXIT; break;
                    case O_USE_SPRAY: command = PlayerCommand.USE_SPRAY; break;
                    default:
                        throw new InvalidOperationException("unhandled selected");
                }

                // bind it.                      
                game.KeyBindings.Set(command, key);
                RefreshValues();
                rebind = false;
            }

            switch (key)
            {
                case Key.Up:
                    if (selected > 0)
                        --selected;
                    else
                        selected = menuEntries.Length - 1;
                    break;

                case Key.Down:
                    selected = (selected + 1) % menuEntries.Length;
                    break;

                case Key.Escape:
                    if (!conflict)
                    {
                        game.KeyBindings.Save(game.KeyBindingsPath);
                        game.PopState();
                    }
                    break;

                case Key.Enter:
                    rebind = true;
                    break;
            }
        }
    }
}
