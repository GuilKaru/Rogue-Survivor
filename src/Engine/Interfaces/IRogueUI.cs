using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.Interfaces
{
    class Ui
    {
        public const int BOLD_LINE_SPACING = 14;
    }

    /// <summary>
    /// Provides UI functionalities to a Rogue game.
    /// </summary>
    interface IRogueUI
    {
        Microsoft.Xna.Framework.GraphicsDeviceManager Graphics { get; }

        Key UI_WaitKey();
        Key UI_PeekKey();
        Point UI_GetMousePosition();
        MouseButton UI_PeekMouseButtons();
        void UI_Wait(int msecs);
        void UI_Repaint();
        void Clear(Color clearColor);
        void UI_DrawImage(string imageID, int gx, int gy);
        void UI_DrawImage(string imageID, int gx, int gy, Color tint);
        void UI_DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale);
        void UI_DrawGrayLevelImage(string imageID, int gx, int gy);
        void UI_DrawTransparentImage(float alpha, string imageID, int gx, int gy);
        void UI_DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo);
        void UI_DrawRect(Color color, Rectangle rect);
        void UI_FillRect(Color color, Rectangle rect);
        void UI_DrawString(Color color, string text, int gx, int gy, Color? shadowColor = null);
        void DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor = null);
        void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void UI_DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void UI_DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy);
        void UI_ClearMinimap(Color color);
        void UI_SetMinimapColor(int x, int y, Color color);
        void UI_DrawMinimap(int gx, int gy);
        bool UI_SaveScreenshot(string filePath);
    }
}
