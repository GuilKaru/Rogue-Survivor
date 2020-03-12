using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueSurvivor.Engine;
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
        enum State
        {
            None,
            Init,
            Running
        }

        class BreakException : Exception { }
        class KeyState
        {
            public Key key;
            public decimal time;
            public bool up, handled, received;
        }

        private Xna.GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private List<IDrawItem> drawItems = new List<IDrawItem>();
        private Texture2D m_MinimapTexture;
        private Xna.Color[] m_MinimapColors = new Xna.Color[RogueGame.MAP_MAX_WIDTH * RogueGame.MAP_MAX_HEIGHT];
        private MouseState prevMouseState;
        private bool shutdown, clearCalled;
        private Color lastClearColor;
        private object window;
        private MethodInfo updateMouseState;
        private List<KeyState> keyStates = new List<KeyState>();
        private Stopwatch stopwatch;
        private State state;
        GameLoader loader = new GameLoader();

        RogueGame m_Game;
        SpriteFont m_NormalFont;
        SpriteFont m_BoldFont;

        internal RogueGame Game
        {
            get { return m_Game; }
        }

        public Xna.GraphicsDeviceManager Graphics => graphics;

        public RogueForm()
        {
            Logger.WriteLine(Logger.Stage.INIT, "Creating main form...");

            graphics = new Xna.GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = RogueGame.CANVAS_WIDTH,
                PreferredBackBufferHeight = RogueGame.CANVAS_HEIGHT,
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

            Forms.Form form = (Forms.Form)Forms.Control.FromHandle(Window.Handle);
            form.FormClosed += Form_FormClosed;

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            m_MinimapTexture = new Texture2D(graphics.GraphicsDevice, RogueGame.MAP_MAX_WIDTH, RogueGame.MAP_MAX_HEIGHT);

            Content.RootDirectory = "Resources/Content";
            m_NormalFont = Content.Load<SpriteFont>("NormalFont");
            m_BoldFont = Content.Load<SpriteFont>("BoldFont");

            stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            m_Game = new RogueGame(this);
        }

        private void Form_FormClosed(object sender, Forms.FormClosedEventArgs e)
        {
            shutdown = true;
        }

        protected override void Update(Xna.GameTime gameTime)
        {
            base.Update(gameTime);

            try
            {
                switch (state)
                {
                    case State.None:
                        // do nothing on first frame
                        state = State.Init;
                        Logger.WriteLine(Logger.Stage.INIT, "Preparing items to load...");
                        GameImages.LoadResources(loader, GraphicsDevice);
                        m_Game.Init(loader);
                        break;
                    case State.Init:
                        if (loader.Process())
                        {
                            loader = null;
                            state = State.Running;
                        }
                        break;
                    default:
                        //if (!m_Game.Update())
                        //    Exit();
                        break;
                }
            }
            catch (BreakException)
            {
                Logger.WriteLine(Logger.Stage.CLEAN, "Window closed, shutting down...");
                m_Game.Exit();
                Exit();
            }
        }

        protected override void Draw(Xna.GameTime gameTime)
        {
            base.Draw(gameTime);

            Xna.Matrix matrix;
            if (graphics.IsFullScreen)
            {
                matrix = Xna.Matrix.CreateScale(new Xna.Vector3(
                    (float)Graphics.GraphicsDevice.DisplayMode.Width / RogueGame.CANVAS_WIDTH,
                    (float)Graphics.GraphicsDevice.DisplayMode.Height / RogueGame.CANVAS_HEIGHT,
                    1f));
            }
            else
                matrix = Xna.Matrix.Identity;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: matrix);

            switch (state)
            {
                case State.None:
                    UI_Clear(Color.Black);
                    break;
                case State.Init:
                    loader.Draw(this);
                    break;
                case State.Running:
                    UI_Clear(Color.Red);
                    break;
            }

            spriteBatch.End();
        }

        public Key UI_WaitKey()
        {
            while (true)
            {
                Key key = UI_PeekKey();
                if (key != Key.None)
                    return key;
                Thread.Sleep(10);
            }
        }

        private const decimal KEY_REPEAT = 0.5M;
        private const decimal KEY_REPEAT_INTERVAL = 0.1M;
        public Key UI_PeekKey()
        {
            Forms.Application.DoEvents();
            if (shutdown)
                throw new BreakException();

            decimal dt = stopwatch.ElapsedMilliseconds / 1000M;
            stopwatch.Restart();

            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] keys = keyboardState.GetPressedKeys();
            bool control = false, alt = false, shift = false;
            keys = keys.Where(k =>
            {
                switch (k)
                {
                    case Keys.LeftControl:
                    case Keys.RightControl:
                        control = true;
                        return false;
                    case Keys.LeftShift:
                    case Keys.RightShift:
                        shift = true;
                        return false;
                    case Keys.LeftAlt:
                    case Keys.RightAlt:
                        alt = true;
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

            Key key = Key.None;
            foreach (KeyState keyState in keyStates)
            {
                if (!keyState.received && key == Key.None)
                {
                    key = keyState.key;
                    keyState.received = true;
                }
                if (!keyState.handled)
                    keyState.up = true;
            }
            keyStates.RemoveAll(x => x.received && x.up);

            if (key != Key.None)
            {
                if (control)
                    key |= Key.Control;
                if (shift)
                    key |= Key.Shift;
                if (alt)
                    key |= Key.Alt;
            }

            if (key == (Key.Enter | Key.Alt))
            {
                graphics.ToggleFullScreen();
                Forms.Application.DoEvents();
                UI_Repaint();
            }

            HandleDebugKey(key);

            return key;
        }

        [Conditional("DEBUG")]
        private void HandleDebugKey(Key key)
        {
            // F6 - CHEAT - reveal all
            if (key == Key.F6)
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
        }

        private void RefreshMouse()
        {
            if (shutdown)
                throw new BreakException();

            if (updateMouseState == null)
            {
                object platform = GetType()
                    .GetField("Platform", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)
                    .GetValue(this);
                window = platform.GetType()
                    .GetField("_window", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)
                    .GetValue(platform);
                updateMouseState = window.GetType().GetMethod("UpdateMouseState", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            }

            updateMouseState.Invoke(window, new object[0]);
        }

        public Point UI_GetMousePosition()
        {
            RefreshMouse();
            Xna.Point point = Mouse.GetState().Position;
            float scaleX = (float)Graphics.GraphicsDevice.DisplayMode.Width / RogueGame.CANVAS_WIDTH;
            float scaleY = (float)Graphics.GraphicsDevice.DisplayMode.Height / RogueGame.CANVAS_HEIGHT;
            return new Point((int)(point.X / scaleX), (int)(point.Y / scaleY));
        }

        public MouseButton UI_PeekMouseButtons()
        {
            MouseState mouseState = Mouse.GetState();
            if (prevMouseState == null)
            {
                prevMouseState = mouseState;
                return MouseButton.None;
            }
            MouseButton button = MouseButton.None;
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                button = MouseButton.Left;
            else if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
                button = MouseButton.Right;
            prevMouseState = mouseState;
            return button;
        }

        public void UI_Wait(int msecs)
        {
            Thread.Sleep(msecs);
        }

        private bool takeScreenshot;
        public void UI_Repaint()
        {
            if (!clearCalled)
                graphics.GraphicsDevice.Clear(lastClearColor.ToXna());

            Xna.Matrix matrix;
            if (graphics.IsFullScreen)
            {
                matrix = Xna.Matrix.CreateScale(new Xna.Vector3(
                    (float)Graphics.GraphicsDevice.DisplayMode.Width / RogueGame.CANVAS_WIDTH,
                    (float)Graphics.GraphicsDevice.DisplayMode.Height / RogueGame.CANVAS_HEIGHT,
                    1f));
            }
            else
                matrix = Xna.Matrix.Identity;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: matrix);

            foreach (IDrawItem drawItem in drawItems)
            {
                switch (drawItem)
                {
                    case DrawTextItem drawText:
                        if (drawText.shadowColor.HasValue)
                            spriteBatch.DrawString(drawText.font, drawText.text, new Xna.Vector2(drawText.pos.X + 1, drawText.pos.Y + 1), drawText.shadowColor.Value);
                        spriteBatch.DrawString(drawText.font, drawText.text, drawText.pos, drawText.color);
                        break;
                    case DrawLineItem drawLine:
                        spriteBatch.DrawLine(drawLine.from, drawLine.to, drawLine.color);
                        break;
                    case DrawImageItem drawImage:
                        if (drawImage.transform)
                        {
                            spriteBatch.Draw(drawImage.image, drawImage.pos, null, drawImage.tint, Xna.MathHelper.ToRadians(drawImage.rotation),
                                drawImage.origin, drawImage.scale, SpriteEffects.None, 0.0f);
                        }
                        else
                            spriteBatch.Draw(drawImage.image, drawImage.pos, drawImage.tint);
                        break;
                    case DrawRectangleItem drawRectangle:
                        if (drawRectangle.filled)
                            spriteBatch.DrawRectangle(drawRectangle.rectangle, drawRectangle.color);
                        else
                        {
                            Xna.Rectangle rect = drawRectangle.rectangle;
                            spriteBatch.DrawLine(new Xna.Vector2(rect.Left, rect.Bottom), new Xna.Vector2(rect.Right, rect.Bottom), drawRectangle.color);
                            spriteBatch.DrawLine(new Xna.Vector2(rect.Left, rect.Top), new Xna.Vector2(rect.Right, rect.Top), drawRectangle.color);
                            spriteBatch.DrawLine(new Xna.Vector2(rect.Left, rect.Bottom), new Xna.Vector2(rect.Left, rect.Top), drawRectangle.color);
                            spriteBatch.DrawLine(new Xna.Vector2(rect.Right, rect.Bottom), new Xna.Vector2(rect.Right, rect.Top), drawRectangle.color);
                        }
                        break;
                }
            }

            spriteBatch.End();

            if (!takeScreenshot)
            {
                clearCalled = false;
                EndDraw();
            }
        }

        public void UI_Clear(Color clearColor)
        {
            //clearCalled = true;
            //lastClearColor = clearColor;
            graphics.GraphicsDevice.Clear(clearColor.ToXna());
            //drawItems.Clear();
        }

        public void UI_DrawImage(string imageID, int gx, int gy)
        {
            drawItems.Add(new DrawImageItem
            {
                image = GameImages.Get(imageID),
                pos = new Xna.Vector2(gx, gy),
                tint = Xna.Color.White
            });
        }

        public void UI_DrawImage(string imageID, int gx, int gy, Color tint)
        {
            drawItems.Add(new DrawImageItem
            {
                image = GameImages.Get(imageID),
                pos = new Xna.Vector2(gx, gy),
                tint = tint.ToXna()
            });
        }

        public void UI_DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale)
        {
            Texture2D image = GameImages.Get(imageID);
            drawItems.Add(new DrawImageItem
            {
                image = image,
                pos = new Xna.Vector2(gx + image.Width / 2, gy + image.Height / 2),
                origin = new Xna.Vector2(image.Width / 2, image.Height / 2),
                tint = Xna.Color.White,
                rotation = rotation,
                scale = scale,
                transform = true
            });
        }

        public void UI_DrawGrayLevelImage(string imageID, int gx, int gy)
        {
            drawItems.Add(new DrawImageItem
            {
                image = GameImages.GetGrayLevel(imageID),
                pos = new Xna.Vector2(gx, gy),
                tint = Xna.Color.White
            });
        }

        public void UI_DrawTransparentImage(float alpha, string imageID, int gx, int gy)
        {
            drawItems.Add(new DrawImageItem
            {
                image = GameImages.Get(imageID),
                pos = new Xna.Vector2(gx, gy),
                tint = new Xna.Color(1.0f, 1.0f, 1.0f, alpha)
            });
        }

        public void UI_DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo)
        {
            drawItems.Add(new DrawLineItem
            {
                color = color.ToXna(),
                from = new Xna.Vector2(gxFrom, gyFrom),
                to = new Xna.Vector2(gxTo, gyTo)
            });
        }

        public void UI_DrawString(Color color, string text, int gx, int gy, Color? shadowColor)
        {
            spriteBatch.DrawString(m_NormalFont, text, new Xna.Vector2(gx, gy), color.ToXna());
        }

        public void UI_DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor)
        {
            if (shadowColor.HasValue)
                spriteBatch.DrawString(m_BoldFont, text, new Xna.Vector2(gx + 1, gy + 1), shadowColor.Value.ToXna());
            spriteBatch.DrawString(m_BoldFont, text, new Xna.Vector2(gx, gy), color.ToXna());
        }

        public void UI_DrawRect(Color color, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentOutOfRangeException("rectangle Width/Height <= 0");
            drawItems.Add(new DrawRectangleItem
            {
                color = color.ToXna(),
                rectangle = rect.ToXna(),
                filled = false
            });
        }

        public void UI_FillRect(Color color, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentOutOfRangeException("rectangle Width/Height <= 0");
            drawItems.Add(new DrawRectangleItem
            {
                color = color.ToXna(),
                rectangle = rect.ToXna(),
                filled = true
            });
        }

        public void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
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
            UI_FillRect(boxFillColor, boxRect);
            UI_DrawRect(boxBorderColor, boxRect);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = boxPos.Y + BOX_MARGIN;
            for (int i = 0; i < lines.Length; i++)
            {
                UI_DrawStringBold(textColor, lines[i], lineX, lineY, null);
                lineY += linesSize[i].Y;
            }
        }

        public void UI_DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
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
            UI_FillRect(boxFillColor, boxRect);
            UI_DrawRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (longestLineWidth - titleSize.X) / 2;
            int titleY = boxPos.Y + BOX_MARGIN;
            int titleLineY = titleY + titleSize.Y + TITLE_BAR_LINE;
            UI_DrawStringBold(titleColor, title, titleX, titleY, null);
            UI_DrawLine(boxBorderColor, boxRect.Left, titleLineY, boxRect.Right, titleLineY);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = titleLineY + TITLE_BAR_LINE;

            for (int i = 0; i < lines.Length; i++)
            {
                UI_DrawStringBold(textColor, lines[i], lineX, lineY, null);
                lineY += linesSize[i].Y;
            }
        }

        public void UI_DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy)
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
            UI_FillRect(boxFillColor, boxRect);
            UI_DrawRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (longestLineWidth - titleSize.X) / 2;
            int titleY = boxPos.Y + BOX_MARGIN;
            int titleLineY = titleY + titleSize.Y + TITLE_BAR_LINE;
            UI_DrawStringBold(titleColor, title, titleX, titleY, null);
            UI_DrawLine(boxBorderColor, boxRect.Left, titleLineY, boxRect.Right, titleLineY);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = titleLineY + TITLE_BAR_LINE;

            for (int i = 0; i < lines.Length; i++)
            {
                UI_DrawStringBold(colors[i], lines[i], lineX, lineY, null);
                lineY += linesSize[i].Y;
            }
        }

        public void UI_ClearMinimap(Color color)
        {
            Xna.Color xcolor = color.ToXna();
            for (int i = 0; i < RogueGame.MAP_MAX_HEIGHT * RogueGame.MAP_MAX_WIDTH; ++i)
                m_MinimapColors[i] = xcolor;
        }

        public void UI_SetMinimapColor(int x, int y, Color color)
        {
            m_MinimapColors[x + y * RogueGame.MAP_MAX_WIDTH] = color.ToXna();
        }

        public void UI_DrawMinimap(int gx, int gy)
        {
            m_MinimapTexture.SetData(m_MinimapColors);
            drawItems.Add(new DrawImageItem
            {
                image = m_MinimapTexture,
                pos = new Xna.Vector2(gx, gy),
                origin = Xna.Vector2.Zero,
                tint = Xna.Color.White,
                rotation = 0,
                scale = RogueGame.MINITILE_SIZE,
                transform = true
            });
        }

        public bool UI_SaveScreenshot(string filePath)
        {
            takeScreenshot = true;

            try
            {
                int w = GraphicsDevice.PresentationParameters.BackBufferWidth,
                    h = GraphicsDevice.PresentationParameters.BackBufferHeight;
                RenderTarget2D screenshot = new RenderTarget2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                GraphicsDevice.SetRenderTarget(screenshot);
                UI_Repaint();

                using (FileStream file = new FileStream(filePath, FileMode.Create))
                {
                    screenshot.SaveAsPng(file, w, h);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(Logger.Stage.RUN, String.Format("exception when taking screenshot : {0}", ex.ToString()));
                return false;
            }
            finally
            {
                GraphicsDevice.SetRenderTarget(null);
                takeScreenshot = false;
            }
        }
    }
}
