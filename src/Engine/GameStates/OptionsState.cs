using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.UI;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    class OptionsState : GameState
    {
        readonly GameOptions.IDs[] list = new GameOptions.IDs[]
        {
            // autosave
            GameOptions.IDs.GAME_AUTOSAVE_PERIOD,
            // display & sounds
            GameOptions.IDs.UI_MUSIC,
            GameOptions.IDs.UI_MUSIC_VOLUME,
            GameOptions.IDs.UI_ANIM_DELAY,
            GameOptions.IDs.UI_SHOW_MINIMAP,
            GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP,
            // helpers
            GameOptions.IDs.UI_ADVISOR,
            GameOptions.IDs.UI_COMBAT_ASSISTANT,
            GameOptions.IDs.UI_SHOW_PLAYER_TARGETS,
            GameOptions.IDs.UI_SHOW_TARGETS,
            // sim
            GameOptions.IDs.GAME_SIMULATE_DISTRICTS,
            GameOptions.IDs.GAME_SIM_THREAD,
            GameOptions.IDs.GAME_SIMULATE_SLEEP,
            // death
            GameOptions.IDs.GAME_DEATH_SCREENSHOT,
            GameOptions.IDs.GAME_PERMADEATH,
            // maps
            GameOptions.IDs.GAME_CITY_SIZE,
            GameOptions.IDs.GAME_DISTRICT_SIZE,
            GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT,
            // living
            GameOptions.IDs.GAME_MAX_CIVILIANS,
            // GameOptions.IDs.GAME_MAX_DOGS,
            GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE,
            GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS,
            GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH,
            GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE,
            // undeads
            GameOptions.IDs.GAME_MAX_UNDEADS,
            GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION,
            GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT,
            GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE,
            GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS,
            GameOptions.IDs.GAME_SHAMBLERS_UPGRADE,
            GameOptions.IDs.GAME_SKELETONS_UPGRADE,
            GameOptions.IDs.GAME_RATS_UPGRADE,
            // events
            GameOptions.IDs.GAME_NATGUARD_FACTOR,
            GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR,
            // reinc
            GameOptions.IDs.GAME_MAX_REINCARNATIONS,
            GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED,
            GameOptions.IDs.GAME_REINCARNATE_AS_RAT,
            GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS
        };

        GameOptions prevOptions;
        string[] menuEntries;
        string[] values;
        int selected;

        public override void Init()
        {
            menuEntries = new string[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                menuEntries[i] = GameOptions.Name(list[i]);
                GameOptions.IDs id = list[i];
                if (id == GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION ||
                    id == GameOptions.IDs.GAME_RATS_UPGRADE ||
                    id == GameOptions.IDs.GAME_SKELETONS_UPGRADE ||
                    id == GameOptions.IDs.GAME_SHAMBLERS_UPGRADE)
                    menuEntries[i] += " -V";
                else if (id == GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE ||
                    id == GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE)
                    menuEntries[i] += " =S";
            }

            values = new string[list.Length];
        }

        void RefreshValues()
        {
            for (int i = 0; i < list.Length; i++)
                values[i] = RogueGame.Options.DescribeValue(game.Session.GameMode, list[i]);
        }

        public override void Enter()
        {
            prevOptions = RogueGame.Options;
            RefreshValues();
            selected = 0;
        }

        public override void Draw()
        {
            int gx, gy;
            gx = gy = 0;
            ui.Clear(Color.Black);
            ui.DrawHeader();
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Yellow, "Options", 0, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGreen, values, gx, ref gy, false, 400);

            // describe current option.
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, menuEntries[selected].TrimStart(), gx, gy);
            gy += Ui.BOLD_LINE_SPACING;
            string desc = GameOptions.Describe(list[selected]);
            string[] descLines = desc.Split(new char[] { '\n' });
            foreach (string d in descLines)
            {
                ui.DrawString(Color.White, "  " + d, gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
            }

            // legend.
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.Red, "* Caution : increasing these values makes the game runs slower and saving/loading longer.", gx, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "-V : option always OFF when playing VTG-Vintage", gx, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "=S : option used only when playing STD-Standard", gx, gy);
            gy += Ui.BOLD_LINE_SPACING;

            // difficulty rating.               
            gy += Ui.BOLD_LINE_SPACING;
            int diffForSurvivor = (int)(100 * Scoring.ComputeDifficultyRating(RogueGame.Options, DifficultySide.FOR_SURVIVOR, 0));
            int diffforUndead = (int)(100 * Scoring.ComputeDifficultyRating(RogueGame.Options, DifficultySide.FOR_UNDEAD, 0));
            ui.DrawStringBold(Color.Yellow, string.Format("Difficulty Rating : {0}% as survivor / {1}% as undead.", diffForSurvivor, diffforUndead), gx, gy);
            gy += Ui.BOLD_LINE_SPACING;
            ui.DrawStringBold(Color.White, "Difficulty used for scoring automatically decrease with each reincarnation.", gx, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;

            // footnote.
            ui.DrawFootnote(Color.White, "cursor to move and change values, R to restore previous values, ESC to save and leave");
        }

        public override void Update(double dt)
        {
            ref GameOptions options = ref RogueGame.Options;
            Key key = ui.ReadKey();
            switch (key)
            {
                case Key.Up:
                    if (selected > 0)
                        --selected;
                    else
                        selected = menuEntries.Length - 1;
                    break;

                case Key.Down:
                    selected = (selected + 1) % menuEntries.Length;
                    break;

                case Key.R: // restore previous.
                    options = prevOptions;
                    game.ApplyOptions();
                    RefreshValues();
                    break;

                case Key.Escape:
                    options.Save(RogueGame.OptionsFile);
                    game.PopState();
                    break;

                case Key.Left:
                    switch (list[selected])
                    {
                        case GameOptions.IDs.GAME_DISTRICT_SIZE: options.DistrictSize -= 5; break;
                        case GameOptions.IDs.UI_MUSIC: options.PlayMusic = !options.PlayMusic; break;
                        case GameOptions.IDs.UI_MUSIC_VOLUME: options.MusicVolume -= 5; break;
                        case GameOptions.IDs.UI_ANIM_DELAY: options.IsAnimDelayOn = !options.IsAnimDelayOn; break;
                        case GameOptions.IDs.UI_SHOW_MINIMAP: options.IsMinimapOn = !options.IsMinimapOn; break;
                        case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: options.ShowPlayerTagsOnMinimap = !options.ShowPlayerTagsOnMinimap; break;
                        case GameOptions.IDs.UI_ADVISOR: options.IsAdvisorEnabled = !options.IsAdvisorEnabled; break;
                        case GameOptions.IDs.UI_COMBAT_ASSISTANT: options.IsCombatAssistantOn = !options.IsCombatAssistantOn; break;
                        case GameOptions.IDs.UI_SHOW_TARGETS: options.ShowTargets = !options.ShowTargets; break;
                        case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS: options.ShowPlayerTargets = !options.ShowPlayerTargets; break;
                        case GameOptions.IDs.GAME_MAX_CIVILIANS: options.MaxCivilians -= 5; break;
                        case GameOptions.IDs.GAME_MAX_DOGS: --options.MaxDogs; break;
                        case GameOptions.IDs.GAME_MAX_UNDEADS: options.MaxUndeads -= 10; break;
                        case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: options.DayZeroUndeadsPercent -= 5; break;
                        case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: --options.ZombieInvasionDailyIncrease; break;
                        case GameOptions.IDs.GAME_CITY_SIZE: options.CitySize -= 1; break;
                        case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH: options.NPCCanStarveToDeath = !options.NPCCanStarveToDeath; break;
                        case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: options.StarvedZombificationChance -= 5; break;
                        case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                            if (options.SimulateDistricts != GameOptions.SimRatio.OFF)
                                options.SimulateDistricts = (GameOptions.SimRatio)(options.SimulateDistricts - 1);
                            break;
                        case GameOptions.IDs.GAME_SIMULATE_SLEEP: options.SimulateWhenSleeping = !options.SimulateWhenSleeping; break;
                        case GameOptions.IDs.GAME_SIM_THREAD: options.SimThread = !options.SimThread; break;
                        case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE: options.ZombificationChance -= 5; break;
                        case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT: options.RevealStartingDistrict = !options.RevealStartingDistrict; break;
                        case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION: options.AllowUndeadsEvolution = !options.AllowUndeadsEvolution; break;
                        case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                            if (options.ZombifiedsUpgradeDays != GameOptions.ZupDays._FIRST)
                                options.ZombifiedsUpgradeDays = (GameOptions.ZupDays)(options.ZombifiedsUpgradeDays - 1);
                            break;
                        case GameOptions.IDs.GAME_MAX_REINCARNATIONS: --options.MaxReincarnations; break;
                        case GameOptions.IDs.GAME_REINCARNATE_AS_RAT: options.CanReincarnateAsRat = !options.CanReincarnateAsRat; break;
                        case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS: options.CanReincarnateToSewers = !options.CanReincarnateToSewers; break;
                        case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED: options.IsLivingReincRestricted = !options.IsLivingReincRestricted; break;
                        case GameOptions.IDs.GAME_PERMADEATH: options.IsPermadeathOn = !options.IsPermadeathOn; break;
                        case GameOptions.IDs.GAME_DEATH_SCREENSHOT: options.IsDeathScreenshotOn = !options.IsDeathScreenshotOn; break;
                        case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: options.IsAggressiveHungryCiviliansOn = !options.IsAggressiveHungryCiviliansOn; break;
                        case GameOptions.IDs.GAME_NATGUARD_FACTOR: options.NatGuardFactor -= 10; break;
                        case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR: options.SuppliesDropFactor -= 10; break;
                        case GameOptions.IDs.GAME_RATS_UPGRADE:
                            options.RatsUpgrade = !options.RatsUpgrade;
                            break;
                        case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
                            options.ShamblersUpgrade = !options.ShamblersUpgrade;
                            break;
                        case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
                            options.SkeletonsUpgrade = !options.SkeletonsUpgrade;
                            break;
                        case GameOptions.IDs.GAME_AUTOSAVE_PERIOD:
                            options.AutoSavePeriodInHours -= 12;
                            break;
                    }
                    game.ApplyOptions();
                    RefreshValues();
                    break;

                case Key.Right:
                    switch (list[selected])
                    {
                        case GameOptions.IDs.GAME_DISTRICT_SIZE: options.DistrictSize += 5; break;
                        case GameOptions.IDs.UI_MUSIC: options.PlayMusic = !options.PlayMusic; break;
                        case GameOptions.IDs.UI_MUSIC_VOLUME: options.MusicVolume += 5; break;
                        case GameOptions.IDs.UI_ANIM_DELAY: options.IsAnimDelayOn = !options.IsAnimDelayOn; break;
                        case GameOptions.IDs.UI_SHOW_MINIMAP: options.IsMinimapOn = !options.IsMinimapOn; break;
                        case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: options.ShowPlayerTagsOnMinimap = !options.ShowPlayerTagsOnMinimap; break;
                        case GameOptions.IDs.UI_ADVISOR: options.IsAdvisorEnabled = !options.IsAdvisorEnabled; break;
                        case GameOptions.IDs.UI_COMBAT_ASSISTANT: options.IsCombatAssistantOn = !options.IsCombatAssistantOn; break;
                        case GameOptions.IDs.UI_SHOW_TARGETS: options.ShowTargets = !options.ShowTargets; break;
                        case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS: options.ShowPlayerTargets = !options.ShowPlayerTargets; break;
                        case GameOptions.IDs.GAME_MAX_CIVILIANS: options.MaxCivilians += 5; break;
                        case GameOptions.IDs.GAME_MAX_DOGS: ++options.MaxDogs; break;
                        case GameOptions.IDs.GAME_MAX_UNDEADS: options.MaxUndeads += 10; break;
                        case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: options.DayZeroUndeadsPercent += 5; break;
                        case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: ++options.ZombieInvasionDailyIncrease; break;
                        case GameOptions.IDs.GAME_CITY_SIZE: options.CitySize += 1; break;
                        case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH: options.NPCCanStarveToDeath = !options.NPCCanStarveToDeath; break;
                        case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: options.StarvedZombificationChance += 5; break;
                        case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                            if (options.SimulateDistricts != GameOptions.SimRatio.FULL)
                            {
                                options.SimulateDistricts = (GameOptions.SimRatio)(options.SimulateDistricts + 1);
                            }
                            break;
                        case GameOptions.IDs.GAME_SIMULATE_SLEEP: options.SimulateWhenSleeping = !options.SimulateWhenSleeping; break;
                        case GameOptions.IDs.GAME_SIM_THREAD: options.SimThread = !options.SimThread; break;
                        case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE: options.ZombificationChance += 5; break;
                        case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT: options.RevealStartingDistrict = !options.RevealStartingDistrict; break;
                        case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
                            options.AllowUndeadsEvolution = !options.AllowUndeadsEvolution;
                            break;
                        case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                            if (options.ZombifiedsUpgradeDays != GameOptions.ZupDays._COUNT - 1)
                                options.ZombifiedsUpgradeDays = (GameOptions.ZupDays)(options.ZombifiedsUpgradeDays + 1);
                            break;
                        case GameOptions.IDs.GAME_MAX_REINCARNATIONS: ++options.MaxReincarnations; break;
                        case GameOptions.IDs.GAME_REINCARNATE_AS_RAT: options.CanReincarnateAsRat = !options.CanReincarnateAsRat; break;
                        case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS: options.CanReincarnateToSewers = !options.CanReincarnateToSewers; break;
                        case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED: options.IsLivingReincRestricted = !options.IsLivingReincRestricted; break;
                        case GameOptions.IDs.GAME_PERMADEATH: options.IsPermadeathOn = !options.IsPermadeathOn; break;
                        case GameOptions.IDs.GAME_DEATH_SCREENSHOT: options.IsDeathScreenshotOn = !options.IsDeathScreenshotOn; break;
                        case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: options.IsAggressiveHungryCiviliansOn = !options.IsAggressiveHungryCiviliansOn; break;
                        case GameOptions.IDs.GAME_NATGUARD_FACTOR: options.NatGuardFactor += 10; break;
                        case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR: options.SuppliesDropFactor += 10; break;
                        case GameOptions.IDs.GAME_RATS_UPGRADE: options.RatsUpgrade = !options.RatsUpgrade; break;
                        case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE: options.ShamblersUpgrade = !options.ShamblersUpgrade; break;
                        case GameOptions.IDs.GAME_SKELETONS_UPGRADE: options.SkeletonsUpgrade = !options.SkeletonsUpgrade; break;
                        case GameOptions.IDs.GAME_AUTOSAVE_PERIOD:
                            options.AutoSavePeriodInHours += 12;
                            break;
                    }
                    game.ApplyOptions();
                    RefreshValues();
                    break;
            }
        }
    }
}
