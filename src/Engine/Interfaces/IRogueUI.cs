using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.Interfaces
{
    /// <summary>
    /// UI constants
    /// </summary>
    class Ui
    {
        public const int CANVAS_WIDTH = 1024;
        public const int CANVAS_HEIGHT = 768;

        public const int LINE_SPACING = 12;
        public const int BOLD_LINE_SPACING = 14;

        public const int TEXTFILE_CHARS_PER_LINE = 120;
        public const int TEXTFILE_LINES_PER_PAGE = 50;
    }

    /// <summary>
    /// Provides UI functionalities to a Rogue game.
    /// </summary>
    interface IRogueUI
    {
        Microsoft.Xna.Framework.Graphics.GraphicsDevice Graphics { get; }

        // Input

        Key ReadKey();
        Point GetMousePosition();
        MouseButton ReadMouseButton();

        // !FIXME
        void UI_Wait(int msecs);
        void UI_Repaint();

        // Drawing

        void ToggleFullscreen();
        void Clear(Color clearColor);
        void DrawImage(string imageID, int gx, int gy);
        void DrawImage(string imageID, int gx, int gy, Color tint);
        void DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale);
        void DrawGrayLevelImage(string imageID, int gx, int gy);
        void DrawTransparentImage(float alpha, string imageID, int gx, int gy);
        void DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo);
        void DrawRect(Color color, Rectangle rect);
        void FillRect(Color color, Rectangle rect);
        void DrawString(Color color, string text, int gx, int gy, Color? shadowColor = null);
        void DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor = null);
        void DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void DrawHeader();
        void DrawFootnote(Color color, string text);
        void DrawMenuOrOptions(int currentChoice, Color entriesColor, string[] entries, Color valuesColor, string[] values, int gx, ref int gy, bool valuesOnNewLine = false, int rightPadding = 256);

        void UI_ClearMinimap(Color color);
        void UI_SetMinimapColor(int x, int y, Color color);
        void UI_DrawMinimap(int gx, int gy);
        bool UI_SaveScreenshot(string filePath);
    }
}
