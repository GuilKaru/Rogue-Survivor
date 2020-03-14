using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class SelectGenderState : GameState
    {
        readonly string[] menuEntries = new string[]
        {
            "*Random*",
            "Male",
            "Female"
        };
        string[] descs;

        int selected;
        bool confirmChoice, confirmMale;

        public override void Init()
        {
            ActorModel maleModel = game.Actors.MaleCivilian;
            ActorModel femaleModel = game.Actors.FemaleCivilian;

            descs = new string[]
            {
                "(picks a gender at random for you)",
                string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}", maleModel.StartingSheet.BaseHitPoints, maleModel.StartingSheet.BaseDefence.Value,  maleModel.StartingSheet.UnarmedAttack.DamageValue),
                string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}", femaleModel.StartingSheet.BaseHitPoints, femaleModel.StartingSheet.BaseDefence.Value, femaleModel.StartingSheet.UnarmedAttack.DamageValue),
            };
        }

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
            ui.DrawStringBold(Color.Yellow, string.Format("[{0}] New Living - Choose Gender", Session.DescGameMode(game.Session.GameMode)), gx, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
            ui.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");

            if (confirmChoice)
            {
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, string.Format("Gender : {0}.", confirmMale ? "Male" : "Female"), gx, gy);
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
                    SelectGender(confirmMale);
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
                            confirmChoice = true;
                            confirmMale = game.Session.charGenRoller.RollChance(50);
                            break;

                        case 1: // male
                            SelectGender(true);
                            break;

                        case 2: // female
                            SelectGender(false);
                            break;
                    }
                    break;
            }
        }

        void SelectGender(bool male)
        {
            game.Session.charGen.IsMale = male;
            game.PopState();
        }
    }
}
