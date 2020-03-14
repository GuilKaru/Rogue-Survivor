using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class SelectUndeadTypeState : GameState
    {
        string[] menuEntries;
        string[] descs;
        int selected;
        GameActors.IDs confirmType;
        bool confirmChoice;

        public override void Init()
        {
            ActorModel skeletonModel = game.Actors.Skeleton;
            ActorModel shamblerModel = game.Actors.Zombie;
            ActorModel maleModel = game.Actors.MaleZombified;
            ActorModel femaleModel = game.Actors.FemaleZombified;
            ActorModel masterModel = game.Actors.ZombieMaster;

            menuEntries = new string[]
            {
                "*Random*",
                skeletonModel.Name,
                shamblerModel.Name,
                maleModel.Name,
                femaleModel.Name,
                masterModel.Name,
            };

            descs = new string[]
            {
                "(picks a type at random for you)",
                DescribeUndeadModelStatLine(skeletonModel),
                DescribeUndeadModelStatLine(shamblerModel),
                DescribeUndeadModelStatLine(maleModel),
                DescribeUndeadModelStatLine(femaleModel),
                DescribeUndeadModelStatLine(masterModel)
            };
        }

        string DescribeUndeadModelStatLine(ActorModel m)
        {
            return string.Format("HP:{0:D3}  Spd:{1:F2}  Atk:{2:D2}  Def:{3:D2}  Dmg:{4:D2}  FoV:{5:D1}  Sml:{6:F2}",
                m.StartingSheet.BaseHitPoints, m.DollBody.Speed / 100f,
                m.StartingSheet.UnarmedAttack.HitValue, m.StartingSheet.BaseDefence.Value, m.StartingSheet.UnarmedAttack.DamageValue,
                m.StartingSheet.BaseViewRange, m.StartingSheet.BaseSmellRating);
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
            ui.DrawStringBold(Color.Yellow, string.Format("[{0}] New Undead - Choose Type", Session.DescGameMode(game.Session.GameMode)), gx, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, descs, gx, ref gy);
            ui.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");

            if (confirmChoice)
            {
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, string.Format("Type : {0}.", game.Actors[confirmType].Name), gx, gy);
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
                    SelectType(confirmType);
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
                            selected = game.Session.charGenRoller.Roll(0, 5);
                            switch (selected)
                            {
                                case 0: confirmType = GameActors.IDs.UNDEAD_SKELETON; break;
                                case 1: confirmType = GameActors.IDs.UNDEAD_ZOMBIE; break;
                                case 2: confirmType = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED; break;
                                case 3: confirmType = GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED; break;
                                case 4: confirmType = GameActors.IDs.UNDEAD_ZOMBIE_MASTER; break;
                                default:
                                    throw new ArgumentOutOfRangeException("unhandled select " + selected);
                            }
                            confirmChoice = true;
                            break;

                        case 1: // skeleton
                            SelectType(GameActors.IDs.UNDEAD_SKELETON);
                            break;

                        case 2: // shambler
                            SelectType(GameActors.IDs.UNDEAD_ZOMBIE);
                            break;

                        case 3: // male zombified
                            SelectType(GameActors.IDs.UNDEAD_MALE_ZOMBIFIED);
                            break;

                        case 4: // female zombified
                            SelectType(GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED);
                            break;

                        case 5: // zm
                            SelectType(GameActors.IDs.UNDEAD_ZOMBIE_MASTER);
                            break;
                    }
                    break;
            }
        }

        void SelectType(GameActors.IDs type)
        {
            game.Session.charGen.UndeadModel = type;
            if (type == GameActors.IDs.UNDEAD_MALE_ZOMBIFIED)
                game.Session.charGen.IsMale = true;
            else if (type == GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED)
                game.Session.charGen.IsMale = false;
            game.PopState();
            game.PushState<GenerateWorldState>();
        }
    }
}
