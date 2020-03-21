using RogueSurvivor.Data;
using RogueSurvivor.Engine.Items;
using RogueSurvivor.Gameplay;
using RogueSurvivor.Gameplay.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class GenerateWorldState : LoadScreenState
    {
        const int DISTRICT_EXIT_CHANCE_PER_TILE = 15;

        BaseTownGenerator townGenerator;
        World world;
        Map startMap;
        Point policeStationDistrictPos;
        Point hospitalDistrictPos;

        public override void Init()
        {
            BaseTownGenerator.Parameters genParams = BaseTownGenerator.DEFAULT_PARAMS;
            genParams.MapWidth = genParams.MapHeight = 100;
            Logger.WriteLine(Logger.Stage.INIT, "creating Generator");
            townGenerator = new StdTownGenerator(genParams);
            game.townGenerator = townGenerator;
        }

        public override void Enter()
        {
            CreateEmptyWorld();

            // Create districts maps
            CategoryStart("Creating districts...");
            for (int x = 0; x < world.Size; x++)
            {
                for (int y = 0; y < world.Size; y++)
                {
                    int _x = x, _y = y;
                    Action(() => CreateDistrict(_x, _y));
                }
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
                {
                    int _x = x, _y = y;
                    Action(() => LinkDistrict(_x, _y));
                }
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
            world = new World(RogueGame.Options.CitySize);
            game.Session.World = world;

            ////////////////////////
            // Roll initial weather
            ////////////////////////
            world.Weather = (Weather)game.Rules.Roll((int)Weather._FIRST, (int)Weather._COUNT);
            world.NextWeatherCheckTurn = game.Rules.Roll(RogueGame.WEATHER_MIN_DURATION, RogueGame.WEATHER_MAX_DURATION);

            //////////////////////////////////////////////
            // Roll locations of special buildings.
            // Only ONE special building max per district.
            //////////////////////////////////////////////
            List<Point> noSpecialDistricts = new List<Point>();
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                    noSpecialDistricts.Add(new Point(x, y));

            policeStationDistrictPos = noSpecialDistricts[game.Rules.Roll(0, noSpecialDistricts.Count)];
            noSpecialDistricts.Remove(policeStationDistrictPos);

            hospitalDistrictPos = noSpecialDistricts[game.Rules.Roll(0, noSpecialDistricts.Count)];
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
            genParams.MapWidth = genParams.MapHeight = RogueGame.Options.DistrictSize;
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
            game.Session.UniqueActors.TheSewersThing = SpawnUniqueSewersThing(world);

            // Unique survivors NPCs.
            game.Session.UniqueActors.BigBear = CreateUniqueBigBear(world);
            game.Session.UniqueActors.FamuFataru = CreateUniqueFamuFataru(world);
            game.Session.UniqueActors.Santaman = CreateUniqueSantaman(world);
            game.Session.UniqueActors.Roguedjack = CreateUniqueRoguedjack(world);
            game.Session.UniqueActors.Duckman = CreateUniqueDuckman(world);
            game.Session.UniqueActors.HansVonHanz = CreateUniqueHansVonHanz(world);

            // Make all uniques npcs invincible until spotted
            foreach (UniqueActor uniqueActor in game.Session.UniqueActors.ToArray())
                uniqueActor.TheActor.IsInvincible = true;
        }

        UniqueActor SpawnUniqueSewersThing(World world)
        {
            ///////////////////////////////////////////////////////
            // 1. Pick a random sewers map.
            // 2. Create Sewers Thing.
            // 3. Spawn in sewers map.
            // 4. Add warning board in maintenance rooms (if any).
            ///////////////////////////////////////////////////////

            // 1. Pick a random sewers map.
            Map map = world[game.Rules.Roll(0, world.Size), game.Rules.Roll(0, world.Size)].SewersMap;

            // 2. Create Sewers Thing.
            ActorModel model = game.Actors.SewersThing;
            Actor actor = model.CreateNamed(game.Factions.TheUndeads, "The Sewers Thing", false, 0);

            // 3. Spawn in sewers map.
            DiceRoller roller = new DiceRoller(map.Seed);
            bool spawned = townGenerator.ActorPlace(roller, 10000, map, actor);
            if (!spawned)
                throw new InvalidOperationException("could not spawn unique The Sewers Thing");

            // 4. Add warning board in maintenance rooms (if any).
            Zone maintenanceZone = map.GetZoneByPartialName(RogueGame.NAME_SEWERS_MAINTENANCE);
            if (maintenanceZone != null)
            {
                townGenerator.MapObjectPlaceInGoodPosition(map, maintenanceZone.Bounds,
                    (pt) => map.IsWalkable(pt.X, pt.Y) && map.GetActorAt(pt) == null && map.GetItemsAt(pt) == null,
                    roller,
                    (pt) => townGenerator.MakeObjBoard(GameImages.OBJ_BOARD,
                        new string[] { "TO SEWER WORKERS :",
                                       "- It lives here.",
                                       "- Do not disturb.",
                                       "- Approach with caution.",
                                       "- Watch your back.",
                                       "- In case of emergency, take refuge here.",
                                       "- Do not let other people interact with it!"}));
            }

            // done.
            return new UniqueActor() { TheActor = actor, IsSpawned = true };
        }

        UniqueActor CreateUniqueBigBear(World world)
        {
            ActorModel model = game.Actors.MaleCivilian;
            Actor actor = model.CreateNamed(game.Factions.TheCivilians, "Big Bear", false, 0);
            actor.IsUnique = true;

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_BIG_BEAR);

            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.TOUGH);

            Item bat = new ItemMeleeWeapon(game.Items.UNIQUE_BIGBEAR_BAT) { IsUnique = true };
            actor.Inventory.AddAll(bat);
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear an angry man shouting 'FOOLS!'",
                EventThemeMusic = GameMusics.BIGBEAR_THEME_SONG
            };
        }

        UniqueActor CreateUniqueFamuFataru(World world)
        {
            ActorModel model = game.Actors.FemaleCivilian;
            Actor actor = model.CreateNamed(game.Factions.TheCivilians, "Famu Fataru", false, 0);
            actor.IsUnique = true;

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);

            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AGILE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);

            Item katana = new ItemMeleeWeapon(game.Items.UNIQUE_FAMU_FATARU_KATANA) { IsUnique = true };
            actor.Inventory.AddAll(katana);
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear a woman laughing.",
                EventThemeMusic = GameMusics.FAMU_FATARU_THEME_SONG
            };
        }

        UniqueActor CreateUniqueSantaman(World world)
        {
            ActorModel model = game.Actors.MaleCivilian;
            Actor actor = model.CreateNamed(game.Factions.TheCivilians, "Santaman", false, 0);
            actor.IsUnique = true;

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_SANTAMAN);

            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.AWAKE);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);

            Item shotty = new ItemRangedWeapon(game.Items.UNIQUE_SANTAMAN_SHOTGUN) { IsUnique = true };
            actor.Inventory.AddAll(shotty);
            actor.Inventory.AddAll(townGenerator.MakeItemShotgunAmmo());
            actor.Inventory.AddAll(townGenerator.MakeItemShotgunAmmo());
            actor.Inventory.AddAll(townGenerator.MakeItemShotgunAmmo());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear christmas music and drunken vomitting.",
                EventThemeMusic = GameMusics.SANTAMAN_THEME_SONG
            };
        }

        UniqueActor CreateUniqueRoguedjack(World world)
        {
            ActorModel model = game.Actors.MaleCivilian;
            Actor actor = model.CreateNamed(game.Factions.TheCivilians, "Roguedjack", false, 0);
            actor.IsUnique = true;

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_ROGUEDJACK);

            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HARDY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);

            Item basher = new ItemMeleeWeapon(game.Items.UNIQUE_ROGUEDJACK_KEYBOARD) { IsUnique = true };
            actor.Inventory.AddAll(basher);
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear a man shouting in French.",
                EventThemeMusic = GameMusics.ROGUEDJACK_THEME_SONG
            };
        }

        UniqueActor CreateUniqueDuckman(World world)
        {
            ActorModel model = game.Actors.MaleCivilian;
            Actor actor = model.CreateNamed(game.Factions.TheCivilians, "Duckman", false, 0);
            actor.IsUnique = true;

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_DUCKMAN);

            // awesome superhero!
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.CHARISMATIC);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.STRONG);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HIGH_STAMINA);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.MARTIAL_ARTS);

            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());

            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear loud demented QUACKS.",
                EventThemeMusic = GameMusics.DUCKMAN_THEME_SONG
            };
        }

        UniqueActor CreateUniqueHansVonHanz(World world)
        {
            ActorModel model = game.Actors.MaleCivilian;
            Actor actor = model.CreateNamed(game.Factions.TheCivilians, "Hans von Hanz", false, 0);
            actor.IsUnique = true;

            actor.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_HANS_VON_HANZ);

            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.HAULER);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.FIREARMS);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.LEADERSHIP);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);
            townGenerator.GiveStartingSkillToActor(actor, Skills.IDs.NECROLOGY);

            Item pistol = new ItemRangedWeapon(game.Items.UNIQUE_HANS_VON_HANZ_PISTOL) { IsUnique = true };
            actor.Inventory.AddAll(pistol);
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());
            actor.Inventory.AddAll(townGenerator.MakeItemCannedFood());

            // done.
            return new UniqueActor()
            {
                TheActor = actor,
                IsSpawned = false,
                IsWithRefugees = true,
                EventMessage = "You hear a man barking orders in German.",
                EventThemeMusic = GameMusics.HANS_VON_HANZ_THEME_SONG
            };
        }

        void SpawnUniqueItems()
        {
            // "Subway Worker Badge" - somewhere on the subway tracks...
            game.Session.UniqueItems.TheSubwayWorkerBadge = SpawnUniqueSubwayWorkerBadge(world);
        }

        UniqueItem SpawnUniqueSubwayWorkerBadge(World world)
        {
            ///////////////////////////////////
            // 1. Pick a random Subway map.
            //    Fails if not found.
            // 2. Pick a position in the rails.
            // 3. Drop it.
            ///////////////////////////////////

            Item it = new Item(game.Items.UNIQUE_SUBWAY_BADGE) { IsUnique = true, IsForbiddenToAI = true };

            // 1. Pick a random Subway map.
            List<Map> allSubways = new List<Map>();
            for (int x = 0; x < world.Size; x++)
                for (int y = 0; y < world.Size; y++)
                    if (world[x, y].HasSubway)
                        allSubways.Add(world[x, y].SubwayMap);
            if (allSubways.Count == 0)
                return new UniqueItem() { TheItem = it, IsSpawned = false };
            Map subway = allSubways[game.Rules.Roll(0, allSubways.Count)];

            // 2. Pick a position in the rails.
            Rectangle railsRect = subway.GetZoneByPartialName(RogueGame.NAME_SUBWAY_RAILS).Bounds;
            Point dropPt = new Point(game.Rules.Roll(railsRect.Left, railsRect.Right), game.Rules.Roll(railsRect.Top, railsRect.Bottom));

            // 3. Drop it.
            subway.DropItemAt(it, dropPt);
            // blood! deceased worker.
            subway.GetTileAt(dropPt).AddDecoration(GameImages.DECO_BLOODIED_FLOOR);

            // done.
            return new UniqueItem() { TheItem = it, IsSpawned = true };
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

            startMap = world[gridCenter, gridCenter].EntryMap;
            GeneratePlayerOnMap(startMap);
            game.SetCurrentMap(startMap);
            game.RefreshPlayer();
            game.UpdatePlayerFOV(game.Player);  // to make sure we get notified of actors acting before us in turn 0.
        }

        void GeneratePlayerOnMap(Map map)
        {
            DiceRoller roller = new DiceRoller(map.Seed);

            /////////////////////////////////////////////////////
            // Create player actor : living/undead x male/female
            /////////////////////////////////////////////////////
            ActorModel playerModel;
            Actor player;
            CharGen charGen = game.Session.charGen;
            if (charGen.IsUndead)
            {
                // Handle specific undead type.
                // Zombified : need living, then zombify.
                switch (charGen.UndeadModel)
                {
                    case GameActors.IDs.UNDEAD_SKELETON:
                        {
                            // Create the Skeleton.
                            player = game.Actors.Skeleton.CreateNumberedName(game.Factions.TheUndeads, 0);
                            break;
                        }

                    case GameActors.IDs.UNDEAD_ZOMBIE:
                        {
                            // Create the Zombie.
                            player = game.Actors.Zombie.CreateNumberedName(game.Factions.TheUndeads, 0);
                            break;
                        }

                    case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
                    case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
                        {
                            // First create as living.
                            playerModel = charGen.IsMale ? game.Actors.MaleCivilian : game.Actors.FemaleCivilian;
                            player = playerModel.CreateAnonymous(game.Factions.TheCivilians, 0);
                            townGenerator.DressCivilian(roller, player);
                            townGenerator.GiveNameToActor(roller, player);
                            // Then zombify.
                            player = game.Zombify(null, player, true);
                            break;
                        }

                    case GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
                        {
                            // Create the ZM.
                            player = game.Actors.ZombieMaster.CreateNumberedName(game.Factions.TheUndeads, 0);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException("unhandled undeadModel");
                }

                // Then make sure player related stuff are setup properly.
                game.PrepareActorForPlayerControl(player);
            }
            else
            {
                // Create living.
                playerModel = charGen.IsMale ? game.Actors.MaleCivilian : game.Actors.FemaleCivilian;
                player = playerModel.CreateAnonymous(game.Factions.TheCivilians, 0);
                townGenerator.DressCivilian(roller, player);
                townGenerator.GiveNameToActor(roller, player);
                player.Sheet.SkillTable.AddOrIncreaseSkill((int)charGen.StartingSkill);

                player.RecomputeStartingStats();
                player.OnSkillUpgrade(charGen.StartingSkill);
                // slightly randomize Food and Sleep - 0..25%.
                int foodDeviation = (int)(0.25f * player.FoodPoints);
                player.FoodPoints = player.FoodPoints - game.Rules.Roll(0, foodDeviation);
                int sleepDeviation = (int)(0.25f * player.SleepPoints);
                player.SleepPoints = player.SleepPoints - game.Rules.Roll(0, sleepDeviation);
            }

            player.Controller = new PlayerController();

            /////////////
            // Spawn him.
            /////////////
            // living: try to spawn inside on a couch, then if failed spawn anywhere inside.
            // undead: spawn outside.
            // NEVER spawn in CHAR Office!!
            bool preferedSpawnOk = townGenerator.ActorPlace(roller, 10 * map.Width * map.Height, map, player,
                (pt) =>
                {
                    bool isInside = map.GetTileAt(pt.X, pt.Y).IsInside;
                    if ((charGen.IsUndead && isInside) || (!charGen.IsUndead && !isInside))
                        return false;

                    if (RogueGame.IsInCHAROffice(new Location(map, pt)))
                        return false;

                    MapObject mapObj = map.GetMapObjectAt(pt);
                    if (charGen.IsUndead)
                        return mapObj == null;
                    else
                        return mapObj != null && mapObj.IsCouch;
                });

            if (!preferedSpawnOk)
            {
                // no couch, try inside but never in char office.
                bool spawnedInside = townGenerator.ActorPlace(roller, map.Width * map.Height, map, player,
                    (pt) => map.GetTileAt(pt.X, pt.Y).IsInside && !RogueGame.IsInCHAROffice(new Location(map, pt)));

                if (!spawnedInside)
                {
                    // could not spawn inside, do it outside...
                    while (!townGenerator.ActorPlace(roller, int.MaxValue, map, player, (pt) => !RogueGame.IsInCHAROffice(new Location(map, pt))))
                        ;
                }
            }
        }

        void RevealStartingMap()
        {
            if (!RogueGame.Options.RevealStartingDistrict)
                return;

            List<Zone> startZones = startMap.GetZonesAt(game.Player.Location.Position.X, game.Player.Location.Position.Y);
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
                game.SetState<RogueGame>();
        }
    }
}
