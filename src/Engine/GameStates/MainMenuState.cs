using RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSurvivor.Engine.GameStates
{
    class MainMenuState : GameState
    {
        List<Point> christmasSpecial;

        public override void Enter()
        {
            m_MusicManager.Play(GameMusics.INTRO, MusicPriority.PRIORITY_EVENT);

            // christmas special.
            DateTime dateNow = DateTime.Now;
            if (dateNow.Month == 12 && dateNow.Day >= 24 && dateNow.Day <= 26)
            {
                christmasSpecial = new List<Point>();
                const int NB_SANTAS = 10;
                for (int i = 0; i < NB_SANTAS; i++)
                    christmasSpecial.Add(new Point(m_Rules.Roll(0, 1024), m_Rules.Roll(0, 768)));
            }
        }

        public override void Draw()
        {
            string[] menuEntries = new string[] {
                "New Game",                                     // 0 
                isLoadEnabled ?  "Load Game" : "(Load Game)",   // 1
                "Redefine keys",                                // 2
                "Options",                                      // 3
                "Game Manual",                                  // 4
                "All Hints",                                    // 5
                "Hi Scores",                                    // 6
                "Credits",                                      // 7
                "Quit Game" };                                  // 8
            int selected = 0;

            // display.
            int gx, gy;
            gx = gy = 0;
            m_UI.UI_Clear(Color.Black);
            DrawHeader();
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Main Menu", 0, gy);
            gy += 2 * BOLD_LINE_SPACING;
            DrawMenuOrOptions(selected, Color.White, menuEntries, Color.White, null, gx, ref gy);
            DrawFootnote(Color.White, "cursor to move, ENTER to select");

            // christmas special.
            foreach (Point pt in christmasSpecial)
            {
                m_UI.UI_DrawImage(GameImages.ACTOR_SANTAMAN, pt.X, pt.Y);
                m_UI.UI_DrawStringBold(Color.Snow, "* Merry Christmas *", pt.X - 60, pt.Y - 10);
            }
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
