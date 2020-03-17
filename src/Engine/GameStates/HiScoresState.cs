using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Extensions;
using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class HiScoresState : GameState
    {
        public override void Enter()
        {
            SaveToTextFile();
        }

        string FormatScore(HiScore hi, int i)
        {
            return string.Format("{0,3}. | {1,-25} | {2,6} |     {3,3}% | {4,6} | {5,6} | {6,6} | {7,14} | {8}",
                i + 1, hi.Name.Truncate(25),
                hi.TotalPoints, hi.DifficultyPercent, hi.SurvivalPoints, hi.KillPoints, hi.AchievementPoints,
                new WorldTime(hi.TurnSurvived).ToString(), hi.PlayingTime.ToStringShort());
        }

        void SaveToTextFile()
        {
            TextFile file = new TextFile();

            file.Append(string.Format("ROGUE SURVIVOR REANIMATED {0}", SetupConfig.GAME_VERSION));
            file.Append("Hi Scores");
            file.Append("Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time");

            HiScoreTable hiScores = game.HiScoreTable;
            for (int i = 0; i < hiScores.Count; i++)
            {
                HiScore hi = hiScores[i];
                file.Append("------------------------------------------------------------------------------------------------------------------------");
                file.Append(FormatScore(hi, i));
                file.Append(string.Format("     | {0}", hi.SkillsDescription));
                file.Append(string.Format("     | {0}", hi.Death));
            }

            file.Save(RogueGame.HiScoreTextFile);
        }

        public override void Draw()
        {
            ui.Clear(Color.Black);
            int gy = 0;
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Hi Scores", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // display.
            ui.DrawStringBold(Color.White, "Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // individual entries.
            HiScoreTable hiScores = game.HiScoreTable;
            for (int i = 0; i < hiScores.Count; i++)
            {
                // display.
                Color rankColor = (i == 0 ? Color.LightYellow : i == 1 ? Color.LightCyan : i == 2 ? Color.LightGreen : Color.DimGray);
                ui.DrawStringBold(rankColor, "------------------------------------------------------------------------------------------------------------------------", 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
                HiScore hi = hiScores[i];
                string line = FormatScore(hi, i);
                ui.DrawStringBold(rankColor, line, 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(rankColor, string.Format("     | {0}.", hi.SkillsDescription), 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(rankColor, string.Format("     | {0}.", hi.Death), 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
            }

            // display.
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, RogueGame.HiScoreTextFile, 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawFootnote(Color.White, "press ESC to leave");
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();
            if (key == Key.Escape)
                game.PopState();
        }
    }
}
