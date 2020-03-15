using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Engine.Items;
using RogueSurvivor.Engine.MapObjects;
using RogueSurvivor.Extensions;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace RogueSurvivor.Engine.GameStates
{
    class GameState : BaseGameState
    {
        enum WaitFor
        {
            None,
            AdvisorInfo,
            WelcomeInfo
        }

        const int TILE_SIZE = 32;
        const int ACTOR_SIZE = 32;
        const int ACTOR_OFFSET = (TILE_SIZE - ACTOR_SIZE) / 2;
        const int TILE_VIEW_WIDTH = 21;
        const int TILE_VIEW_HEIGHT = 21;

        const int RIGHTPANEL_X = TILE_SIZE * TILE_VIEW_WIDTH + 4;
        const int RIGHTPANEL_Y = 0;
        const int RIGHTPANEL_TEXT_X = RIGHTPANEL_X + 4;
        const int RIGHTPANEL_TEXT_Y = RIGHTPANEL_Y + 4;

        const int INVENTORYPANEL_X = RIGHTPANEL_TEXT_X;
        const int INVENTORYPANEL_Y = RIGHTPANEL_TEXT_Y + 170;
        const int GROUNDINVENTORYPANEL_Y = INVENTORYPANEL_Y + 64;
        const int CORPSESPANEL_Y = GROUNDINVENTORYPANEL_Y + 64;
        const int INVENTORY_SLOTS_PER_LINE = 10;

        const int LOCATIONPANEL_X = RIGHTPANEL_X;
        const int LOCATIONPANEL_Y = MESSAGES_Y;
        const int LOCATIONPANEL_TEXT_X = LOCATIONPANEL_X + 4;
        const int LOCATIONPANEL_TEXT_Y = LOCATIONPANEL_Y + 4;

        const int SKILLTABLE_Y = CORPSESPANEL_Y + 64;
        const int SKILLTABLE_LINES = 8;

        public const int MINITILE_SIZE = 2;
        const int MINIMAP_X = RIGHTPANEL_X + (Ui.CANVAS_WIDTH - RIGHTPANEL_X - RogueGame.MAP_MAX_WIDTH * MINITILE_SIZE) / 2;
        const int MINIMAP_Y = MESSAGES_Y - MINITILE_SIZE * RogueGame.MAP_MAX_HEIGHT - 1;
        const int MINI_TRACKER_OFFSET = 1;

        const int MESSAGES_X = 4;
        const int MESSAGES_Y = TILE_VIEW_HEIGHT * TILE_SIZE + 4;
        const int MESSAGES_SPACING = 12;
        const int MESSAGES_FADEOUT = 25;
        const int MAX_MESSAGES = 7;
        const int MESSAGES_HISTORY = 59;

        Session session;
        Actor player;
        WaitFor wait;
        MessageManager msgs = new MessageManager(MESSAGES_SPACING, MESSAGES_FADEOUT, MESSAGES_HISTORY);
        List<Overlay> overlays = new List<Overlay>();

        public override void Enter()
        {
            session = game.Session;
            player = game.Player;

            // scoring : hello there.
            session.Scoring.AddVisit(session.WorldTime.TurnCounter, player.Location.Map);
            session.Scoring.AddEvent(session.WorldTime.TurnCounter, string.Format(player.IsUndead ? "Rose in {0}." : "Woke up in {0}.", player.Location.Map.Name));

            // setup proper scoring mode.
            session.Scoring.Side = (player.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR);

            // schedule first autosave.
            game.ScheduleNextAutoSave();

            // advisor on?
            if (RogueGame.Options.IsAdvisorEnabled)
                ShowAdvisorInfo();
            else
                ShowWelcomeInfo();
        }

        void AddMessage(Message msg)
        {
            // ignore empty messages
            if (msg.Text.Length == 0)
                return;

            // Clear if too much messages.
            if (msgs.Count >= MAX_MESSAGES)
                msgs.Clear();

            // Format message: <turn> <Text>           
            msg.Text = string.Format("{0} {1}", session.WorldTime.TurnCounter, msg.Text.Capitalize());

            // Add.
            msgs.Add(msg);
        }

        void ShowAdvisorInfo()
        {
            msgs.Clear();
            msgs.ClearHistory();
            if (player.IsUndead)
            {
                AddMessage(new Message("The Advisor is enabled but you will get no hint when playing undead.", 0, Color.Red));
            }
            else
            {
                AddMessage(new Message("The Advisor is enabled and will give you hints during the game.", 0, Color.LightGreen));
                AddMessage(new Message("The hints help a beginner learning the basic controls.", 0, Color.LightGreen));
                AddMessage(new Message("You can disable the Advisor by going to the Options screen.", 0, Color.LightGreen));
            }
            AddMessage(new Message(string.Format("Press {0} during the game to change the options.", RogueGame.KeyBindings.Get(PlayerCommand.OPTIONS_MODE)), 0, Color.LightGreen));
            AddMessage(new Message("<press ENTER>", 0, Color.Yellow));
            wait = WaitFor.AdvisorInfo;
        }

        void ShowWelcomeInfo()
        {
            // welcome banner.
            msgs.Clear();
            msgs.ClearHistory();
            AddMessage(new Message("*****************************", 0, Color.LightGreen));
            AddMessage(new Message("* Welcome to Rogue Survivor *", 0, Color.LightGreen));
            AddMessage(new Message("* We hope you like Zombies  *", 0, Color.LightGreen));
            AddMessage(new Message("*****************************", 0, Color.LightGreen));
            AddMessage(new Message(string.Format("Press {0} for help", RogueGame.KeyBindings.Get(PlayerCommand.HELP_MODE)), 0, Color.LightGreen));
            AddMessage(new Message(string.Format("Press {0} to redefine keys", RogueGame.KeyBindings.Get(PlayerCommand.KEYBINDING_MODE)), 0, Color.LightGreen));
            AddMessage(new Message("<press ENTER>", 0, Color.Yellow));
            game.RefreshPlayer();
            wait = WaitFor.WelcomeInfo;
        }

        public override void Draw()
        {
            bool canKnowTime = game.Rules.CanActorKnowTime(player);

            // get mutex.
            Monitor.Enter(ui);

            ui.Clear(Color.Black);
            {
                // map & minimap
                Color mapTint = Color.White; // disabled changing brightness bad for the eyes TintForDayPhase(session.WorldTime.Phase);
                ui.DrawLine(Color.DarkGray, RIGHTPANEL_X, 0, RIGHTPANEL_X, MESSAGES_Y);
                DrawMap(session.CurrentMap, mapTint);

                ui.DrawLine(Color.DarkGray, RIGHTPANEL_X, MINIMAP_Y - 4, Ui.CANVAS_WIDTH, MINIMAP_Y - 4);
                DrawMiniMap(session.CurrentMap);

                // messages
                ui.DrawLine(Color.DarkGray, MESSAGES_X, MESSAGES_Y - 1, Ui.CANVAS_WIDTH, MESSAGES_Y - 1);
                msgs.Draw(ui, session.LastTurnPlayerActed, MESSAGES_X, MESSAGES_Y);

                // location info.
                //    x0            x1 
                // y0 <map name>
                // y1 <zone name>
                // y2 <day>        <dayphase>
                // y3 <hour>       <weather>/<lighting>
                // y4 <turn>       <scoring>@<difficulty> <mode>
                // y5 <life>/<lives>
                // y6 <murders>
                const int X0 = LOCATIONPANEL_TEXT_X;
                const int X1 = LOCATIONPANEL_TEXT_X + 128;
                const int Y0 = LOCATIONPANEL_TEXT_Y;
                const int Y1 = Y0 + Ui.LINE_SPACING;
                const int Y2 = Y1 + Ui.LINE_SPACING;
                const int Y3 = Y2 + Ui.LINE_SPACING;
                const int Y4 = Y3 + Ui.LINE_SPACING;
                const int Y5 = Y4 + Ui.LINE_SPACING;
                const int Y6 = Y5 + Ui.LINE_SPACING;

                ui.DrawLine(Color.DarkGray, LOCATIONPANEL_X, LOCATIONPANEL_Y, LOCATIONPANEL_X, Ui.CANVAS_HEIGHT);
                ui.DrawString(Color.White, session.CurrentMap.Name, X0, Y0);
                ui.DrawString(Color.White, LocationText(session.CurrentMap, player), X0, Y1);
                ui.DrawString(Color.White, string.Format("Day  {0}", session.WorldTime.Day), X0, Y2);
                if (canKnowTime)
                    ui.DrawString(Color.White, string.Format("Hour {0}", session.WorldTime.Hour), X0, Y3);
                else
                    ui.DrawString(Color.White, "Hour ??", X0, Y3);

                // alpha10 desc day fov effect, not if cant know time
                string dayPhaseString;
                if (canKnowTime)
                {
                    dayPhaseString = session.WorldTime.Phase.AsString();
                    int timeFovPenalty = game.Rules.NightFovPenalty(player, session.WorldTime);
                    if (timeFovPenalty != 0)
                        dayPhaseString += "  fov -" + timeFovPenalty;
                }
                else
                {
                    dayPhaseString = "???";
                }

                ui.DrawString(session.WorldTime.IsNight ? game.NIGHT_COLOR : game.DAY_COLOR, dayPhaseString, X1, Y2);

                Color weatherOrLightingColor;
                string weatherOrLightingString;
                switch (session.CurrentMap.Lighting)
                {
                    case Lighting.OUTSIDE:
                        weatherOrLightingColor = session.World.Weather.ToColor();
                        // alpha10 only show weather if can see it
                        if (game.Rules.CanActorSeeSky(player))
                        {
                            weatherOrLightingString = session.World.Weather.AsString();
                            // alpha10 desc weather fov effect
                            int fovPenalty = game.Rules.WeatherFovPenalty(player, session.World.Weather);
                            if (fovPenalty != 0)
                                weatherOrLightingString += "  fov -" + fovPenalty;
                        }
                        else
                            weatherOrLightingString = "???";
                        break;
                    case Lighting.DARKNESS:
                        weatherOrLightingColor = Color.Blue;
                        weatherOrLightingString = "Darkness";
                        int darknessFov = game.Rules.DarknessFov(player);
                        if (darknessFov != player.Sheet.BaseViewRange)
                            weatherOrLightingString += "  fov " + darknessFov;
                        break;
                    case Lighting.LIT:
                        weatherOrLightingColor = Color.Yellow;
                        weatherOrLightingString = "Lit";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("unhandled lighting");
                }
                ui.DrawString(weatherOrLightingColor, weatherOrLightingString, X1, Y3);
                ui.DrawString(Color.White, string.Format("Turn {0}", session.WorldTime.TurnCounter), X0, Y4);
                ui.DrawString(Color.White, string.Format("Score   {0}@{1}% {2}", session.Scoring.TotalPoints, (int)(100 * Scoring.ComputeDifficultyRating(RogueGame.Options, session.Scoring.Side, session.Scoring.ReincarnationNumber)), Session.DescShortGameMode(session.GameMode)), X1, Y4);
                ui.DrawString(Color.White, string.Format("Avatar  {0}/{1}", (1 + session.Scoring.ReincarnationNumber), (1 + RogueGame.Options.MaxReincarnations)), X1, Y5);
                if (player.MurdersCounter > 0)
                    ui.DrawString(Color.White, string.Format("Murders {0}", player.MurdersCounter), X1, Y6);

                // character status.
                DrawActorStatus(player, RIGHTPANEL_TEXT_X, RIGHTPANEL_TEXT_Y);

                // inventories.
                if (player.Inventory != null && player.Model.Abilities.HasInventory)
                    DrawInventory(player.Inventory, "Inventory", true, INVENTORY_SLOTS_PER_LINE, player.Inventory.MaxCapacity, INVENTORYPANEL_X, INVENTORYPANEL_Y);
                DrawInventory(player.Location.Map.GetItemsAt(player.Location.Position), "Items on ground", true, INVENTORY_SLOTS_PER_LINE, Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y);
                DrawCorpsesList(player.Location.Map.GetCorpsesAt(player.Location.Position), "Corpses on ground", INVENTORY_SLOTS_PER_LINE, INVENTORYPANEL_X, CORPSESPANEL_Y);

                // character skills.
                if (player.Sheet.SkillTable != null && player.Sheet.SkillTable.CountSkills > 0)
                    DrawActorSkillTable(player, RIGHTPANEL_TEXT_X, SKILLTABLE_Y);

                // overlays
                Monitor.Enter(overlays);
                foreach (Overlay o in overlays)
                    o.Draw(ui);
                Monitor.Exit(overlays);

                // DEV STATS
#if DEBUG
                if (RogueGame.Options.DEV_ShowActorsStats)
                {
                    int countLiving, countUndead;
                    countLiving = game.CountLivings(session.CurrentMap);
                    countUndead = game.CountUndeads(session.CurrentMap);
                    ui.DrawString(Color.White, string.Format("Living {0} vs {1} Undead", countLiving, countUndead), RIGHTPANEL_TEXT_X, SKILLTABLE_Y - 32);
                }
#endif
            }

            // release mutex.
            Monitor.Exit(ui);
        }

        void DrawMap(Map map, Color tint)
        {
            // trim to outer map bounds.
            int left = Math.Max(-1, game.m_MapViewRect.Left);
            int right = Math.Min(map.Width + 1, game.m_MapViewRect.Right);
            int top = Math.Max(-1, game.m_MapViewRect.Top);
            int bottom = Math.Min(map.Height + 1, game.m_MapViewRect.Bottom);

            // get weather image.
            string weatherImage;
            switch (session.World.Weather)
            {
                case Weather.RAIN:
                    weatherImage = (session.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_RAIN1 : GameImages.WEATHER_RAIN2);
                    break;
                case Weather.HEAVY_RAIN:
                    weatherImage = (session.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_HEAVY_RAIN1 : GameImages.WEATHER_HEAVY_RAIN2);
                    break;

                default:
                    weatherImage = null;
                    break;
            }

            ///////////////////////////////////////////
            // Layered draw:
            // 1. Tiles.
            // 2. Corpses.
            // 3. (Target statut), Map objects.
            // 4. Scents.
            // 5. Items, Actors (if visible).
            // 6. Water cover.
            // 7. Weather (if visible and not inside).
            ///////////////////////////////////////////
            Point position = new Point();
            bool isUndead = player.Model.Abilities.IsUndead;
            bool hasSmell = player.Model.StartingSheet.BaseSmellRating > 0;
            int playerSmellTheshold = game.Rules.ActorSmellThreshold(player);
            for (int x = left; x < right; x++)
            {
                position.X = x;
                for (int y = top; y < bottom; y++)
                {
                    position.Y = y;
                    Point toScreen = game.MapToScreen(x, y);
                    bool isVisible = game.IsVisibleToPlayer(map, position);
                    bool drawWater = false;
                    Tile tile = map.IsInBounds(x, y) ? map.GetTileAt(x, y) : null;

                    // 1. Tile
                    if (map.IsInBounds(x, y))
                        DrawTile(tile, toScreen, tint);
                    else if (map.IsMapBoundary(x, y))
                    {
                        if (map.GetExitAt(position) != null)
                            DrawExit(toScreen);
                    }

                    // 2. Corpses
                    if (isVisible)
                    {
                        List<Corpse> corpses = map.GetCorpsesAt(x, y);
                        if (corpses != null)
                        {
                            foreach (Corpse c in corpses)
                                DrawCorpse(c, toScreen.X, toScreen.Y, tint);
                        }
                    }

                    // 3. (TargetStatus), Map objects
                    if (RogueGame.Options.ShowPlayerTargets && !player.IsSleeping && player.Location.Position == position)
                        DrawPlayerActorTargets(player);
                    MapObject mapObj = map.GetMapObjectAt(x, y);
                    if (mapObj != null)
                    {
                        DrawMapObject(mapObj, toScreen, tint);
                        drawWater = true;
                    }

                    // 4. Scents
                    if (!player.IsSleeping && map.IsInBounds(x, y) && game.Rules.GridDistance(player.Location.Position, position) <= 1)
                    {
                        // scents alpha is low to be able to see objects behind them (eg: scent on a door)
                        // squaring alpha helps increase discrimination for player.

                        if (isUndead)
                        {
                            // Undead can see living & zm scents.
                            if (hasSmell)
                            {
                                // living scent?
                                int livingScent = map.GetScentByOdorAt(Odor.LIVING, position);
                                if (livingScent >= playerSmellTheshold)
                                {
                                    float alpha = 0.90f * (float)livingScent / (float)OdorScent.MAX_STRENGTH;
                                    alpha *= alpha;
                                    ui.DrawTransparentImage(alpha, GameImages.ICON_SCENT_LIVING, toScreen.X, toScreen.Y);
                                }

                                // zombie master scent?
                                int masterScent = map.GetScentByOdorAt(Odor.UNDEAD_MASTER, position);
                                if (masterScent >= playerSmellTheshold)
                                {
                                    float alpha = 0.90f * (float)masterScent / (float)OdorScent.MAX_STRENGTH;
                                    alpha *= alpha;
                                    ui.DrawTransparentImage(alpha, GameImages.ICON_SCENT_ZOMBIEMASTER, toScreen.X, toScreen.Y);
                                }
                            }
                        }
                        else
                        {
                            // Living can see some perfumes.
                            // alpha10 obsolete // perfume: living suppressor?
                            //int livingSupr = map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, position);
                            //if (livingSupr > 0)
                            //{
                            //    float alpha = 0.90f * (float)livingSupr / (float)OdorScent.MAX_STRENGTH;
                            //    //alpha *= alpha;
                            //    ui.UI_DrawTransparentImage(alpha, GameImages.ICON_SCENT_LIVING_SUPRESSOR, toScreen.X, toScreen.Y);
                            //}
                        }
                    }

                    // 5. Items, Actors (if visible)
                    if (isVisible)
                    {
                        // 4.2. Items
                        Inventory inv = map.GetItemsAt(x, y);
                        if (inv != null)
                        {
                            DrawItemsStack(inv, toScreen.X, toScreen.Y, tint);
                            drawWater = true;
                        }

                        // 4.3. Actors
                        Actor actor = map.GetActorAt(x, y);
                        if (actor != null)
                        {
                            DrawActorSprite(actor, toScreen, tint);
                            drawWater = true;
                        }
                    }

                    // 6. Water cover.
                    if (tile != null && tile.HasDecorations)
                        drawWater = true;
                    if (drawWater && tile.Model.IsWater)
                        DrawTileWaterCover(tile, toScreen, tint);

                    // 7. Weather (if visible and not inside).
                    if (isVisible && weatherImage != null && tile != null && !tile.IsInside)
                        ui.DrawImage(weatherImage, toScreen.X, toScreen.Y);
                }
            }

            // DEV: scents
#if false
            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                {
                    if (map.IsInBounds(x, y))
                    {
                        int scent = map.GetScentByOdorAt(Odor.LIVING, new Point(x, y));
                        if (scent > 0)
                        {
                            ui.UI_DrawString(Color.White, string.Format("{0}", scent), MapToScreen(x, y).X, MapToScreen(x, y).Y);
                        }
                    }
                }
#endif
        }

        void DrawTile(Tile tile, Point screen, Color tint)
        {
            if (tile.IsInView)  // visible
            {
                // tile.
                ui.DrawImage(tile.Model.ImageID, screen.X, screen.Y, tint);

                // animation layer.
                string movingWater = MovingWaterImage(tile.Model, session.WorldTime.TurnCounter);
                if (movingWater != null)
                    ui.DrawImage(movingWater, screen.X, screen.Y, tint);

                // decorations.
                if (tile.HasDecorations)
                    foreach (string deco in tile.Decorations)
                        ui.DrawImage(deco, screen.X, screen.Y, tint);
            }
            else if (tile.IsVisited && !player.IsSleeping) // memorized
            {
                // tile.
                ui.DrawGrayLevelImage(tile.Model.ImageID, screen.X, screen.Y);

                // animation layer.
                string movingWater = MovingWaterImage(tile.Model, session.WorldTime.TurnCounter);
                if (movingWater != null)
                    ui.DrawGrayLevelImage(movingWater, screen.X, screen.Y);

                // deocrations.
                if (tile.HasDecorations)
                    foreach (string deco in tile.Decorations)
                        ui.DrawGrayLevelImage(deco, screen.X, screen.Y);
            }
        }

        string MovingWaterImage(TileModel model, int turnCount)
        {
            if (model == game.Tiles.FLOOR_SEWER_WATER)
            {
                int i = turnCount % 3;
                switch (i)
                {
                    case 0: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM1;
                    case 1: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM2;
                    default: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM3;
                }
            }

            return null;
        }

        void DrawTileWaterCover(Tile tile, Point screen, Color tint)
        {
            if (tile.IsInView)  // visible
            {
                // tile.
                ui.DrawImage(tile.Model.WaterCoverImageID, screen.X, screen.Y, tint);
            }
            else if (tile.IsVisited && !player.IsSleeping) // memorized
            {
                // tile.
                ui.DrawGrayLevelImage(tile.Model.WaterCoverImageID, screen.X, screen.Y);
            }
        }

        void DrawExit(Point screen)
        {
            ui.DrawImage(GameImages.MAP_EXIT, screen.X, screen.Y);
        }

        void DrawCorpse(Corpse c, int gx, int gy, Color tint)
        {
            float rotation = c.Rotation;
            float scale = c.Scale;
            int offset = 0;// TILE_SIZE / 2;

            Actor actor = c.DeadGuy;

            gx += ACTOR_OFFSET + offset;
            gy += ACTOR_OFFSET + offset;

            // model.
            if (actor.Model.ImageID != null)
                ui.DrawImageTransform(actor.Model.ImageID, gx, gy, rotation, scale);

            // skinning/clothing.
            DrawActorDecoration(actor, gx, gy, DollPart.SKIN, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.FEET, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.LEGS, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.EYES, rotation, scale);
            DrawActorDecoration(actor, gx, gy, DollPart.HEAD, rotation, scale);

            gx -= ACTOR_OFFSET + offset;
            gy -= ACTOR_OFFSET + offset;

            // rotting.
            int rotLevel = game.Rules.CorpseRotLevel(c);
            string img = null;
            switch (rotLevel)
            {
                case 5:
                case 4:
                case 3:
                case 2:
                case 1: img = "rot" + rotLevel + "_"; break;
                case 0: break;
                default: throw new Exception("unhandled rot level");
            }
            if (img != null)
            {
                // anim frame.
                img += 1 + (session.WorldTime.TurnCounter % 2);
                // a bit of offset for a nice flies movement effect.
                int rotdx = (session.WorldTime.TurnCounter % 5) - 2;
                int rotdy = ((session.WorldTime.TurnCounter / 3) % 5) - 2;
                ui.DrawImage(img, gx + rotdx, gy + rotdy);
            }
        }

        void DrawCorpsesList(List<Corpse> list, string title, int slots, int gx, int gy)
        {
            int x, y;
            int slot = 0;

            // Draw title.
            int n = (list == null ? 0 : list.Count);
            if (n > 0) title += " : " + n;
            gy -= Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, title, gx, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // Draw slots.
            x = gx; y = gy; slot = 0;
            for (int i = 0; i < slots; i++)
            {
                ui.DrawImage(GameImages.ITEM_SLOT, x, y);
                x += TILE_SIZE;
            }

            // Draw corpses.
            if (list == null)
                return;

            x = gx; y = gy; slot = 0;
            foreach (Corpse c in list)
            {
                if (c.IsDragged)
                    ui.DrawImage(GameImages.CORPSE_DRAGGED, x, y);
                DrawCorpse(c, x, y, Color.White);
                if (++slot >= slots)
                    break;
                else
                    x += TILE_SIZE;
            }
        }

        void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, float rotation, float scale)
        {
            List<string> decos = actor.Doll.GetDecorations(part);
            if (decos == null)
                return;

            foreach (string imageID in decos)
                ui.DrawImageTransform(imageID, gx, gy, rotation, scale);
        }

        /// <summary>
        /// immediate mode
        /// </summary>
        /// <param name="player"></param>
        void DrawPlayerActorTargets(Actor player)
        {
            Point offset = new Point(TILE_SIZE / 2, TILE_SIZE / 2);

            if (player.TargetActor != null && !player.TargetActor.IsDead && game.IsVisibleToPlayer(player.TargetActor))
            {
                Point gpos = game.MapToScreen(player.TargetActor.Location.Position);
                ui.DrawImage(GameImages.ICON_IS_TARGET, gpos.X, gpos.Y);
            }
            foreach (Actor a in player.Location.Map.Actors)
            {
                if (a == player || a.IsDead || !game.IsVisibleToPlayer(a))
                    continue;
                if (a.TargetActor == player && (a.Activity == Activity.CHASING || a.Activity == Activity.FIGHTING))
                {
                    Point gpos = game.MapToScreen(player.Location.Position);
                    ui.DrawImage(GameImages.ICON_IS_TARGETTED, gpos.X, gpos.Y);
                    break;
                }
            }
        }

        void DrawMapObject(MapObject mapObj, Point screen, Color tint)
        {
            // pushables objects in water floating animation.
            if (mapObj.IsMovable && mapObj.Location.Map.GetTileAt(mapObj.Location.Position.X, mapObj.Location.Position.Y).Model.IsWater)
            {
                int yDrift = (mapObj.Location.Position.X + session.WorldTime.TurnCounter) % 2 == 0 ? -2 : 0;
                screen.Y -= yDrift;
            }

            if (game.IsVisibleToPlayer(mapObj))
            {
                DrawMapObject(mapObj, screen, mapObj.ImageID, (imageID, gx, gy) => ui.DrawImage(imageID, gx, gy, tint));

                if (mapObj.HitPoints < mapObj.MaxHitPoints && mapObj.HitPoints > 0)
                    DrawMapHealthBar(mapObj.HitPoints, mapObj.MaxHitPoints, screen.X, screen.Y);

                DoorWindow door = mapObj as DoorWindow;
                if (door != null && door.BarricadePoints > 0)
                {
                    DrawMapHealthBar(door.BarricadePoints, Rules.BARRICADING_MAX, screen.X, screen.Y, Color.Green);
                    ui.DrawImage(GameImages.EFFECT_BARRICADED, screen.X, screen.Y, tint);
                }
            }
            else if (game.IsKnownToPlayer(mapObj) && !player.IsSleeping)
            {
                DrawMapObject(mapObj, screen, mapObj.HiddenImageID, (imageID, gx, gy) => ui.DrawGrayLevelImage(imageID, gx, gy));
            }
        }

        void DrawMapObject(MapObject mapObj, Point screen, string imageID, Action<string, int, int> drawFn)
        {
            // draw image.
            drawFn(imageID, screen.X, screen.Y);

            // draw effects.
            if (mapObj.IsOnFire)
                drawFn(GameImages.EFFECT_ONFIRE, screen.X, screen.Y);
        }

        void DrawItemsStack(Inventory inventory, int gx, int gy, Color tint)
        {
            if (inventory == null)
                return;

            foreach (Item it in inventory.Items)
                DrawItem(it, gx, gy, tint);
        }

        void DrawActorSprite(Actor actor, Point screen, Color tint)
        {
            int gx = screen.X;
            int gy = screen.Y;

            // player follower?
            if (actor.Leader != null && actor.Leader == player)
            {
                if (game.Rules.HasActorBondWith(actor, player))
                    ui.DrawImage(GameImages.PLAYER_FOLLOWER_BOND, gx, gy, tint);
                else if (game.Rules.IsActorTrustingLeader(actor))
                    ui.DrawImage(GameImages.PLAYER_FOLLOWER_TRUST, gx, gy, tint);
                else
                    ui.DrawImage(GameImages.PLAYER_FOLLOWER, gx, gy, tint);
            }

            gx += ACTOR_OFFSET;
            gy += ACTOR_OFFSET;

            // model
            if (actor.Model.ImageID != null)
                ui.DrawImage(actor.Model.ImageID, gx, gy, tint);

            // skinning/clothing and body equipment.
            DrawActorDecoration(actor, gx, gy, DollPart.SKIN, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.FEET, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.LEGS, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.TORSO, tint);
            if (actor.GetEquippedItem(DollPart.TORSO) != null)
                DrawActorEquipment(actor, gx - ACTOR_OFFSET, gy - ACTOR_OFFSET, DollPart.TORSO, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.EYES, tint);
            DrawActorDecoration(actor, gx, gy, DollPart.HEAD, tint);

            // hands equipment
            DrawActorEquipment(actor, gx - ACTOR_OFFSET, gy - ACTOR_OFFSET, DollPart.LEFT_HAND, tint);
            DrawActorEquipment(actor, gx - ACTOR_OFFSET, gy - ACTOR_OFFSET, DollPart.RIGHT_HAND, tint);

            gx -= ACTOR_OFFSET;
            gy -= ACTOR_OFFSET;

            // personal enemy?
            if (player != null)
            {
                bool imSelfDefence = player.IsSelfDefenceFrom(actor);
                bool imTheAggressor = player.IsAggressorOf(actor);
                bool groupEnemies = !player.Faction.IsEnemyOf(actor.Faction) && game.Rules.AreGroupEnemies(player, actor); // alpha10
                if (imSelfDefence)
                    ui.DrawImage(GameImages.ICON_SELF_DEFENCE, gx, gy, tint);
                else if (imTheAggressor)
                    ui.DrawImage(GameImages.ICON_AGGRESSOR, gx, gy, tint);
                else if (groupEnemies)
                    ui.DrawImage(GameImages.ICON_INDIRECT_ENEMIES, gx, gy, tint);
            }

            // activity
            switch (actor.Activity)
            {
                case Activity.IDLE:
                    break;

                case Activity.CHASING:
                case Activity.FIGHTING:
                    if (actor.IsPlayer)
                        break;
                    if (actor.TargetActor == null)
                        break;

                    if (actor.TargetActor != null && actor.TargetActor == player)
                        ui.DrawImage(GameImages.ACTIVITY_CHASING_PLAYER, gx, gy, tint);
                    else
                        ui.DrawImage(GameImages.ACTIVITY_CHASING, gx, gy, tint);
                    break;

                case Activity.TRACKING:
                    if (actor.IsPlayer)
                        break;

                    ui.DrawImage(GameImages.ACTIVITY_TRACKING, gx, gy, tint);
                    break;

                case Activity.FLEEING:
                    if (actor.IsPlayer)
                        break;

                    ui.DrawImage(GameImages.ACTIVITY_FLEEING, gx, gy, tint);
                    break;

                case Activity.FLEEING_FROM_EXPLOSIVE:
                    if (actor.IsPlayer)
                        break;

                    ui.DrawImage(GameImages.ACTIVITY_FLEEING_FROM_EXPLOSIVE, gx, gy, tint);
                    break;

                case Activity.FOLLOWING:
                    if (actor.IsPlayer)
                        break;
                    if (actor.TargetActor == null)
                        break;

                    if (actor.TargetActor.IsPlayer)
                        ui.DrawImage(GameImages.ACTIVITY_FOLLOWING_PLAYER, gx, gy);
                    else if (actor.TargetActor == actor.Leader) // alpha10
                        ui.DrawImage(GameImages.ACTIVITY_FOLLOWING_LEADER, gx, gy);
                    else
                        ui.DrawImage(GameImages.ACTIVITY_FOLLOWING, gx, gy);
                    break;

                case Activity.FOLLOWING_ORDER:
                    ui.DrawImage(GameImages.ACTIVITY_FOLLOWING_ORDER, gx, gy);
                    break;

                case Activity.SLEEPING:
                    ui.DrawImage(GameImages.ACTIVITY_SLEEPING, gx, gy);
                    break;

                default:
                    throw new InvalidOperationException("unhandled activity " + actor.Activity);
            }

            // health bar.
            int maxHP = game.Rules.ActorMaxHPs(actor);
            if (actor.HitPoints < maxHP)
            {
                DrawMapHealthBar(actor.HitPoints, maxHP, gx, gy);
            }

            // run/tired icon.
            if (actor.IsRunning)
                ui.DrawImage(GameImages.ICON_RUNNING, gx, gy, tint);
            else if (actor.Model.Abilities.CanRun && !game.Rules.CanActorRun(actor))
                ui.DrawImage(GameImages.ICON_CANT_RUN, gx, gy, tint);

            // sleepy, hungry & insane icons.
            if (actor.Model.Abilities.HasToSleep)
            {
                if (game.Rules.IsActorExhausted(actor))
                    ui.DrawImage(GameImages.ICON_SLEEP_EXHAUSTED, gx, gy, tint);
                else if (game.Rules.IsActorSleepy(actor))
                    ui.DrawImage(GameImages.ICON_SLEEP_SLEEPY, gx, gy, tint);
                else if (game.Rules.IsAlmostSleepy(actor))
                    ui.DrawImage(GameImages.ICON_SLEEP_ALMOST_SLEEPY, gx, gy, tint);
            }

            if (actor.Model.Abilities.HasToEat)
            {
                if (game.Rules.IsActorStarving(actor))
                    ui.DrawImage(GameImages.ICON_FOOD_STARVING, gx, gy, tint);
                else if (game.Rules.IsActorHungry(actor))
                    ui.DrawImage(GameImages.ICON_FOOD_HUNGRY, gx, gy, tint);
                else if (game.IsAlmostHungry(actor))
                    ui.DrawImage(GameImages.ICON_FOOD_ALMOST_HUNGRY, gx, gy, tint);
            }
            else if (actor.Model.Abilities.IsRotting)
            {
                if (game.Rules.IsRottingActorStarving(actor))
                    ui.DrawImage(GameImages.ICON_ROT_STARVING, gx, gy, tint);
                else if (game.Rules.IsRottingActorHungry(actor))
                    ui.DrawImage(GameImages.ICON_ROT_HUNGRY, gx, gy, tint);
                else if (game.IsAlmostRotHungry(actor))
                    ui.DrawImage(GameImages.ICON_ROT_ALMOST_HUNGRY, gx, gy, tint);
            }

            if (actor.Model.Abilities.HasSanity)
            {
                if (game.Rules.IsActorInsane(actor))
                    ui.DrawImage(GameImages.ICON_SANITY_INSANE, gx, gy, tint);
                else if (game.Rules.IsActorDisturbed(actor))
                    ui.DrawImage(GameImages.ICON_SANITY_DISTURBED, gx, gy, tint);
            }

            // can trade with player icon.
            // alpha10.1 or has needed item (not for undead player duh)
            if (player != null)
            {
                if (actor != player && !player.Model.Abilities.IsUndead && ActorHasVitalItemForPlayer(actor))
                    ui.DrawImage(GameImages.ICON_HAS_VITAL_ITEM, gx, gy, tint);
                else if (game.Rules.CanActorInitiateTradeWith(player, actor))
                    ui.DrawImage(GameImages.ICON_CAN_TRADE, gx, gy, tint);
            }

            // alpha10 odor suppressed icon (will overlap with sleep healing but its fine)
            if (actor.OdorSuppressorCounter > 0)
                ui.DrawImage(GameImages.ICON_ODOR_SUPPRESSED, gx, gy, tint);

            // sleep-healing icon.
            if (actor.IsSleeping && (game.Rules.IsOnCouch(actor) || game.Rules.ActorHealChanceBonus(actor) > 0))
                ui.DrawImage(GameImages.ICON_HEALING, gx, gy, tint);

            // is a leader icon.
            if (actor.CountFollowers > 0)
                ui.DrawImage(GameImages.ICON_LEADER, gx, gy, tint);

            // alpha10
            // z-grab skill warning icon
            if (actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_GRAB) > 0)
                ui.DrawImage(GameImages.ICON_ZGRAB, gx, gy, tint);

            // combat assitant helper.
            if (RogueGame.Options.IsCombatAssistantOn)
            {
                if (actor != player && player != null && game.Rules.AreEnemies(actor, player))
                {
                    if (game.Rules.WillActorActAgainBefore(player, actor))
                        ui.DrawImage(GameImages.ICON_THREAT_SAFE, gx, gy, tint);
                    else if (game.Rules.WillOtherActTwiceBefore(player, actor))
                        ui.DrawImage(GameImages.ICON_THREAT_HIGH_DANGER, gx, gy, tint);
                    else
                        ui.DrawImage(GameImages.ICON_THREAT_DANGER, gx, gy, tint);
                }
            }
        }

        void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, Color tint)
        {
            List<string> decos = actor.Doll.GetDecorations(part);
            if (decos == null)
                return;

            foreach (string imageID in decos)
                ui.DrawImage(imageID, gx, gy, tint);
        }

        void DrawActorEquipment(Actor actor, int gx, int gy, DollPart part, Color tint)
        {
            Item it = actor.GetEquippedItem(part);
            if (it == null)
                return;

            ui.DrawImage(it.ImageID, gx, gy, tint);
        }

        void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy)
        {
            DrawMapHealthBar(hitPoints, maxHitPoints, gx, gy, Color.Red);
        }

        void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy, Color barColor)
        {
            int hpX = gx + 4;
            int hpY = gy + TILE_SIZE - 4;
            int barLength = (int)(20 * (float)hitPoints / (float)maxHitPoints);
            ui.FillRect(Color.Black, new Rectangle(hpX, hpY, 20, 4));
            if (barLength > 0)
                ui.FillRect(barColor, new Rectangle(hpX + 1, hpY + 1, barLength, 2));
        }

        /// <summary>
        /// Checks if npc has a vital item for the player :
        /// - Food if player is hungry
        /// - Anti-sleep meds if player is sleepy
        /// - Healing meds if player injured
        /// - Curing meds if player infected
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        bool ActorHasVitalItemForPlayer(Actor actor)
        {
            if (actor.Inventory == null)
                return false;
            if (actor.Inventory.IsEmpty)
                return false;

            // hungry -> food
            if (game.Rules.IsActorHungry(player)
                && actor.Inventory.HasItemOfType(typeof(ItemFood)))
                return true;

            // sleepy -> anti-sleep meds
            if (game.Rules.IsActorSleepy(player)
                && actor.Inventory.HasItemMatching((it) => it is ItemMedicine && (it as ItemMedicine).SleepBoost > 0))
                return true;

            // injured -> healing meds
            if (player.HitPoints < game.Rules.ActorMaxHPs(player)
                && actor.Inventory.HasItemMatching((it) => it is ItemMedicine && (it as ItemMedicine).Healing > 0))
                return true;

            // infected -> curing meds
            if (player.Infection > 0
                && actor.Inventory.HasItemMatching((it) => it is ItemMedicine && (it as ItemMedicine).InfectionCure > 0))
                return true;

            // no vital items
            return false;
        }

        string LocationText(Map map, Actor actor)
        {
            if (map == null || actor == null)
                return "";

            StringBuilder sb = new StringBuilder(string.Format("({0},{1}) ", actor.Location.Position.X, actor.Location.Position.Y));

            List<Zone> zones = map.GetZonesAt(actor.Location.Position.X, actor.Location.Position.Y);
            if (zones == null || zones.Count == 0)
                return sb.ToString();

            foreach (Zone z in zones)
                sb.Append(string.Format("{0} ", z.Name));

            return sb.ToString();
        }

        void DrawMiniMap(Map map)
        {
            // clear minimap.
            if (RogueGame.Options.IsMinimapOn)
            {
                ui.ClearMinimap(Color.Black);
            }

            // set visited tiles color.
            if (RogueGame.Options.IsMinimapOn)
            {
                Point pt = new Point();
                for (int x = 0; x < map.Width; x++)
                {
                    pt.X = x;
                    for (int y = 0; y < map.Height; y++)
                    {
                        pt.Y = y;
                        Tile tile = map.GetTileAt(x, y);
                        if (tile.IsVisited)
                        {
                            // exits override tile color.
                            if (map.GetExitAt(pt) != null)
                                ui.SetMinimapColor(x, y, Color.HotPink);
                            else
                                ui.SetMinimapColor(x, y, tile.Model.MinimapColor);
                        }
                    }
                }
            }

            // show minimap.
            if (RogueGame.Options.IsMinimapOn)
            {
                ui.DrawMinimap(MINIMAP_X, MINIMAP_Y);
            }

            // show view rect.
            ui.DrawRect(Color.White, new Rectangle(
                MINIMAP_X + game.m_MapViewRect.Left * MINITILE_SIZE, MINIMAP_Y + game.m_MapViewRect.Top * MINITILE_SIZE,
                game.m_MapViewRect.Width * MINITILE_SIZE, game.m_MapViewRect.Height * MINITILE_SIZE));

            // show player tags.
            if (RogueGame.Options.ShowPlayerTagsOnMinimap)
            {
                for (int x = 0; x < map.Width; x++)
                    for (int y = 0; y < map.Height; y++)
                    {
                        Tile tile = map.GetTileAt(x, y);
                        if (tile.IsVisited)
                        {
                            string minitag = null;
                            if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG1))
                                minitag = GameImages.MINI_PLAYER_TAG1;
                            else if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG2))
                                minitag = GameImages.MINI_PLAYER_TAG2;
                            else if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG3))
                                minitag = GameImages.MINI_PLAYER_TAG3;
                            else if (tile.HasDecoration(GameImages.DECO_PLAYER_TAG4))
                                minitag = GameImages.MINI_PLAYER_TAG4;
                            if (minitag != null)
                            {
                                Point pos = new Point(MINIMAP_X + x * MINITILE_SIZE, MINIMAP_Y + y * MINITILE_SIZE);
                                ui.DrawImage(minitag, pos.X - MINI_TRACKER_OFFSET, pos.Y - MINI_TRACKER_OFFSET);
                            }
                        }
                    }
            }

            // show player & tracked actors.
            // add tracked targets images out of player fov on the map.
            if (player != null)
            {
                // tracker items.
                if (!player.IsSleeping)
                {
                    ItemTracker tracker = player.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;

                    // tracking...
                    if (tracker != null && tracker.Batteries > 0)
                    {
                        // ...followers?
                        if (player.CountFollowers > 0 && tracker.CanTrackFollowersOrLeader)
                        {
                            foreach (Actor fo in player.Followers)
                            {
                                // only track in same map.
                                if (fo.Location.Map != player.Location.Map)
                                    continue;

                                ItemTracker foTracker = fo.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
                                if (foTracker != null && foTracker.CanTrackFollowersOrLeader)
                                {
                                    // show follower position.
                                    Point foMiniPos = new Point(MINIMAP_X + fo.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + fo.Location.Position.Y * MINITILE_SIZE);
                                    ui.DrawImage(GameImages.MINI_FOLLOWER_POSITION, foMiniPos.X - MINI_TRACKER_OFFSET, foMiniPos.Y - MINI_TRACKER_OFFSET);

                                    // if out of FoV but in view,, draw on map.
                                    if (game.IsInViewRect(fo.Location.Position) && !game.IsVisibleToPlayer(fo))
                                    {
                                        Point screenPos = game.MapToScreen(fo.Location.Position);
                                        ui.DrawImage(GameImages.TRACK_FOLLOWER_POSITION, screenPos.X, screenPos.Y);
                                    }
                                }
                            }
                        }

                        // ...undeads?
                        if (tracker.CanTrackUndeads)
                        {
                            foreach (Actor other in map.Actors)
                            {
                                if (other == player)
                                    continue;
                                if (!other.Model.Abilities.IsUndead)
                                    continue;
                                // only track in same map.
                                if (other.Location.Map != player.Location.Map)
                                    continue;
                                if (game.Rules.GridDistance(other.Location.Position, player.Location.Position) > Rules.ZTRACKINGRADIUS)
                                    continue;

                                // close undead, show it.
                                Point undeadPos = new Point(MINIMAP_X + other.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + other.Location.Position.Y * MINITILE_SIZE);
                                ui.DrawImage(GameImages.MINI_UNDEAD_POSITION, undeadPos.X - MINI_TRACKER_OFFSET, undeadPos.Y - MINI_TRACKER_OFFSET);

                                // if out of FoV but in view, draw on map.
                                if (game.IsInViewRect(other.Location.Position) && !game.IsVisibleToPlayer(other))
                                {
                                    Point screenPos = game.MapToScreen(other.Location.Position);
                                    ui.DrawImage(GameImages.TRACK_UNDEAD_POSITION, screenPos.X, screenPos.Y);
                                }
                            }
                        }

                        // ...BlackOps?
                        if (tracker.CanTrackBlackOps)
                        {
                            foreach (Actor other in map.Actors)
                            {
                                if (other == player)
                                    continue;
                                if (other.Faction != game.Factions.TheBlackOps)
                                    continue;
                                // only track in same map.
                                if (other.Location.Map != player.Location.Map)
                                    continue;

                                // blackop, show it.
                                Point boPos = new Point(MINIMAP_X + other.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + other.Location.Position.Y * MINITILE_SIZE);
                                ui.DrawImage(GameImages.MINI_BLACKOPS_POSITION, boPos.X - MINI_TRACKER_OFFSET, boPos.Y - MINI_TRACKER_OFFSET);

                                // if out of FoV but in view, draw on map.
                                if (game.IsInViewRect(other.Location.Position) && !game.IsVisibleToPlayer(other))
                                {
                                    Point screenPos = game.MapToScreen(other.Location.Position);
                                    ui.DrawImage(GameImages.TRACK_BLACKOPS_POSITION, screenPos.X, screenPos.Y);
                                }
                            }
                        }

                        // ...Police?
                        if (tracker.CanTrackPolice)
                        {
                            foreach (Actor other in map.Actors)
                            {
                                if (other == player)
                                    continue;
                                if (other.Faction != game.Factions.ThePolice)
                                    continue;
                                // only track in same map.
                                if (other.Location.Map != player.Location.Map)
                                    continue;

                                // policeman, show it.
                                Point boPos = new Point(MINIMAP_X + other.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + other.Location.Position.Y * MINITILE_SIZE);
                                ui.DrawImage(GameImages.MINI_POLICE_POSITION, boPos.X - MINI_TRACKER_OFFSET, boPos.Y - MINI_TRACKER_OFFSET);

                                // if out of FoV but in view, draw on map.
                                if (game.IsInViewRect(other.Location.Position) && !game.IsVisibleToPlayer(other))
                                {
                                    Point screenPos = game.MapToScreen(other.Location.Position);
                                    ui.DrawImage(GameImages.TRACK_POLICE_POSITION, screenPos.X, screenPos.Y);
                                }
                            }
                        }
                    }
                }

                // player.
                Point pos = new Point(MINIMAP_X + player.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + player.Location.Position.Y * MINITILE_SIZE);
                ui.DrawImage(GameImages.MINI_PLAYER_POSITION, pos.X - MINI_TRACKER_OFFSET, pos.Y - MINI_TRACKER_OFFSET);
            }
        }

        void DrawActorStatus(Actor actor, int gx, int gy)
        {
            // 1. Name & occupation
            ui.DrawStringBold(actor.IsInvincible ? Color.LightGreen : Color.White, string.Format("{0}, {1}", actor.Name, actor.Faction.MemberName), gx, gy);

            // 2. Bars: Health, Stamina, Food, Sleep, Infection.
            gy += Ui.BOLD_LINE_SPACING;
            int maxHP = game.Rules.ActorMaxHPs(actor);
            ui.DrawStringBold(Color.White, string.Format("HP  {0}", actor.HitPoints), gx, gy);
            DrawBar(actor.HitPoints, actor.PreviousHitPoints, maxHP, 0, 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Red, Color.DarkRed, Color.OrangeRed, Color.Gray);
            ui.DrawStringBold(Color.White, string.Format("{0}", maxHP), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);

            gy += Ui.BOLD_LINE_SPACING;
            if (actor.Model.Abilities.CanTire)
            {
                int maxSTA = game.Rules.ActorMaxSTA(actor);
                ui.DrawStringBold(Color.White, string.Format("STA {0}", actor.StaminaPoints), gx, gy);
                DrawBar(actor.StaminaPoints, actor.PreviousStaminaPoints, maxSTA, Rules.STAMINA_MIN_FOR_ACTIVITY, 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Green, Color.DarkGreen, Color.LightGreen, Color.Gray);
                ui.DrawStringBold(Color.White, string.Format("{0}", maxSTA), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);
                if (actor.IsRunning)
                    ui.DrawStringBold(Color.LightGreen, "RUNNING!", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                else if (game.Rules.CanActorRun(actor))
                    ui.DrawStringBold(Color.Green, "can run", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                else if (game.Rules.IsActorTired(actor))
                    ui.DrawStringBold(Color.Gray, "TIRED", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
            }

            gy += Ui.BOLD_LINE_SPACING;
            if (actor.Model.Abilities.HasToEat)
            {
                int maxFood = game.Rules.ActorMaxFood(actor);
                ui.DrawStringBold(Color.White, string.Format("FOO {0}", actor.FoodPoints), gx, gy);
                DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxFood, Rules.FOOD_HUNGRY_LEVEL, 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
                ui.DrawStringBold(Color.White, string.Format("{0}", maxFood), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);
                if (game.Rules.IsActorHungry(actor))
                {
                    if (game.Rules.IsActorStarving(actor))
                        ui.DrawStringBold(Color.Red, "STARVING!", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        ui.DrawStringBold(Color.Yellow, "Hungry", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    ui.DrawStringBold(Color.White, string.Format("{0}h", game.FoodToHoursUntilHungry(actor.FoodPoints)), gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
            }
            else if (actor.Model.Abilities.IsRotting)
            {
                int maxFood = game.Rules.ActorMaxRot(actor);
                ui.DrawStringBold(Color.White, string.Format("ROT {0}", actor.FoodPoints), gx, gy);
                DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxFood, Rules.ROT_HUNGRY_LEVEL, 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
                ui.DrawStringBold(Color.White, string.Format("{0}", maxFood), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);
                if (game.Rules.IsRottingActorHungry(actor))
                {
                    if (game.Rules.IsRottingActorStarving(actor))
                        ui.DrawStringBold(Color.Red, "STARVING!", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        ui.DrawStringBold(Color.Yellow, "Hungry", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    ui.DrawStringBold(Color.White, string.Format("{0}h", game.FoodToHoursUntilRotHungry(actor.FoodPoints)), gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
            }

            gy += Ui.BOLD_LINE_SPACING;
            if (actor.Model.Abilities.HasToSleep)
            {
                int maxSleep = game.Rules.ActorMaxSleep(actor);
                ui.DrawStringBold(Color.White, string.Format("SLP {0}", actor.SleepPoints), gx, gy);
                DrawBar(actor.SleepPoints, actor.PreviousSleepPoints, maxSleep, Rules.SLEEP_SLEEPY_LEVEL, 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Blue, Color.DarkBlue, Color.LightBlue, Color.Gray);
                ui.DrawStringBold(Color.White, string.Format("{0}", maxSleep), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);
                if (game.Rules.IsActorSleepy(actor))
                {
                    if (game.Rules.IsActorExhausted(actor))
                        ui.DrawStringBold(Color.Red, "EXHAUSTED!", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        ui.DrawStringBold(Color.Yellow, "Sleepy", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    ui.DrawStringBold(Color.White, string.Format("{0}h", game.Rules.SleepToHoursUntilSleepy(actor.SleepPoints, session.WorldTime.IsNight)), gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
            }

            gy += Ui.BOLD_LINE_SPACING;
            if (actor.Model.Abilities.HasSanity)
            {
                int maxSan = game.Rules.ActorMaxSanity(actor);
                ui.DrawStringBold(Color.White, string.Format("SAN {0}", actor.Sanity), gx, gy);
                DrawBar(actor.Sanity, actor.PreviousSanity, maxSan, game.Rules.ActorDisturbedLevel(actor), 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Orange, Color.DarkOrange, Color.OrangeRed, Color.Gray);
                ui.DrawStringBold(Color.White, string.Format("{0}", maxSan), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);
                if (game.Rules.IsActorDisturbed(actor))
                {
                    if (game.Rules.IsActorInsane(actor))
                        ui.DrawStringBold(Color.Red, "INSANE!", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                    else
                        ui.DrawStringBold(Color.Yellow, "Disturbed", gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
                }
                else
                    ui.DrawStringBold(Color.White, string.Format("{0}h", game.Rules.SanityToHoursUntilUnstable(actor)), gx + Ui.BOLD_LINE_SPACING * 9 + 100, gy);
            }

            if (Rules.HasInfection(session.GameMode) && !actor.Model.Abilities.IsUndead)
            {
                int maxInf = game.Rules.ActorInfectionHPs(actor);
                int refInf = (Rules.INFECTION_LEVEL_1_WEAK * maxInf) / 100;
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, string.Format("INF {0}", actor.Infection), gx, gy);
                DrawBar(actor.Infection, actor.Infection, maxInf, refInf, 100, Ui.BOLD_LINE_SPACING, gx + Ui.BOLD_LINE_SPACING * 5, gy, Color.Purple, Color.Black, Color.Black, Color.Gray);
                ui.DrawStringBold(Color.White, string.Format("{0}%", game.Rules.ActorInfectionPercent(actor)), gx + Ui.BOLD_LINE_SPACING * 6 + 100, gy);
            }

            // 3. Melee & Ranged Attacks.
            gy += Ui.BOLD_LINE_SPACING;
            Attack melee = game.Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, null);
            int dmgBonusVsUndead = game.Rules.ActorDamageBonusVsUndeads(actor);
            ui.DrawStringBold(Color.White, string.Format("Melee  Atk {0:D2}  Dmg {1:D2}/{2:D2}", melee.HitValue, melee.DamageValue, melee.DamageValue + dmgBonusVsUndead), gx, gy);

            gy += Ui.BOLD_LINE_SPACING;
            Attack ranged = game.Rules.ActorRangedAttack(actor, actor.CurrentRangedAttack, actor.CurrentRangedAttack.EfficientRange, null);
            ItemRangedWeapon rangedWeapon = actor.GetEquippedWeapon() as ItemRangedWeapon;
            int ammo, maxAmmo;
            ammo = maxAmmo = 0;
            if (rangedWeapon != null)
            {
                ammo = rangedWeapon.Ammo;
                maxAmmo = (rangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo;
                ui.DrawStringBold(Color.White, string.Format("Ranged Atk {0:D2}  Dmg {1:D2}/{2:D2} Rng {3}-{4} Amo {5}/{6}",
                    ranged.HitValue, ranged.DamageValue, ranged.DamageValue + dmgBonusVsUndead, ranged.Range, ranged.EfficientRange, ammo, maxAmmo), gx, gy);
            }

            // 4. (living)Def, Pro, Spd, FoV & Nb of followers / (undead)Def, Spd, Fov, Sml, Kills
            gy += Ui.BOLD_LINE_SPACING;
            Defence defence = game.Rules.ActorDefence(actor, actor.CurrentDefence);

            if (actor.Model.Abilities.IsUndead)
            {
                ui.DrawStringBold(Color.White, string.Format("Def {0:D2} Spd {1:F2} FoV {2} Sml {3:F2} Kills {4}",
                    defence.Value,
                    (float)game.Rules.ActorSpeed(actor) / (float)Rules.BASE_SPEED,
                    game.Rules.ActorFOV(actor, session.WorldTime, session.World.Weather),
                    game.Rules.ActorSmell(actor),
                    actor.KillsCount),
                    gx, gy);
            }
            else
            {
                ui.DrawStringBold(Color.White, string.Format("Def {0:D2} Arm {1:D1}/{2:D1} Spd {3:F2} FoV {4}/{5} Fol {6}/{7}",
                    defence.Value, defence.Protection_Hit, defence.Protection_Shot,
                    (float)game.Rules.ActorSpeed(actor) / (float)Rules.BASE_SPEED,
                    game.Rules.ActorFOV(actor, session.WorldTime, session.World.Weather),
                    actor.Sheet.BaseViewRange,
                    actor.CountFollowers, game.Rules.ActorMaxFollowers(actor)),
                    gx, gy);
            }

            // 5. Odor suppressor // alpha10
            gy += Ui.BOLD_LINE_SPACING;
            if (actor.OdorSuppressorCounter > 0)
                ui.DrawStringBold(Color.LightBlue, string.Format("Odor suppr : {0} -{1}", actor.OdorSuppressorCounter, game.Rules.OdorsDecay(actor.Location.Map, actor.Location.Position, session.World.Weather)), gx, gy);
        }

        void DrawInventory(Inventory inventory, string title, bool drawSlotsNumbers, int slotsPerLine, int maxSlots, int gx, int gy)
        {
            int x, y;
            int slot = 0;

            // Draw title.
            gy -= Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, title, gx, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // Draw slots.
            x = gx; y = gy; slot = 0;
            for (int i = 0; i < maxSlots; i++)
            {
                ui.DrawImage(GameImages.ITEM_SLOT, x, y);
                if (++slot >= slotsPerLine)
                {
                    slot = 0;
                    y += TILE_SIZE;
                    x = gx;
                }
                else
                    x += TILE_SIZE;
            }

            // Draw items.
            if (inventory == null)
                return;

            x = gx; y = gy; slot = 0;
            foreach (Item it in inventory.Items)
            {
                if (it.IsEquipped)
                    ui.DrawImage(GameImages.ITEM_EQUIPPED, x, y);
                if (it is ItemRangedWeapon)
                {
                    ItemRangedWeapon w = it as ItemRangedWeapon;
                    if (w.Ammo <= 0)
                        ui.DrawImage(GameImages.ICON_OUT_OF_AMMO, x, y);
                    DrawBar(w.Ammo, w.Ammo, (w.Model as ItemRangedWeaponModel).MaxAmmo, 0, 28, 3, x + 2, y + 27, Color.Blue, Color.Blue, Color.Blue, Color.DarkGray);
                }
                else if (it is ItemSprayPaint)
                {
                    ItemSprayPaint sp = it as ItemSprayPaint;
                    DrawBar(sp.PaintQuantity, sp.PaintQuantity, (sp.Model as ItemSprayPaintModel).MaxPaintQuantity, 0, 28, 3, x + 2, y + 27, Color.Gold, Color.Gold, Color.Gold, Color.DarkGray);
                }
                else if (it is ItemSprayScent)
                {
                    ItemSprayScent sp = it as ItemSprayScent;
                    DrawBar(sp.SprayQuantity, sp.SprayQuantity, (sp.Model as ItemSprayScentModel).MaxSprayQuantity, 0, 28, 3, x + 2, y + 27, Color.Cyan, Color.Cyan, Color.Cyan, Color.DarkGray);
                }
                else if (it is ItemLight)
                {
                    ItemLight lt = it as ItemLight;
                    if (lt.Batteries <= 0)
                        ui.DrawImage(GameImages.ICON_OUT_OF_BATTERIES, x, y);
                    DrawBar(lt.Batteries, lt.Batteries, (lt.Model as ItemLightModel).MaxBatteries, 0, 28, 3, x + 2, y + 27, Color.Yellow, Color.Yellow, Color.Yellow, Color.DarkGray);
                }
                else if (it is ItemTracker)
                {
                    ItemTracker tr = it as ItemTracker;
                    if (tr.Batteries <= 0)
                        ui.DrawImage(GameImages.ICON_OUT_OF_BATTERIES, x, y);
                    DrawBar(tr.Batteries, tr.Batteries, (tr.Model as ItemTrackerModel).MaxBatteries, 0, 28, 3, x + 2, y + 27, Color.Pink, Color.Pink, Color.Pink, Color.DarkGray);
                }
                else if (it is ItemFood)
                {
                    ItemFood food = it as ItemFood;
                    if (game.Rules.IsFoodExpired(food, session.WorldTime.TurnCounter))
                        ui.DrawImage(GameImages.ICON_EXPIRED_FOOD, x, y);
                    else if (game.Rules.IsFoodSpoiled(food, session.WorldTime.TurnCounter))
                        ui.DrawImage(GameImages.ICON_SPOILED_FOOD, x, y);
                }
                else if (it is ItemTrap)
                {
                    ItemTrap trap = it as ItemTrap;
                    DrawTrapItem(trap, x, y);  // alpha10
                }
                else if (it is ItemEntertainment)
                {
                    if (player != null && ((it as ItemEntertainment).IsBoringFor(player))) // alpha10 boring items item centric
                        ui.DrawImage(GameImages.ICON_BORING_ITEM, x, y);
                }
                DrawItem(it, x, y);

                if (++slot >= slotsPerLine)
                {
                    slot = 0;
                    y += TILE_SIZE;
                    x = gx;
                }
                else
                    x += TILE_SIZE;
            }

            // Draw slots numbers.
            if (drawSlotsNumbers)
            {
                x = gx + 4; y = gy + TILE_SIZE;
                for (int i = 0; i < inventory.MaxCapacity; i++)
                {
                    ui.DrawString(Color.White, (i + 1).ToString(), x, y);
                    x += TILE_SIZE;
                }

            }
        }

        void DrawItem(Item it, int gx, int gy)
        {
            DrawItem(it, gx, gy, Color.White);
        }

        void DrawItem(Item it, int gx, int gy, Color tint)
        {
            ui.DrawImage(it.ImageID, gx, gy, tint);

            if (it.Model.IsStackable)
            {
                string q = string.Format("{0}", it.Quantity);
                int tx = gx + TILE_SIZE - 10;
                if (it.Quantity > 100)
                    tx -= 10;
                else if (it.Quantity > 10)
                    tx -= 4;
                ui.DrawString(Color.DarkGray, q, tx + 1, gy + 1);
                ui.DrawString(Color.White, q, tx, gy);
            }
            if (it is ItemTrap)
            {
                DrawTrapItem(it as ItemTrap, gx, gy);
            }
        }

        void DrawTrapItem(ItemTrap trap, int gx, int gy)
        {
            if (trap.IsTriggered)
            {
                if (trap.Owner == player)
                    ui.DrawImage(GameImages.ICON_TRAP_TRIGGERED_SAFE_PLAYER, gx, gy);
                else if (game.Rules.IsSafeFromTrap(trap, player))
                    ui.DrawImage(GameImages.ICON_TRAP_TRIGGERED_SAFE_GROUP, gx, gy);
                else
                    ui.DrawImage(GameImages.ICON_TRAP_TRIGGERED, gx, gy);
            }
            else if (trap.IsActivated)
            {
                if (trap.Owner == player)
                    ui.DrawImage(GameImages.ICON_TRAP_ACTIVATED_SAFE_PLAYER, gx, gy);
                else if (game.Rules.IsSafeFromTrap(trap, player))
                    ui.DrawImage(GameImages.ICON_TRAP_ACTIVATED_SAFE_GROUP, gx, gy);
                else
                    ui.DrawImage(GameImages.ICON_TRAP_ACTIVATED, gx, gy);
            }
        }

        void DrawBar(int value, int previousValue, int maxValue, int refValue, int maxWidth, int height, int gx, int gy,
            Color fillColor, Color lossFillColor, Color gainFillColor, Color emptyColor)
        {
            ui.FillRect(emptyColor, new Rectangle(gx, gy, maxWidth, height));

            int prevBarLength = (int)(maxWidth * (float)previousValue / (float)maxValue);
            int barLength = (int)(maxWidth * (float)value / (float)maxValue);

            if (value > previousValue)
            {
                // gain
                if (barLength > 0)
                    ui.FillRect(gainFillColor, new Rectangle(gx, gy, barLength, height));
                if (prevBarLength > 0)
                    ui.FillRect(fillColor, new Rectangle(gx, gy, prevBarLength, height));
            }
            else if (value < previousValue)
            {
                // loss
                if (prevBarLength > 0)
                    ui.FillRect(lossFillColor, new Rectangle(gx, gy, prevBarLength, height));
                if (barLength > 0)
                    ui.FillRect(fillColor, new Rectangle(gx, gy, barLength, height));
            }
            else
            {
                // no change.
                if (barLength > 0)
                    ui.FillRect(fillColor, new Rectangle(gx, gy, barLength, height));
            }

            // reference line.
            int refLength = (int)(maxWidth * (float)refValue / (float)maxValue);
            ui.DrawLine(Color.White, gx + refLength, gy, gx + refLength, gy + height);
        }

        void DrawActorSkillTable(Actor actor, int gx, int gy)
        {
            gy -= Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Skills", gx, gy);
            gy += Ui.BOLD_LINE_SPACING;

            IEnumerable<Skill> skills = actor.Sheet.SkillTable.Skills;
            if (skills == null)
                return;

            int x, y;
            int count = 0;
            x = gx; y = gy;
            foreach (Skill sk in skills)
            {
                Color skColor = Color.White;

                // alpha10 highlight if active skills are active or not
                switch (sk.ID)
                {
                    case (int)Skills.IDs.MARTIAL_ARTS:
                        skColor = (actor.GetEquippedWeapon() == null ? Color.LightGreen : Color.Red);
                        break;
                    case (int)Skills.IDs.HARDY:
                        if (actor.IsSleeping) skColor = Color.LightGreen;
                        break;
                }

                ui.DrawString(skColor, string.Format("{0}-", sk.Level), x, y);
                x += 16;
                ui.DrawString(skColor, Skills.Name(sk.ID), x, y);
                x -= 16;

                if (++count >= SKILLTABLE_LINES)
                {
                    count = 0;
                    y = gy;
                    x += 120;
                }
                else
                    y += Ui.LINE_SPACING;
            }
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();

            if (wait != WaitFor.None && key == Key.Enter)
            {
                if (wait == WaitFor.AdvisorInfo)
                    ShowWelcomeInfo();
                else if (wait == WaitFor.WelcomeInfo)
                    AfterWelcome();
            }
        }

        void AfterWelcome()
        {
            // wake up!
            msgs.Clear();
            AddMessage(new Message(string.Format(player.IsUndead ? "{0} rises..." : "{0} wakes up.", player.Name), 0, Color.White));

            // reset/cleanup bot from previous session
            // !BOT
            /*#if DEBUG
                        if (m_isBotMode)
                            BotReleaseControl();
            #endif*/

            // start simulation thread.
            game.StopSimThread(false);
            game.StartSimThread();
        }
    }
}
