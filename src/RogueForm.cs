using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueSurvivor.Engine;
using RogueSurvivor.Engine.GameStates;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Extensions;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Forms = System.Windows.Forms;
using Xna = Microsoft.Xna.Framework;
#if DEBUG_STATS
using RogueSurvivor.Data;
#endif

namespace RogueSurvivor
{
    public class RogueForm : Xna.Game, IRogueUI
    {
        class KeyState
        {
            public Key key;
            public double time;
            public bool up, handled, received;
        }

        const double KEY_REPEAT = 0.5;
        const double KEY_REPEAT_INTERVAL = 0.1;

        RogueGame game;
        Xna.GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D m_MinimapTexture;
        Xna.Color[] m_MinimapColors = new Xna.Color[RogueGame.MAP_MAX_WIDTH * RogueGame.MAP_MAX_HEIGHT];
        MouseState prevMouseState;
        List<KeyState> keyStates = new List<KeyState>();
        Key currentKey;
        MouseButton currentButton;
        Point cursorPos;
        SpriteFont m_NormalFont;
        SpriteFont m_BoldFont;

        public GraphicsDevice Graphics => graphics.GraphicsDevice;

        public RogueForm()
        {
            Logger.WriteLine(Logger.Stage.INIT, "Creating main form...");

            graphics = new Xna.GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Ui.CANVAS_WIDTH,
                PreferredBackBufferHeight = Ui.CANVAS_HEIGHT,
                HardwareModeSwitch = false,
                IsFullScreen = false // !FIXME
            };
        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.WriteLine(Logger.Stage.INIT, "Initializing game...");

            Window.Title = "Rogue Survivor Reanimated - " + SetupConfig.GAME_VERSION;
            IsMouseVisible = true;

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            m_MinimapTexture = new Texture2D(graphics.GraphicsDevice, RogueGame.MAP_MAX_WIDTH, RogueGame.MAP_MAX_HEIGHT);

            Content.RootDirectory = "Resources/Content";
            m_NormalFont = Content.Load<SpriteFont>("NormalFont");
            m_BoldFont = Content.Load<SpriteFont>("BoldFont");

            game = new RogueGame(this);
            game.SetState<LoadState>();
        }

        protected override void Update(Xna.GameTime gameTime)
        {
            base.Update(gameTime);

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            HandleInput(dt);

            if (!game.Update(dt))
                Exit();
        }

        protected override void Draw(Xna.GameTime gameTime)
        {
            base.Draw(gameTime);

            Xna.Matrix matrix;
            if (graphics.IsFullScreen)
            {
                matrix = Xna.Matrix.CreateScale(new Xna.Vector3(
                    (float)Graphics.DisplayMode.Width / Ui.CANVAS_WIDTH,
                    (float)Graphics.DisplayMode.Height / Ui.CANVAS_HEIGHT,
                    1f));
            }
            else
                matrix = Xna.Matrix.Identity;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: matrix);

            game.Draw();

            spriteBatch.End();
        }

        private void HandleInput(double dt)
        {
            // Keyboard
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] keys = keyboardState.GetPressedKeys();
            Key keyModifiers = Key.None;

            keys = keys.Where(k =>
            {
                switch (k)
                {
                    case Keys.LeftControl:
                    case Keys.RightControl:
                        keyModifiers |= Key.Control;
                        return false;
                    case Keys.LeftShift:
                    case Keys.RightShift:
                        keyModifiers |= Key.Shift;
                        return false;
                    case Keys.LeftAlt:
                    case Keys.RightAlt:
                        keyModifiers |= Key.Alt;
                        return false;
                    default:
                        return true;
                }
            }).ToArray();

            foreach (KeyState keyState in keyStates)
                keyState.handled = false;

            foreach (Key k in keys)
            {
                KeyState keyState = keyStates.FirstOrDefault(x => x.key == k);
                if (keyState != null)
                {
                    keyState.handled = true;
                    keyState.up = false;
                    keyState.time += dt;
                    if (keyState.time >= KEY_REPEAT)
                    {
                        keyState.time -= KEY_REPEAT_INTERVAL;
                        keyState.received = false;
                    }
                }
                else
                {
                    keyStates.Add(new KeyState
                    {
                        key = k,
                        time = 0,
                        up = false,
                        handled = true,
                        received = false
                    });
                }
            }

            currentKey = Key.None;
            foreach (KeyState keyState in keyStates)
            {
                if (!keyState.received && currentKey == Key.None)
                {
                    currentKey = keyState.key;
                    keyState.received = true;
                }
                if (!keyState.handled)
                    keyState.up = true;
            }
            keyStates.RemoveAll(x => x.received && x.up);

            if (currentKey != Key.None)
                currentKey |= keyModifiers;

            // Mouse
            MouseState mouseState = Mouse.GetState();
            Xna.Point point = mouseState.Position;
            float scaleX = (float)Graphics.DisplayMode.Width / Ui.CANVAS_WIDTH;
            float scaleY = (float)Graphics.DisplayMode.Height / Ui.CANVAS_HEIGHT;
            cursorPos = new Point((int)(point.X / scaleX), (int)(point.Y / scaleY));

            if (prevMouseState != null)
            {
                currentButton = MouseButton.None;
                if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    currentButton = MouseButton.Left;
                else if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
                    currentButton = MouseButton.Right;
            }
            prevMouseState = mouseState;
        }

        public Key ReadKey()
        {
            return currentKey;
        }

        public Point GetMousePosition()
        {
            return cursorPos;
        }

        public MouseButton ReadMouseButton()
        {
            return currentButton;
        }

        [Conditional("DEBUG")]
        private void HandleDebugKey(Key key)
        {
            // F6 - CHEAT - reveal all
            /*if (key == Key.F6)
            {
                if (m_Game.Session != null && m_Game.Session.CurrentMap != null)
                {
                    m_Game.Session.CurrentMap.SetAllAsVisited();
                    UI_Repaint();
                }
            }

            // F7 - DEV - Show actors stats
            if (key == Key.F7)
            {
                m_Game.DEV_ToggleShowActorsStats();
                UI_Repaint();
            }

#if DEBUG_STATS
            // F8 - DEV - Show pop graph.
            if (key == Key.F10)
            {
                District d = m_Game.Player.Location.Map.District;

                UI_Clear(Color.Black);
                // axis
                UI_DrawLine(Color.White, 0, 0, 0, RogueGame.CANVAS_HEIGHT);
                UI_DrawLine(Color.White, 0, RogueGame.CANVAS_HEIGHT, RogueGame.CANVAS_WIDTH, RogueGame.CANVAS_HEIGHT);
                // plot.
                int prevL = 0;
                int prevU = 0;
                const int XSCALE = WorldTime.TURNS_PER_HOUR;
                const int YSCALE = 10;
                for (int turn = 0; turn < m_Game.Session.WorldTime.TurnCounter; turn += XSCALE)
                {
                    if (turn % WorldTime.TURNS_PER_DAY == 0)
                        UI_DrawLine(Color.White, turn / XSCALE, RogueGame.CANVAS_HEIGHT, turn / XSCALE, 0);

                    Session.DistrictStat.Record? r = m_Game.Session.GetStatRecord(d, turn);
                    if (r == null) break;
                    int L = r.Value.livings;
                    UI_DrawLine(Color.Green, 
                        (turn - 1)/XSCALE, RogueGame.CANVAS_HEIGHT - YSCALE * prevL, 
                        turn/XSCALE, RogueGame.CANVAS_HEIGHT - YSCALE * L);
                    int U = r.Value.undeads;
                    UI_DrawLine(Color.Red, 
                        (turn - 1)/XSCALE, RogueGame.CANVAS_HEIGHT - YSCALE * prevU, 
                        turn/XSCALE, RogueGame.CANVAS_HEIGHT - YSCALE * U);
                    prevL = L;
                    prevU = U;
                }
                UI_Repaint();
                UI_WaitKey();
            }
#endif

            // F9 - DEV - Toggle player invincibility
            if (key == Key.F9)
            {
                m_Game.DEV_TogglePlayerInvincibility();
                UI_Repaint();
            }

            // F10 - DEV - Max trust for all player followers
            if (key == Key.F12)
            {
                m_Game.DEV_MaxTrust();
                UI_Repaint();
            }

            // alpha10.1
#if DEBUG
            // INSERT - DEV - Toggle bot mode
            if (key == Key.Insert)
            {
                m_Game.BotToggleControl();
                UI_Repaint();
            }
#endif
*/
            // !FIXME
        }

        public void ToggleFullscreen()
        {
            graphics.ToggleFullScreen();
        }

        public void Clear(Color clearColor)
        {
            graphics.GraphicsDevice.Clear(clearColor.ToXna());
        }

        public void DrawImage(string imageID, int gx, int gy)
        {
            spriteBatch.Draw(GameImages.Get(imageID), new Xna.Vector2(gx, gy), Xna.Color.White);
        }

        public void DrawImage(string imageID, int gx, int gy, Color tint)
        {
            spriteBatch.Draw(GameImages.Get(imageID), new Xna.Vector2(gx, gy), tint.ToXna());
        }

        public void DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale)
        {
            Texture2D image = GameImages.Get(imageID);
            spriteBatch.Draw(image, new Xna.Vector2(gx + image.Width / 2, gy + image.Height / 2), null, Xna.Color.White,
                Xna.MathHelper.ToRadians(rotation), new Xna.Vector2(image.Width / 2, image.Height / 2), scale,
                SpriteEffects.None, 0.0f);
        }

        public void DrawGrayLevelImage(string imageID, int gx, int gy)
        {
            spriteBatch.Draw(GameImages.GetGrayLevel(imageID), new Xna.Vector2(gx, gy), Xna.Color.White);
        }

        public void DrawTransparentImage(float alpha, string imageID, int gx, int gy)
        {
            spriteBatch.Draw(GameImages.Get(imageID), new Xna.Vector2(gx, gy), new Xna.Color(1.0f, 1.0f, 1.0f, alpha));
        }

        public void DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo)
        {
            spriteBatch.DrawLine(new Xna.Vector2(gxFrom, gyFrom), new Xna.Vector2(gxTo, gyTo), color.ToXna());
        }

        public void DrawString(Color color, string text, int gx, int gy, Color? shadowColor)
        {
            spriteBatch.DrawString(m_NormalFont, text, new Xna.Vector2(gx, gy), color.ToXna());
        }

        public void DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor)
        {
            if (shadowColor.HasValue)
                spriteBatch.DrawString(m_BoldFont, text, new Xna.Vector2(gx + 1, gy + 1), shadowColor.Value.ToXna());
            spriteBatch.DrawString(m_BoldFont, text, new Xna.Vector2(gx, gy), color.ToXna());
        }

        public void DrawRect(Color color, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentOutOfRangeException("rectangle Width/Height <= 0");
            Xna.Color xcolor = color.ToXna();
            spriteBatch.DrawLine(new Xna.Vector2(rect.Left, rect.Bottom), new Xna.Vector2(rect.Right, rect.Bottom), xcolor);
            spriteBatch.DrawLine(new Xna.Vector2(rect.Left, rect.Top), new Xna.Vector2(rect.Right, rect.Top), xcolor);
            spriteBatch.DrawLine(new Xna.Vector2(rect.Left, rect.Bottom), new Xna.Vector2(rect.Left, rect.Top), xcolor);
            spriteBatch.DrawLine(new Xna.Vector2(rect.Right, rect.Bottom), new Xna.Vector2(rect.Right, rect.Top), xcolor);
        }

        public void FillRect(Color color, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentOutOfRangeException("rectangle Width/Height <= 0");
            spriteBatch.DrawRectangle(rect.ToXna(), color.ToXna());
        }

        public void DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
        {
            /////////////////
            // Measure lines
            /////////////////
            int longestLineWidth = 0;
            int totalLineHeight = 0;
            Point[] linesSize = new Point[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                linesSize[i] = m_BoldFont.MeasureString(lines[i]).ToPoint().FromXna();
                if (linesSize[i].X > longestLineWidth)
                    longestLineWidth = linesSize[i].X;
                totalLineHeight += linesSize[i].Y;
            }

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(longestLineWidth + 2 * BOX_MARGIN, totalLineHeight + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            FillRect(boxFillColor, boxRect);
            DrawRect(boxBorderColor, boxRect);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = boxPos.Y + BOX_MARGIN;
            for (int i = 0; i < lines.Length; i++)
            {
                DrawStringBold(textColor, lines[i], lineX, lineY, null);
                lineY += linesSize[i].Y;
            }
        }

        public void DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
        {
            /////////////////
            // Measure lines
            /////////////////
            int longestLineWidth = 0;
            int totalLineHeight = 0;
            Point[] linesSize = new Point[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                linesSize[i] = m_BoldFont.MeasureString(lines[i]).ToPoint().FromXna();
                if (linesSize[i].X > longestLineWidth)
                    longestLineWidth = linesSize[i].X;
                totalLineHeight += linesSize[i].Y;
            }

            Point titleSize = m_BoldFont.MeasureString(title).ToPoint().FromXna();
            if (titleSize.X > longestLineWidth)
                longestLineWidth = titleSize.X;
            totalLineHeight += titleSize.Y;
            const int TITLE_BAR_LINE = 1;
            totalLineHeight += TITLE_BAR_LINE;

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(longestLineWidth + 2 * BOX_MARGIN, totalLineHeight + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            FillRect(boxFillColor, boxRect);
            DrawRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (longestLineWidth - titleSize.X) / 2;
            int titleY = boxPos.Y + BOX_MARGIN;
            int titleLineY = titleY + titleSize.Y + TITLE_BAR_LINE;
            DrawStringBold(titleColor, title, titleX, titleY, null);
            DrawLine(boxBorderColor, boxRect.Left, titleLineY, boxRect.Right, titleLineY);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = titleLineY + TITLE_BAR_LINE;

            for (int i = 0; i < lines.Length; i++)
            {
                DrawStringBold(textColor, lines[i], lineX, lineY, null);
                lineY += linesSize[i].Y;
            }
        }

        public void DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy)
        {
            /////////////////
            // Measure lines
            /////////////////
            int longestLineWidth = 0;
            int totalLineHeight = 0;
            Point[] linesSize = new Point[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                linesSize[i] = m_BoldFont.MeasureString(lines[i]).ToPoint().FromXna();
                if (linesSize[i].X > longestLineWidth)
                    longestLineWidth = linesSize[i].X;
                totalLineHeight += linesSize[i].Y;
            }

            Point titleSize = m_BoldFont.MeasureString(title).ToPoint().FromXna();
            if (titleSize.X > longestLineWidth)
                longestLineWidth = titleSize.X;
            totalLineHeight += titleSize.Y;
            const int TITLE_BAR_LINE = 1;
            totalLineHeight += TITLE_BAR_LINE;

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(longestLineWidth + 2 * BOX_MARGIN, totalLineHeight + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            FillRect(boxFillColor, boxRect);
            DrawRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (longestLineWidth - titleSize.X) / 2;
            int titleY = boxPos.Y + BOX_MARGIN;
            int titleLineY = titleY + titleSize.Y + TITLE_BAR_LINE;
            DrawStringBold(titleColor, title, titleX, titleY, null);
            DrawLine(boxBorderColor, boxRect.Left, titleLineY, boxRect.Right, titleLineY);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = titleLineY + TITLE_BAR_LINE;

            for (int i = 0; i < lines.Length; i++)
            {
                DrawStringBold(colors[i], lines[i], lineX, lineY, null);
                lineY += linesSize[i].Y;
            }
        }

        public void DrawHeader()
        {
            DrawStringBold(Color.Red, "ROGUE SURVIVOR REANIMATED - " + SetupConfig.GAME_VERSION, 0, 0, Color.DarkRed);
        }

        public void DrawFootnote(Color color, string text)
        {
            Color shadowColor = Color.FromArgb(color.A, color.R / 2, color.G / 2, color.B / 2);
            DrawStringBold(color, string.Format("<{0}>", text), 0, Ui.CANVAS_HEIGHT - Ui.BOLD_LINE_SPACING, shadowColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries">choices text</param>
        /// <param name="values">(options values) can be null, array must be same length as choices</param>
        /// <param name="gx"></param>
        /// <param name="gy"></param>
        /// <param name="valuesOnNewLine">false: draw values on same line as entries; true: draw values on new lines</param>
        /// <param param name="rightPadding">x padding to add when displaying values</param>
        /// <returns></returns>
        public void DrawMenuOrOptions(int currentChoice, Color entriesColor, string[] entries, Color valuesColor, string[] values, int gx, ref int gy, bool valuesOnNewLine = false, int rightPadding = 256)
        {
            int right = gx + rightPadding;

            if (values != null && entries.Length != values.Length)
                throw new ArgumentException("values length!= choices length");

            // display.
            Color entriesShadowColor = Color.FromArgb(entriesColor.A, entriesColor.R / 2, entriesColor.G / 2, entriesColor.B / 2);
            for (int i = 0; i < entries.Length; i++)
            {
                string choiceStr;
                if (i == currentChoice)
                    choiceStr = string.Format("---> {0}", entries[i]);
                else
                    choiceStr = string.Format("     {0}", entries[i]);
                DrawStringBold(entriesColor, choiceStr, gx, gy, entriesShadowColor);

                if (values != null)
                {
                    string valueStr;
                    if (i == currentChoice && !valuesOnNewLine)
                        valueStr = string.Format("{0} <---", values[i]);
                    else
                        valueStr = values[i];

                    if (valuesOnNewLine)
                    {
                        gy += Ui.BOLD_LINE_SPACING;
                        DrawStringBold(valuesColor, valueStr, gx + right, gy, null);
                    }
                    else
                    {
                        DrawStringBold(valuesColor, valueStr, right, gy, null);
                    }
                }

                gy += Ui.BOLD_LINE_SPACING;
            }
        }

        public void ClearMinimap(Color color)
        {
            Xna.Color xcolor = color.ToXna();
            for (int i = 0; i < RogueGame.MAP_MAX_HEIGHT * RogueGame.MAP_MAX_WIDTH; ++i)
                m_MinimapColors[i] = xcolor;
        }

        public void SetMinimapColor(int x, int y, Color color)
        {
            m_MinimapColors[x + y * RogueGame.MAP_MAX_WIDTH] = color.ToXna();
        }

        public void DrawMinimap(int gx, int gy)
        {
            m_MinimapTexture.SetData(m_MinimapColors);
            spriteBatch.Draw(m_MinimapTexture, new Xna.Vector2(gx, gy), null, Xna.Color.White,
                0, Xna.Vector2.Zero, GameState.MINITILE_SIZE, SpriteEffects.None, 0.0f);
        }

        public bool SaveScreenshot(string filePath)
        {
            try
            {
                int w = GraphicsDevice.PresentationParameters.BackBufferWidth,
                    h = GraphicsDevice.PresentationParameters.BackBufferHeight;
                RenderTarget2D screenshot = new RenderTarget2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                GraphicsDevice.SetRenderTarget(screenshot);

                Draw(new Xna.GameTime());

                using (FileStream file = new FileStream(filePath, FileMode.Create))
                {
                    screenshot.SaveAsPng(file, w, h);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("exception when taking screenshot : {0}", ex.ToString()));
                return false;
            }
            finally
            {
                GraphicsDevice.SetRenderTarget(null);
            }
        }
    }
}
