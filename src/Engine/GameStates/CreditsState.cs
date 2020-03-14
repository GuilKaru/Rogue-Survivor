using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class CreditsState : GameState
    {
        public override void Enter()
        {
            musicManager.Play(GameMusics.SLEEP, MusicPriority.PRIORITY_BGM);
        }

        public override void Draw()
        {
            const int left = 0;
            const int right = 256;
            int gy = 0;

            ui.Clear(Color.Black);
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Credits", 0, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Programming, Graphics & Music by Jacques Ruiz (roguedjack) 2018", 0, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;

            ui.DrawStringBold(Color.White, "Programming", left, gy); ui.DrawString(Color.White, "- C# NET 3.5, Microsoft Visual Studio Community 2017", right, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Graphic softwares", left, gy); ui.DrawString(Color.White, "- Inkscape, Paint.NET", right, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Sound & Music softwares", left, gy); ui.DrawString(Color.White, "- GuitarPro 7, Audacity", right, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Sound samples", left, gy); ui.DrawString(Color.White, @"- http://www.sound-fishing.net  http://www.soundsnap.com/", right, gy);

            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Contact", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawString(Color.White, @"Email      : roguedjack@yahoo.fr", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawString(Color.White, @"Blog       : http://roguesurvivor.blogspot.com/", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawString(Color.White, @"Fans Forum : http://roguesurvivor.proboards.com/", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Thanks to the players for their feedback and eagerness to die!", 0, gy);
            gy += Ui.BOLD_LINE_SPACING;

            ui.DrawFootnote(Color.White, "ESC to leave");
        }

        public override void Update()
        {
            Key key = ui.ReadKey();
            if (key == Key.Escape)
                game.PopState();
        }
    }
}
