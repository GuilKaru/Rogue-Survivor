using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using System;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class CityInfoState : GameState
    {
        public override void Draw()
        {
            int gx, gy;

            gx = gy = 0;
            ui.Clear(Color.Black);
            ui.DrawStringBold(Color.White, "CITY INFORMATION", gy, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;

            /////////////////////
            // Undead : no info!
            // Living : normal.
            /////////////////////
            if (game.Player.IsUndead)
            {
                ui.DrawStringBold(Color.Red, "You can't remember where you are...", gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.Red, "Must be that rotting brain of yours...", gx, gy);
                gy += 2 * Ui.BOLD_LINE_SPACING;
            }
            else
            {
                ////////////
                // City map
                ////////////
                ui.DrawStringBold(Color.White, "> DISTRICTS LAYOUT", gx, gy);
                gy += Ui.BOLD_LINE_SPACING;

                // coordinates.
                gy += Ui.BOLD_LINE_SPACING;
                for (int y = 0; y < game.Session.World.Size; y++)
                {
                    Color color = (y == game.Player.Location.Map.District.WorldPosition.Y ? Color.LightGreen : Color.White);
                    ui.DrawStringBold(color, y.ToString(), 20, gy + y * 3 * Ui.BOLD_LINE_SPACING + Ui.BOLD_LINE_SPACING);
                    ui.DrawStringBold(color, ".", 20, gy + y * 3 * Ui.BOLD_LINE_SPACING);
                    ui.DrawStringBold(color, ".", 20, gy + y * 3 * Ui.BOLD_LINE_SPACING + 2 * Ui.BOLD_LINE_SPACING);
                }
                gy -= Ui.BOLD_LINE_SPACING;
                for (int x = 0; x < game.Session.World.Size; x++)
                {
                    Color color = (x == game.Player.Location.Map.District.WorldPosition.X ? Color.LightGreen : Color.White);
                    ui.DrawStringBold(color, string.Format("..{0}..", (char)('A' + x)), 32 + x * 48, gy);
                }
                // districts.
                gy += Ui.BOLD_LINE_SPACING;
                int mx = 32;
                int my = gy;
                for (int y = 0; y < game.Session.World.Size; y++)
                    for (int x = 0; x < game.Session.World.Size; x++)
                    {
                        District d = game.Session.World[x, y];
                        char dStatus = d == game.Session.CurrentMap.District ? '*' : game.Session.Scoring.HasVisited(d.EntryMap) ? '-' : '?';
                        Color dColor;
                        string dChar;
                        switch (d.Kind)
                        {
                            case DistrictKind.BUSINESS: dColor = Color.Red; dChar = "Bus"; break;
                            case DistrictKind.GENERAL: dColor = Color.Gray; dChar = "Gen"; break;
                            case DistrictKind.GREEN: dColor = Color.Green; dChar = "Gre"; break;
                            case DistrictKind.RESIDENTIAL: dColor = Color.Orange; dChar = "Res"; break;
                            case DistrictKind.SHOPPING: dColor = Color.White; dChar = "Sho"; break;
                            default:
                                throw new ArgumentOutOfRangeException("unhandled district kind");
                        }

                        string lchar = "";
                        for (int i = 0; i < 5; i++)
                            lchar += dStatus;
                        Color lColor = (d == game.Player.Location.Map.District ? Color.LightGreen : dColor);

                        ui.DrawStringBold(lColor, lchar, mx + x * 48, my + (y * 3) * Ui.BOLD_LINE_SPACING);
                        ui.DrawStringBold(lColor, dStatus.ToString(), mx + x * 48, my + (y * 3 + 1) * Ui.BOLD_LINE_SPACING);
                        ui.DrawStringBold(dColor, dChar, mx + x * 48 + 8, my + (y * 3 + 1) * Ui.BOLD_LINE_SPACING);
                        ui.DrawStringBold(lColor, dStatus.ToString(), mx + x * 48 + 4 * 8, my + (y * 3 + 1) * Ui.BOLD_LINE_SPACING);
                        ui.DrawStringBold(lColor, lchar, mx + x * 48, my + (y * 3 + 2) * Ui.BOLD_LINE_SPACING);
                    }
                // subway line.
                const string subwayChar = "=";
                int subwayY = game.Session.World.Size / 2;
                for (int x = 1; x < game.Session.World.Size; x++)
                {
                    ui.DrawStringBold(Color.White, subwayChar, mx + x * 48 - 8, my + (subwayY * 3) * Ui.BOLD_LINE_SPACING + Ui.BOLD_LINE_SPACING);
                }

                gy += (game.Session.World.Size * 3 + 1) * Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, "Legend", gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawString(Color.White, "  *   - current     ?   - unvisited", gx, gy);
                gy += Ui.LINE_SPACING;
                ui.DrawString(Color.White, "  Bus - Business    Gen - General    Gre - Green", gx, gy);
                gy += Ui.LINE_SPACING;
                ui.DrawString(Color.White, "  Res - Residential Sho - Shopping", gx, gy);
                gy += Ui.LINE_SPACING;
                ui.DrawString(Color.White, "  =   - Subway Line", gx, gy);
                gy += Ui.LINE_SPACING;

                /////////////////////
                // Notable locations
                /////////////////////
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, "> NOTABLE LOCATIONS", gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
                int buildingsY = gy;
                for (int y = 0; y < game.Session.World.Size; y++)
                    for (int x = 0; x < game.Session.World.Size; x++)
                    {
                        District d = game.Session.World[x, y];
                        Map map = d.EntryMap;

                        // Subway station?
                        Zone subwayZone;
                        if ((subwayZone = map.GetZoneByPartialName(RogueGame.NAME_SUBWAY_STATION)) != null)
                        {
                            ui.DrawStringBold(Color.Blue, string.Format("at {0} : {1}.", World.CoordToString(x, y), subwayZone.Name), gx, gy);
                            gy += Ui.BOLD_LINE_SPACING;
                            if (gy >= Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * Ui.BOLD_LINE_SPACING;
                            }
                        }

                        // Police station?
                        if (map == game.Session.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District.EntryMap)
                        {
                            ui.DrawStringBold(Color.CadetBlue, string.Format("at {0} : Police Station.", World.CoordToString(x, y)), gx, gy);
                            gy += Ui.BOLD_LINE_SPACING;
                            if (gy >= Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * Ui.BOLD_LINE_SPACING;
                            }
                        }

                        // Hospital?
                        if (map == game.Session.UniqueMaps.Hospital_Admissions.TheMap.District.EntryMap)
                        {
                            ui.DrawStringBold(Color.White, string.Format("at {0} : Hospital.", World.CoordToString(x, y)), gx, gy);
                            gy += Ui.BOLD_LINE_SPACING;
                            if (gy >= Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * Ui.BOLD_LINE_SPACING;
                            }
                        }

                        // Secrets
                        // - CHAR Underground Facility?
                        if (game.Session.PlayerKnows_CHARUndergroundFacilityLocation && map == game.Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap)
                        {
                            ui.DrawStringBold(Color.Red, string.Format("at {0} : {1}.", World.CoordToString(x, y), game.Session.UniqueMaps.CHARUndergroundFacility.TheMap.Name), gx, gy);
                            gy += Ui.BOLD_LINE_SPACING;
                            if (gy >= Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * Ui.BOLD_LINE_SPACING;
                            }
                        }
                        // - The Sewers Thing?
                        if (game.Session.PlayerKnows_TheSewersThingLocation &&
                            map == game.Session.UniqueActors.TheSewersThing.TheActor.Location.Map.District.EntryMap &&
                            !game.Session.UniqueActors.TheSewersThing.TheActor.IsDead)
                        {
                            ui.DrawStringBold(Color.Red, string.Format("at {0} : The Sewers Thing lives down there.", World.CoordToString(x, y)), gx, gy);
                            gy += Ui.BOLD_LINE_SPACING;
                            if (gy >= Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING)
                            {
                                gy = buildingsY;
                                gx += 25 * Ui.BOLD_LINE_SPACING;
                            }
                        }
                    }
            }

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
