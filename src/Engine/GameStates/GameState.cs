using RogueSurvivor.Data;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSurvivor.Extensions;
using System.Threading;
using RogueSurvivor.Engine.Interfaces;

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

        const int LOCATIONPANEL_X = RIGHTPANEL_X;
        const int LOCATIONPANEL_Y = MESSAGES_Y;
        const int LOCATIONPANEL_TEXT_X = LOCATIONPANEL_X + 4;
        const int LOCATIONPANEL_TEXT_Y = LOCATIONPANEL_Y + 4;

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

        public override void Enter()
        {
            session = game.Session;
            player = game.Player;
            bool isUndead = session.charGen.IsUndead;

            // scoring : hello there.
            session.Scoring.AddVisit(session.WorldTime.TurnCounter, player.Location.Map);
            session.Scoring.AddEvent(session.WorldTime.TurnCounter, string.Format(isUndead ? "Rose in {0}." : "Woke up in {0}.", player.Location.Map.Name));

            // setup proper scoring mode.
            session.Scoring.Side = (isUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR);

            // schedule first autosave.
            game.ScheduleNextAutoSave();

            // advisor on?
            if (RogueGame.Options.IsAdvisorEnabled)
            {
                
            }
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
            if (player.Model.Abilities.IsUndead)
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
                //DrawMap(session.CurrentMap, mapTint);

                //ui.DrawLine(Color.DarkGray, RIGHTPANEL_X, MINIMAP_Y - 4, Ui.CANVAS_WIDTH, MINIMAP_Y - 4);
                //DrawMiniMap(session.CurrentMap);

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
               // if (player != null)
                //    DrawActorStatus(player, RIGHTPANEL_TEXT_X, RIGHTPANEL_TEXT_Y);

                // inventories.
                /*if (player != null)
                {
                    if (player.Inventory != null && player.Model.Abilities.HasInventory)
                        DrawInventory(player.Inventory, "Inventory", true, INVENTORY_SLOTS_PER_LINE, player.Inventory.MaxCapacity, INVENTORYPANEL_X, INVENTORYPANEL_Y);
                    DrawInventory(player.Location.Map.GetItemsAt(player.Location.Position), "Items on ground", true, INVENTORY_SLOTS_PER_LINE, Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y);
                    DrawCorpsesList(player.Location.Map.GetCorpsesAt(player.Location.Position), "Corpses on ground", INVENTORY_SLOTS_PER_LINE, INVENTORYPANEL_X, CORPSESPANEL_Y);
                }*/

                // character skills.
                //if (player != null && player.Sheet.SkillTable != null && player.Sheet.SkillTable.CountSkills > 0)
                //    DrawActorSkillTable(player, RIGHTPANEL_TEXT_X, SKILLTABLE_Y);

                // overlays
                /*Monitor.Enter(m_Overlays);
                foreach (Overlay o in m_Overlays)
                    o.Draw(ui);
                Monitor.Exit(m_Overlays);*/

                // DEV STATS
#if DEBUG
                /*if (game.Options.DEV_ShowActorsStats)
                {
                    int countLiving, countUndead;
                    countLiving = CountLivings(session.CurrentMap);
                    countUndead = CountUndeads(session.CurrentMap);
                    ui.DrawString(Color.White, string.Format("Living {0} vs {1} Undead", countLiving, countUndead), RIGHTPANEL_TEXT_X, SKILLTABLE_Y - 32);
                }*/
#endif
            }

            // release mutex.
            Monitor.Exit(ui);
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

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();

            if (wait != WaitFor.None)
            {
                if (wait == WaitFor.AdvisorInfo)
                    ShowWelcomeInfo();
            }
        }
    }
}
