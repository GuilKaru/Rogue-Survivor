using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class SelectGameModeState : BaseGameState
    {
        readonly string[] menuEntries = new string[]
        {
            Session.DescGameMode(GameMode.GM_STANDARD),
            Session.DescGameMode(GameMode.GM_CORPSES_INFECTION),
            Session.DescGameMode(GameMode.GM_VINTAGE)
        };
        readonly string[] descs = new string[]
        {
            "Rogue Survivor standard game.",
            "Don't get a cold. Keep an eye on your deceased diseased friends.",
            "The classic zombies next door."
        };
        readonly string[][] fullDescs = new string[][]
        {
            new string[]
            {
                "This is the standard game setting.",
                "Recommended for beginners.",
                "- All the kinds of undeads.",
                "- Undeads can evolve to stronger forms.",
                "- Livings can zombify instantly when dead.",
                "- No infection.",
                "- No corpses."
            },
            new string[]
            {
                "This is the standard game setting plus corpses and infection.",
                "Recommended to experience all the features of the game.",
                "- All the kinds of undeads.",
                "- Undeads can evolve to stronger forms.",
                "- Infection:",
                "  - some undeads can infect livings when biting them.",
                "  - infected livings can become ill and die.",
                "  - infected corpses have more chances to rise as zombies.",
                "- Corpses:",
                "  - livings that die drop corpses that will rot away.",
                "  - corpses may rise as zombies.",
                "  - undeads can eat corpses.",
                "  - livings can eat corpses if desperate."
            },
            new string[]
            {
                "This is the classic zombies for hardcore zombie fans.",
                "Recommended if you want classic movies zombies.",
                "- Undeads are only zombified men and women.",
                "- Undeads don't evolve to stronger forms.",
                "- Infection:",
                "  - some undeads can infect livings when biting them.",
                "  - infected livings can become ill and die.",
                "  - infected corpses have more chances to rise as zombies.",
                "- Corpses:",
                "  - livings that die drop corpses that will rot away.",
                "  - corpses may rise as zombies.",
                "  - undeads can eat corpses.",
                "  - livings can eat corpses if desperate.",
                "",
                "NOTE:",
                "This mode force some options OFF.",
                "Remember to set them back ON again when you play other modes!"
            }
        };

        int selected;

        public override void Enter()
        {
            game.Session.Reset();
            selected = 0;
        }

        public override void Draw()
        {
            ui.Clear(Color.Black);
            int gx, gy;
            gx = gy = 0;
            ui.DrawStringBold(Color.Yellow, "New Game - Choose Game Mode", gx, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;

            string[] descMode = fullDescs[selected];
            foreach (string str in descMode)
            {
                ui.DrawStringBold(Color.Gray, str, gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
            }

            ui.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();
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
                    game.PopState();
                    break;

                case Key.Enter:
                    game.SetMode((GameMode)selected);
                    game.PopState();
                    game.PushState<SelectRaceState>();
                    break;
            }
        }
    }
}
