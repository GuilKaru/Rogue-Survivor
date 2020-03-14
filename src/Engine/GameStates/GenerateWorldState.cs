using RogueSurvivor.Data;
using RogueSurvivor.Gameplay;
using RogueSurvivor.Gameplay.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSurvivor.Engine.GameStates
{
    class GenerateWorldState : LoadScreenState
    {
        BaseTownGenerator townGenerator;
        World world;

        public override void Init()
        {
            BaseTownGenerator.Parameters genParams = BaseTownGenerator.DEFAULT_PARAMS;
            genParams.MapWidth = genParams.MapHeight = 100;
            Logger.WriteLine(Logger.Stage.INIT, "creating Generator");
            townGenerator = new StdTownGenerator(this, genParams);
        }

        public override void Enter()
        {
            // Create blank world
            Category("Creating empty world...", CreateEmptyWorld);

            // Create districts maps
            CategoryStart("Creating districts...");
            for (int x = 0; x < world.Size; x++)
            {
                for (int y = 0; y < world.Size; y++)
                    Action(() => CreateDistrict(x, y));
            }
            CategoryEnd();

            // Unique Maps
            Category("Generating unique maps...",
                () => game.Session.UniqueMaps.CHARUndergroundFacility = CreateUniqueMap_CHARUndegroundFacility(world));

            // Unique Actors
            Category("Generating unique actors...", GenerateUniqueActors);

            // Link districts
            CategoryStart("Linking districts...");
            for (int x = 0; x < world.Size; x++)
            {
                for (int y = 0; y < world.Size; y++)
                    Action(() => LinkDistrict(x, y));
            }
            CategoryEnd();

            // Finishing
            CategoryStart("Finishing...");
            Action(SpawnUniqueItems);
            Action(SpawnSpecialDecorations);
            Action(SpawnPlayer);
            Action(RevealStartingMap);
            CategoryEnd();
        }

        void CreateEmptyWorld()
        {
            //////////////////////
            // Create blank world
            //////////////////////
            world = new World(game.Options.CitySize);
            game.Session.World = world;

            ////////////////////////
            // Roll initial weather
            ////////////////////////
            world.Weather = (Weather)game.Rules.Roll((int)Weather._FIRST, (int)Weather._COUNT);
            world.NextWeatherCheckTurn = game.Rules.Roll(WEATHER_MIN_DURATION, WEATHER_MAX_DURATION);  // alpha10

            //////////////////////////////////////////////
            // Roll locations of special buildings.
            // Only ONE special building max per district.
            //////////////////////////////////////////////
            List<Point> noSpecialDistricts = new List<Point>();
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                    noSpecialDistricts.Add(new Point(x, y));

            Point policeStationDistrictPos = noSpecialDistricts[game.Rules.Roll(0, noSpecialDistricts.Count)];
            noSpecialDistricts.Remove(policeStationDistrictPos);

            Point hospitalDistrictPos = noSpecialDistricts[game.Rules.Roll(0, noSpecialDistricts.Count)];
            noSpecialDistricts.Remove(hospitalDistrictPos);
        }

        void CreateDistrict(int x, int y)
        {
            // create the district.
            District district = new District(new Point(x, y), GenerateDistrictKind(world, x, y));
            world[x, y] = district;

            // create the entry map.
            district.EntryMap = GenerateDistrictEntryMap(world, district, policeStationDistrictPos, hospitalDistrictPos);
            district.Name = district.EntryMap.Name;

            // create other maps.
            // - sewers
            Map sewers = GenerateDistrictSewersMap(district);
            district.SewersMap = sewers;
            // - subway (only in the middle district line)
            if (y == world.Size / 2)
            {
                Map subwayMap = GenerateDistrictSubwayMap(district);
                district.SubwayMap = subwayMap;
            }
        }

        DistrictKind GenerateDistrictKind(World world, int gridX, int gridY)
        {
            // Decide district kind - some districts are harcoded:
            //   - (0,0) : always Business.
            if (gridX == 0 && gridY == 0)
                return DistrictKind.BUSINESS;
            else
                return (DistrictKind)game.Rules.Roll((int)DistrictKind._FIRST, (int)DistrictKind._COUNT);
        }

        Map GenerateDistrictEntryMap(World world, District district, Point policeStationDistrictPos, Point hospitalDistrictPos)
        {
            int gridX = district.WorldPosition.X;
            int gridY = district.WorldPosition.Y;

            ///////////////////////////
            // 1. Compute unique seed.
            // 2. Set params for kind.
            // 3. Generate map.
            ///////////////////////////

            // 1. Compute unique seed.
            int gridSeed = game.Session.Seed + gridY * world.Size + gridX;

            // 3. Set gen params.
            BaseTownGenerator.Parameters genParams = BaseTownGenerator.DEFAULT_PARAMS;
            genParams.MapWidth = genParams.MapHeight = game.Options.DistrictSize;
            genParams.District = district;
            int factor = 8;
            string kindName = "District";
            switch (district.Kind)
            {
                case DistrictKind.SHOPPING:
                    // more shops, less other types.
                    kindName = "Shopping District";
                    genParams.CHARBuildingChance /= factor;
                    genParams.ShopBuildingChance *= factor;
                    genParams.ParkBuildingChance /= factor;
                    break;
                case DistrictKind.GREEN:
                    // more parks, less other types.
                    kindName = "Green District";
                    genParams.CHARBuildingChance /= factor;
                    genParams.ParkBuildingChance *= factor;
                    genParams.ShopBuildingChance /= factor;
                    break;
                case DistrictKind.BUSINESS:
                    // more offices, less other types.
                    kindName = "Business District";
                    genParams.CHARBuildingChance *= factor;
                    genParams.ParkBuildingChance /= factor;
                    genParams.ShopBuildingChance /= factor;
                    break;
                case DistrictKind.RESIDENTIAL:
                    // more housings, less other types.
                    kindName = "Residential District";
                    genParams.CHARBuildingChance /= factor;
                    genParams.ParkBuildingChance /= factor;
                    genParams.ShopBuildingChance /= factor;
                    break;
                case DistrictKind.GENERAL:
                    // use default params.
                    kindName = "District";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled district kind");
            }

            // Special params.
            genParams.GeneratePoliceStation = (district.WorldPosition == policeStationDistrictPos);
            genParams.GenerateHospital = (district.WorldPosition == hospitalDistrictPos);

            // 4. Generate map.
            BaseTownGenerator.Parameters prevParams = townGenerator.Params;
            townGenerator.Params = genParams;
            Map map = townGenerator.Generate(gridSeed);
            map.Name = string.Format("{0}@{1}", kindName, World.CoordToString(gridX, gridY));
            townGenerator.Params = prevParams;

            // done.
            return map;
        }

        Map GenerateDistrictSewersMap(District district)
        {
            // Compute uniqueseed.
            int sewersSeed = (district.EntryMap.Seed << 1) ^ district.EntryMap.Seed;

            // Generate map.
            Map sewers = townGenerator.GenerateSewersMap(sewersSeed, district);
            sewers.Name = string.Format("Sewers@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y);

            // done.
            return sewers;
        }

        Map GenerateDistrictSubwayMap(District district)
        {
            // Compute uniqueseed.
            int subwaySeed = (district.EntryMap.Seed << 2) ^ district.EntryMap.Seed;

            // Generate map.
            Map subway = townGenerator.GenerateSubwayMap(subwaySeed, district);
            subway.Name = string.Format("Subway@{0}-{1}", district.WorldPosition.X, district.WorldPosition.Y);

            // done.
            return subway;
        }

        UniqueMap CreateUniqueMap_CHARUndegroundFacility(World world)
        {
            ////////////////////////////////////////////////
            // 1. Find all business districts with offices.
            // 2. Pick one business district at random.
            // 3. Generate underground map there.
            ////////////////////////////////////////////////

            // 1. Find all business districts with offices.
            List<District> goodDistricts = null;
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                {
                    if (world[x, y].Kind == DistrictKind.BUSINESS)
                    {
                        bool hasOffice = false;
                        foreach (Zone z in world[x, y].EntryMap.Zones)
                        {
                            if (z.HasGameAttribute(ZoneAttributes.IS_CHAR_OFFICE))
                            {
                                hasOffice = true;
                                break;
                            }
                        }
                        if (hasOffice)
                        {
                            if (goodDistricts == null)
                                goodDistricts = new List<District>();
                            goodDistricts.Add(world[x, y]);
                        }
                    }
                }

            // 2. Pick one business district at random.
            if (goodDistricts == null)
            {
                throw new InvalidOperationException("world has no business districts with offices");
            }
            District chosenDistrict = goodDistricts[game.Rules.Roll(0, goodDistricts.Count)];

            // 3. Generate underground map there.
            List<Zone> offices = new List<Zone>();
            foreach (Zone z in chosenDistrict.EntryMap.Zones)
            {
                if (z.HasGameAttribute(ZoneAttributes.IS_CHAR_OFFICE))
                {
                    offices.Add(z);
                }
            }
            Zone chosenOffice = offices[game.Rules.Roll(0, offices.Count)];
            Point baseEntryPos;
            Map map = townGenerator.GenerateUniqueMap_CHARUnderground(chosenDistrict.EntryMap, chosenOffice, out baseEntryPos);
            map.District = chosenDistrict;
            map.Name = string.Format("CHAR Underground Facility @{0}-{1}", baseEntryPos.X, baseEntryPos.Y);
            chosenDistrict.AddUniqueMap(map);
            return new UniqueMap() { TheMap = map };
        }

        void LinkDistrict(int x, int y)
        {
            // add exits (from and to).
            Map map = world[x, y].EntryMap;

            if (y > 0)
            {
                // north.
                Map toMap = world[x, y - 1].EntryMap;
                for (int fromX = 0; fromX < map.Width; fromX++)
                {
                    int toX = fromX;
                    if (toX >= toMap.Width)
                        continue;
                    // link?
                    if (game.Rules.RollChance(DISTRICT_EXIT_CHANCE_PER_TILE))
                    {
                        Point ptMapFrom = new Point(fromX, -1);
                        Point ptMapTo = new Point(fromX, toMap.Height - 1);
                        Point ptFromMapFrom = new Point(fromX, toMap.Height);
                        Point ptFromMapTo = new Point(fromX, 0);
                        if (CheckIfExitIsGood(map, ptMapFrom, toMap, ptMapTo) &&
                            CheckIfExitIsGood(toMap, ptFromMapFrom, map, ptFromMapTo))
                        {
                            GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                            GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);
                        }
                    }
                }
            }
            if (x > 0)
            {
                // west.
                Map toMap = world[x - 1, y].EntryMap;
                for (int fromY = 0; fromY < map.Height; fromY++)
                {
                    int toY = fromY;
                    if (toY >= toMap.Height)
                        continue;
                    // link?
                    if (game.Rules.RollChance(DISTRICT_EXIT_CHANCE_PER_TILE))
                    {
                        Point ptMapFrom = new Point(-1, fromY);
                        Point ptMapTo = new Point(toMap.Width - 1, fromY);
                        Point ptFromMapFrom = new Point(toMap.Width, fromY);
                        Point ptFromMapTo = new Point(0, fromY);
                        if (CheckIfExitIsGood(map, ptMapFrom, toMap, ptMapTo) &&
                            CheckIfExitIsGood(toMap, ptFromMapFrom, map, ptFromMapTo))
                        {
                            GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                            GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);
                        }
                    }
                }
            }

            map = world[x, y].SewersMap;
            if (y > 0)
            {
                // north.
                Map toMap = world[x, y - 1].SewersMap;
                for (int fromX = 0; fromX < map.Width; fromX++)
                {
                    int toX = fromX;
                    if (toX >= toMap.Width)
                        continue;
                    Point ptMapFrom = new Point(fromX, -1);
                    Point ptMapTo = new Point(fromX, toMap.Height - 1);
                    Point ptFromMapFrom = new Point(fromX, toMap.Height);
                    Point ptFromMapTo = new Point(fromX, 0);
                    GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                    GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);
                }
            }
            if (x > 0)
            {
                // west.
                Map toMap = world[x - 1, y].SewersMap;
                for (int fromY = 0; fromY < map.Height; fromY++)
                {
                    int toY = fromY;
                    if (toY >= toMap.Height)
                        continue;

                    Point ptMapFrom = new Point(-1, fromY);
                    Point ptMapTo = new Point(toMap.Width - 1, fromY);
                    Point ptFromMapFrom = new Point(toMap.Width, fromY);
                    Point ptFromMapTo = new Point(0, fromY);

                    GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                    GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);

                }
            }

            map = world[x, y].SubwayMap;
            if (map != null)
            {
                if (x > 0)
                {
                    // west.
                    Map toMap = world[x - 1, y].SubwayMap;
                    for (int fromY = 0; fromY < map.Height; fromY++)
                    {
                        int toY = fromY;
                        if (toY >= toMap.Height)
                            continue;

                        Point ptMapFrom = new Point(-1, fromY);
                        Point ptMapTo = new Point(toMap.Width - 1, fromY);
                        Point ptFromMapFrom = new Point(toMap.Width, fromY);
                        Point ptFromMapTo = new Point(0, fromY);

                        if (!map.IsWalkable(map.Width - 1, fromY))
                            continue;
                        if (!toMap.IsWalkable(0, fromY))
                            continue;

                        GenerateExit(map, ptMapFrom, toMap, ptMapTo);
                        GenerateExit(toMap, ptFromMapFrom, map, ptFromMapTo);

                    }
                }
            }
        }

        bool CheckIfExitIsGood(Map fromMap, Point from, Map toMap, Point to)
        {
            // Don't if tile not walkable or map object there.
            if (!toMap.GetTileAt(to.X, to.Y).Model.IsWalkable)
                return false;
            if (toMap.GetMapObjectAt(to.X, to.Y) != null)
                return false;

            // good spot.
            return true;
        }

        void GenerateExit(Map fromMap, Point from, Map toMap, Point to)
        {
            // add exit.
            fromMap.SetExitAt(from, new Exit(toMap, to));
        }

        void GenerateUniqueActors()
        {
            // "Sewers Thing" - in one of the sewers
            m_Session.UniqueActors.TheSewersThing = SpawnUniqueSewersThing(world);

            // Unique survivors NPCs.
            m_Session.UniqueActors.BigBear = CreateUniqueBigBear(world);
            m_Session.UniqueActors.FamuFataru = CreateUniqueFamuFataru(world);
            m_Session.UniqueActors.Santaman = CreateUniqueSantaman(world);
            m_Session.UniqueActors.Roguedjack = CreateUniqueRoguedjack(world);
            m_Session.UniqueActors.Duckman = CreateUniqueDuckman(world);
            m_Session.UniqueActors.HansVonHanz = CreateUniqueHansVonHanz(world);

            // Make all uniques npcs invincible until spotted
            foreach (UniqueActor uniqueActor in m_Session.UniqueActors.ToArray())
                uniqueActor.TheActor.IsInvincible = true;
        }

        void SpawnUniqueItems()
        {
            // "Subway Worker Badge" - somewhere on the subway tracks...
            Action(() => m_Session.UniqueItems.TheSubwayWorkerBadge = SpawnUniqueSubwayWorkerBadge(world));
        }

        void SpawnSpecialDecorations()
        {
            // Easter egg: "roguedjack was here" tag.
            Map easterEggTagMap = world[0, 0].SewersMap;
            easterEggTagMap.RemoveMapObjectAt(1, 1);
            easterEggTagMap.GetTileAt(1, 1).RemoveAllDecorations();
            easterEggTagMap.GetTileAt(1, 1).AddDecoration(GameImages.DECO_ROGUEDJACK_TAG);
        }

        void SpawnPlayer()
        {
            int gridCenter = world.Size / 2;

            Map startMap = world[gridCenter, gridCenter].EntryMap;
            GeneratePlayerOnMap(startMap, townGenerator);
            SetCurrentMap(startMap);
            RefreshPlayer();
            UpdatePlayerFOV(m_Player);  // to make sure we get notified of actors acting before us in turn 0.
        }

        void RevealStartingMap()
        {
            if (!s_Options.RevealStartingDistrict)
                return;

            List<Zone> startZones = startMap.GetZonesAt(m_Player.Location.Position.X, m_Player.Location.Position.Y);
            if (startZones != null)
            {
                Zone startZone = startZones[0];
                for (int x = 0; x < startMap.Width; x++)
                    for (int y = 0; y < startMap.Height; y++)
                    {
                        bool revealThisTile = false;

                        // reveal if:
                        // - starting zone (house).
                        // - outside.

                        // - starting zone (house).
                        List<Zone> zones = startMap.GetZonesAt(x, y);
                        if (zones != null && zones[0] == startZone)
                            revealThisTile = true;
                        else if (!startMap.GetTileAt(x, y).IsInside)
                            revealThisTile = true;

                        // reveal?
                        if (revealThisTile)
                            startMap.GetTileAt(x, y).IsVisited = true;
                    }
            }
        }

        public override void Draw()
        {
            Draw("Generating world, please wait...");
        }

        public override void Update(double dt)
        {
            if (Process())
                game.PopState(); // !FIXME
        }
    }
}
