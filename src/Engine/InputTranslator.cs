﻿using RogueSurvivor.UI;

namespace RogueSurvivor.Engine
{
    static class InputTranslator
    {
        public static PlayerCommand KeyToCommand(Key key)
        {
            PlayerCommand command = RogueGame.KeyBindings.Get(key);
            if (command != PlayerCommand.NONE)
                return command;

            // check special case for item slot keys.
            // slots keys are always used with Ctrl, Shift or Alt modifiers so they are unbound in keybindings.
            KeyInfo keyInfo = new KeyInfo(key);
            if (keyInfo.Modifiers != Key.None)
            {
                // clear modifiers.
                Key unmodifiedKey = keyInfo.KeyCode;

                // slot key?
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_0))
                    return PlayerCommand.ITEM_SLOT_0;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_1))
                    return PlayerCommand.ITEM_SLOT_1;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_2))
                    return PlayerCommand.ITEM_SLOT_2;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_3))
                    return PlayerCommand.ITEM_SLOT_3;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_4))
                    return PlayerCommand.ITEM_SLOT_4;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_5))
                    return PlayerCommand.ITEM_SLOT_5;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_6))
                    return PlayerCommand.ITEM_SLOT_6;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_7))
                    return PlayerCommand.ITEM_SLOT_7;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_8))
                    return PlayerCommand.ITEM_SLOT_8;
                if (unmodifiedKey == RogueGame.KeyBindings.Get(PlayerCommand.ITEM_SLOT_9))
                    return PlayerCommand.ITEM_SLOT_9;
            }

            // no command.
            return PlayerCommand.NONE;
        }
    }
}
