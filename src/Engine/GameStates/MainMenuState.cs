using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace RogueSurvivor.Engine.GameStates
{
    class MainMenuState : GameState
    {
        int selected;
        bool isLoadEnabled, loading;
        List<Point> santas;

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
            isLoadEnabled = File.Exists(RogueGame.SaveFile);
            loading = false;

            game.MusicManager.Play(GameMusics.INTRO, MusicPriority.PRIORITY_EVENT);

            // christmas special.
            DateTime dateNow = DateTime.Now;
            if (dateNow.Month == 12 && dateNow.Day >= 24 && dateNow.Day <= 26)
            {
                santas = new List<Point>();
                UpdateSantas();
            }
        }

        void UpdateSantas()
        {
            if (santas != null)
            {
                const int NB_SANTAS = 10;
                santas.Clear();
                for (int i = 0; i < NB_SANTAS; i++)
                    santas.Add(new Point(game.Rules.Roll(0, Ui.CANVAS_WIDTH), game.Rules.Roll(0, Ui.CANVAS_HEIGHT)));
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

            if (loading)
            {
                gy += 2 * Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.Yellow, "Loading game, please wait...", gx, gy);
            }

            // christmas special.
            if (santas != null)
            {
                foreach (Point pt in santas)
                {
                    ui.DrawImage(GameImages.ACTOR_SANTAMAN, pt.X, pt.Y);
                    ui.DrawStringBold(Color.Snow, "* Merry Christmas *", pt.X - 60, pt.Y - 10);
                }
            }
        }

        public override void Update(double dt)
        {
            if (loading)
            {
                game.LoadGame(RogueGame.SaveFile);
                game.PopState();
                game.PushState<RogueGame>();
                return;
            }

            Key key = ui.ReadKey();
            switch (key)
            {
                case Key.Up:
                    if (selected > 0)
                        --selected;
                    else
                        selected = menuEntries.Length - 1;
                    UpdateSantas();
                    break;

                case Key.Down:
                    selected = (selected + 1) % menuEntries.Length;
                    UpdateSantas();
                    break;

                case Key.Enter:
                    switch (selected)
                    {
                        case 0:
                            game.PushState<SelectGameModeState>();
                            break;

                        case 1:
                            if (!isLoadEnabled)
                                break;
                            loading = true;
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
