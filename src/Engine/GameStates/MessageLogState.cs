using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class MessageLogState : GameState
    {
        public override void Draw()
        {
            // draw header.
            ui.Clear(Color.Black);
            int gy = 0;
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Message Log", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // log.
            foreach (Message msg in game.m_MessageManager.History)
            {
                ui.DrawString(msg.Color, msg.Text, 0, gy);
                gy += Ui.LINE_SPACING;
            }

            // foot.
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
