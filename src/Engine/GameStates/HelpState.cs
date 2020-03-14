using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class HelpState : GameState
    {
        int line;

        public override void Enter()
        {
        }

        public override void Draw()
        {
            int gy = 0;

            if (game.Manual == null)
            {
                ui.Clear(Color.Black);
                ui.DrawStringBold(Color.Red, "Game manual not available ingame.", 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawFootnote(Color.White, "press ENTER");
                return;
            }

            // draw header.
            ui.Clear(Color.Black);
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Game Manual", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // draw manual.
            List<string> lines = game.Manual.FormatedLines;
            int iLine = line;
            do
            {
                // ignore commands
                bool ignore = (lines[iLine] == "<SECTION>");

                if (!ignore)
                {
                    ui.DrawStringBold(Color.LightGray, lines[iLine], 0, gy);
                    gy += Ui.BOLD_LINE_SPACING;
                }
                ++iLine;
            }
            while (iLine < lines.Count && gy < Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING);

            // draw foot.
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, numbers to jump to section, ESC to leave");
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();

            if (game.Manual == null)
            {
                if (key == Key.Enter)
                    game.PopState();
                return;
            }

            List<string> lines = game.Manual.FormatedLines;
            int choice = key.ToChoiceNumber();

            if (choice >= 0)
            {
                if (choice == 0)
                {
                    line = 0;
                }
                else
                {
                    // jump to Nth section.
                    int prevLine = line;
                    int sectionCount = 0;
                    line = 0;
                    while (sectionCount < choice && line < lines.Count)
                    {
                        if (lines[line] == "<SECTION>")
                        {
                            ++sectionCount;
                        }
                        ++line;
                    }

                    // if section not found, don't move.
                    if (line >= lines.Count)
                    {
                        line = prevLine;
                    }
                }
            }
            else
            {
                switch (key)
                {
                    case Key.Escape:
                        game.PopState();
                        break;
                    case Key.Up:
                        --line;
                        break;
                    case Key.Down:
                        ++line;
                        break;
                    case Key.PageUp:
                        line -= Ui.TEXTFILE_LINES_PER_PAGE;
                        break;
                    case Key.PageDown:
                        line += Ui.TEXTFILE_LINES_PER_PAGE;
                        break;
                }
            }

            if (line < 0)
                line = 0;
            if (line + Ui.TEXTFILE_LINES_PER_PAGE >= lines.Count)
                line = Math.Max(0, lines.Count - Ui.TEXTFILE_LINES_PER_PAGE);
        }
    }
}
