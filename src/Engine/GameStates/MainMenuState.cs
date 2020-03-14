using RogueSurvivor.Engine;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSurvivor.Engine.GameStates
{
    class MainMenuState : BaseGameState
    {
        int selected;
        bool isLoadEnabled;
        List<Point> christmasSpecial;

        string[] menuEntries => new string[] {
                "New Game",                                     // 0 
                isLoadEnabled ?  "Load Game" : "(Load Game)",   // 1
                "Redefine keys",                                // 2
                "Options",                                      // 3
                "Game Manual",                                  // 4
                "All Hints",                                    // 5
                "Hi Scores",                                    // 6
                "Credits",                                      // 7
                "Quit Game" };                                  // 8

        public override void Enter()
        {
            selected = 0;
            isLoadEnabled = File.Exists(game.SaveFilePath);

            game.MusicManager.Play(GameMusics.INTRO, MusicPriority.PRIORITY_EVENT);

            // christmas special.
            DateTime dateNow = DateTime.Now;
            if (dateNow.Month == 12 && dateNow.Day >= 24 && dateNow.Day <= 26)
            {
                christmasSpecial = new List<Point>();
                const int NB_SANTAS = 10;
                //for (int i = 0; i < NB_SANTAS; i++)
                //    christmasSpecial.Add(new Point(m_Rules.Roll(0, 1024), m_Rules.Roll(0, 768)));
                // !FIXME
            }
        }

        public override void Draw()
        {
            // display.
            int gx, gy;
            gx = gy = 0;
            ui.Clear(Color.Black);
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Main Menu", 0, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.White, null, gx, ref gy);
            ui.DrawFootnote(Color.White, "cursor to move, ENTER to select");

            // christmas special.
            /*foreach (Point pt in christmasSpecial)
            {
                ui.DrawImage(GameImages.ACTOR_SANTAMAN, pt.X, pt.Y);
                ui.DrawStringBold(Color.Snow, "* Merry Christmas *", pt.X - 60, pt.Y - 10);
            }*/
            // !FIXME
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

                case Key.Enter:
                    switch (selected)
                    {
                        case 0:
                            game.PushState<SelectGameModeState>();
                            break;

                        case 1:
                            /*if (!isLoadEnabled)
                                break;
                            gy += 2 * Ui.BOLD_LINE_SPACING;
                            m_UI.DrawStringBold(Color.Yellow, "Loading game, please wait...", gx, gy);
                            m_UI.UI_Repaint();
                            LoadGame(GetUserSave());
                            loop = false;
                            // alpha10
                            if (s_Options.IsSimON && s_Options.SimThread)
                                StartSimThread();*/
                            break;

                        case 2:
                            game.PushState<RedefineKeysState>();
                            break;

                        case 3:
                            game.PushState<OptionsState>();
                            break;

                        case 4:
                            game.PushState<HelpState>();
                            break;

                        case 5:
                            game.PushState<HintsState>();
                            break;

                        case 6:
                            game.PushState<HiScoresState>();
                            break;

                        case 7:
                            game.PushState<CreditsState>();
                            break;

                        case 8:
                            game.Exit();
                            break;
                    }
                    break;
            }
        }
    }
}
