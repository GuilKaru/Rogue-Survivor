﻿namespace RogueSurvivor.Engine
{
    enum PlayerCommand
    {
        NONE,

        QUIT_GAME,
        HELP_MODE,
        ADVISOR,
        OPTIONS_MODE,
        KEYBINDING_MODE,
        HINTS_SCREEN_MODE,
        SCREENSHOT,
        SAVE_GAME,
        LOAD_GAME,
        ABANDON_GAME,

        MOVE_N,
        MOVE_NE,
        MOVE_E,
        MOVE_SE,
        MOVE_S,
        MOVE_SW,
        MOVE_W,
        MOVE_NW,
        RUN_TOGGLE,
        WAIT_OR_SELF,
        WAIT_LONG,

        BARRICADE_MODE,
        BREAK_MODE,
        BUILD_LARGE_FORTIFICATION,
        BUILD_SMALL_FORTIFICATION,
        CLOSE_DOOR,
        EAT_CORPSE,
        FIRE_MODE,
        GIVE_ITEM,
        NEGOCIATE_TRADE,
        LEAD_MODE,
        MARK_ENEMIES_MODE,
        ORDER_MODE,
        PULL_MODE,
        PUSH_MODE,
        REVIVE_CORPSE,
        SHOUT,
        SLEEP,
        SWITCH_PLACE,
        USE_EXIT,
        USE_SPRAY,

        CITY_INFO,
        MESSAGE_LOG,

        ITEM_SLOT_0,
        ITEM_SLOT_1,
        ITEM_SLOT_2,
        ITEM_SLOT_3,
        ITEM_SLOT_4,
        ITEM_SLOT_5,
        ITEM_SLOT_6,
        ITEM_SLOT_7,
        ITEM_SLOT_8,
        ITEM_SLOT_9
    }

    static class PlayerCommandMethods
    {
        public static bool IsSpecial(this PlayerCommand playerCommand)
        {
            switch(playerCommand)
            {
                case PlayerCommand.QUIT_GAME:
                case PlayerCommand.ABANDON_GAME:
                case PlayerCommand.HELP_MODE:
                case PlayerCommand.HINTS_SCREEN_MODE:
                case PlayerCommand.ADVISOR:
                case PlayerCommand.OPTIONS_MODE:
                case PlayerCommand.KEYBINDING_MODE:
                case PlayerCommand.MESSAGE_LOG:
                case PlayerCommand.LOAD_GAME:
                case PlayerCommand.SAVE_GAME:
                case PlayerCommand.SCREENSHOT:
                case PlayerCommand.CITY_INFO:
                    return true;
                default:
                    return false;
            }
        }
    }
}
