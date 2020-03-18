using RogueSurvivor.Engine.Interfaces;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class SelectRaceState : GameState
    {
        readonly string[] menuEntries = new string[]
        {
            "*Random*",
            "Living",
            "Undead"
        };
        readonly string[] descs = new string[]
        {
            "(picks a race at random for you)",
            "Try to survive.",
            "Eat brains and die again."
        };

        int selected;
        bool confirmChoice, confirmUndead;

        public override void Enter()
        {
            selected = 0;
            confirmChoice = false;
        }

        public override void Draw()
        {
            ui.Clear(Color.Black);
            int gx, gy;
            gx = gy = 0;
            ui.DrawStringBold(Color.Yellow, string.Format("[{0}] New Character - Choose Race", Session.DescGameMode(game.Session.GameMode)), gx, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");

            if (confirmChoice)
            {
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, string.Format("Race : {0}.", confirmUndead ? "Undead" : "Living"), gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy);
            }
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();

            if (confirmChoice)
            {
                if (key == Key.Y)
                    SelectRace(confirmUndead);
                else if (key == Key.N || key == Key.Escape)
                    confirmChoice = false;
                return;
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
                    game.PopState();
                    break;

                case Key.Enter:
                    switch (selected)
                    {
                        case 0: // random
                            confirmUndead = game.Session.charGenRoller.RollChance(50);
                            confirmChoice = true;
                            break;

                        case 1: // living
                            SelectRace(false);
                            break;

                        case 2: // undead
                            SelectRace(true);
                            break;
                    }
                    break;
            }
        }

        void SelectRace(bool isUndead)
        {
            game.Session.charGen.IsUndead = isUndead;
            game.PopState();
            if (isUndead)
                game.PushState<SelectUndeadTypeState>();
            else
                game.PushState<SelectGenderState>();
        }
    }
}
