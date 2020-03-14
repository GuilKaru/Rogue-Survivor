using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class HintsState : GameState
    {
        List<string> lines;
        int currentLine;
        double resetTimer;

        public override void Enter()
        {
            currentLine = 0;
            resetTimer = 0;
            BuildHints();
        }

        void BuildHints()
        {
            lines = new List<string>();
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
            {
                game.GetAdvisorHintText((AdvisorHint)i, out string title, out string[] body);
                if (game.Hints.IsAdvisorHintGiven((AdvisorHint)i))
                    title += " (hint already given)";
                lines.Add(string.Format("HINT {0} : {1}", i, title));
                lines.AddRange(body);
                lines.Add("~~~~");
                lines.Add("");
            }
        }

        public override void Draw()
        {
            int gy = 0;

            // header.
            ui.Clear(Color.Black);
            gy = 0;
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            if (resetTimer > 0)
            {
                ui.DrawStringBold(Color.White, "Hints reset done.", 0, gy);
                return;
            }

            // display currently viewed lines.
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            int iLine = currentLine;
            do
            {
                ui.DrawStringBold(Color.LightGray, lines[iLine], 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ++iLine;
            }
            while (iLine < lines.Count && gy < Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING);

            // draw foot.
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, R to reset hints, ESC to leave");
        }

        public override void Update(double dt)
        {
            if (resetTimer > 0)
            {
                resetTimer -= dt;
                return;
            }

            Key key = ui.ReadKey();
            switch (key)
            {
                case Key.Escape:
                    game.PopState();
                    break;
                case Key.Up:
                    --currentLine;
                    break;
                case Key.Down:
                    ++currentLine;
                    break;
                case Key.PageUp:
                    currentLine -= Ui.TEXTFILE_LINES_PER_PAGE;
                    break;
                case Key.PageDown:
                    currentLine += Ui.TEXTFILE_LINES_PER_PAGE;
                    break;
                case Key.R:
                    // reset hints
                    game.Hints.ResetAllHints();
                    BuildHints();
                    resetTimer = 1.0;
                    break;
            }

            if (currentLine < 0)
                currentLine = 0;
            if (currentLine + Ui.TEXTFILE_LINES_PER_PAGE >= lines.Count)
                currentLine = Math.Max(0, lines.Count - Ui.TEXTFILE_LINES_PER_PAGE);
        }
    }
}
