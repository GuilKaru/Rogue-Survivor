using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Extensions;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace RogueSurvivor.Engine.GameStates
{
    class PostMortemState : GameState
    {
        enum State
        {
            Saving,
            Confirm
        }

        TextFile graveyard;
        string name;
        int line;
        bool confirmError;

        public override void Enter()
        {
            ////////////////
            // Prepare data.
            ////////////////
            WorldTime deathTime = new WorldTime { TurnCounter = game.Session.Scoring.TurnsSurvived };
            string heOrShe = game.Player.HeOrShe.Capitalize();
            string hisOrHer = game.Player.HisOrHer;
            string himOrHer = game.Player.HimOrHer;
            name = game.Player.TheName.Replace("(YOU) ", "");
            TimeSpan rt = game.Session.Scoring.RealLifePlayingTime;
            string realTimeString = rt.ToStringShort();
            game.Session.Scoring.Side = game.Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
            game.Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(RogueGame.Options, game.Session.Scoring.Side, game.Session.Scoring.ReincarnationNumber);

            ////////////////////////////////////
            // Format scoring into a text file.
            ///////////////////////////////////
            graveyard = new TextFile();

            graveyard.Append(string.Format("ROGUE SURVIVOR REANIMATED {0}", SetupConfig.GAME_VERSION));
            graveyard.Append("POST MORTEM");

            graveyard.Append(string.Format("{0} was {1} and {2}.", name, AorAn(game.Player.Model.Name), AorAn(game.Player.Faction.MemberName)));
            graveyard.Append(string.Format("{0} survived to see {1}.", heOrShe, deathTime.ToString()));
            graveyard.Append(string.Format("{0}'s spirit guided {1} for {2}.", name, himOrHer, realTimeString));
            if (game.Session.Scoring.ReincarnationNumber > 0)
                graveyard.Append(string.Format("{0} was reincarnation {1}.", heOrShe, game.Session.Scoring.ReincarnationNumber));
            graveyard.Append(" ");

            graveyard.Append("> SCORING");
            graveyard.Append(string.Format("{0} scored a total of {1} points.", heOrShe, game.Session.Scoring.TotalPoints));
            graveyard.Append(string.Format("- difficulty rating of {0}%.", (int)(100 * game.Session.Scoring.DifficultyRating)));
            graveyard.Append(string.Format("- {0} base points for survival.", game.Session.Scoring.SurvivalPoints));
            graveyard.Append(string.Format("- {0} base points for kills.", game.Session.Scoring.KillPoints));
            graveyard.Append(string.Format("- {0} base points for achievements.", game.Session.Scoring.AchievementPoints));
            graveyard.Append(" ");

            graveyard.Append("> ACHIEVEMENTS");
            foreach (Achievement ach in game.Session.Scoring.Achievements)
            {
                if (ach.IsDone)
                    graveyard.Append(string.Format("- {0} for {1} points!", ach.Name, ach.ScoreValue));
                else
                    graveyard.Append(string.Format("- Fail : {0}.", ach.TeaseName));
            }
            if (game.Session.Scoring.CompletedAchievementsCount == 0)
            {
                graveyard.Append("Didn't achieve anything notable. And then died.");
                graveyard.Append(string.Format("(unlock all the {0} achievements to win this game version)", Scoring.MAX_ACHIEVEMENTS));
            }
            else
            {
                graveyard.Append(string.Format("Total : {0}/{1}.", game.Session.Scoring.CompletedAchievementsCount, Scoring.MAX_ACHIEVEMENTS));
                if (game.Session.Scoring.CompletedAchievementsCount >= Scoring.MAX_ACHIEVEMENTS)
                {
                    graveyard.Append("*** You achieved everything! You can consider having won this version of the game! CONGRATULATIONS! ***");
                }
                else
                    graveyard.Append("(unlock all the achievements to win this game version)");
                graveyard.Append("(later versions of the game will feature real winning conditions and multiple endings...)");
            }
            graveyard.Append(" ");

            graveyard.Append("> DEATH");
            graveyard.Append(string.Format("{0} in {1}.", game.Session.Scoring.DeathReason, game.Session.Scoring.DeathPlace));
            graveyard.Append(" ");

            graveyard.Append("> KILLS");
            if (game.Session.Scoring.HasNoKills)
            {
                graveyard.Append(string.Format("{0} was a pacifist. Or too scared to fight.", heOrShe));
            }
            else
            {
                // models kill list.
                foreach (Scoring.KillData killData in game.Session.Scoring.Kills)
                {
                    string modelName = killData.Amount > 1 ? Models.Actors[killData.ActorModelID].PluralName : Models.Actors[killData.ActorModelID].Name;
                    graveyard.Append(string.Format("{0,4} {1}.", killData.Amount, modelName));
                }
            }
            // murders? only livings.
            if (!game.Player.Model.Abilities.IsUndead)
            {
                if (game.Player.MurdersCounter > 0)
                {
                    graveyard.Append(string.Format("{0} committed {1} murder{2}!", heOrShe, game.Player.MurdersCounter, game.Player.MurdersCounter > 1 ? "s" : ""));
                }
            }

            graveyard.Append(" ");

            graveyard.Append("> FUN FACTS!");
            graveyard.Append(string.Format("While {0} has died, others are still having fun!", name));
            string[] funFacts = game.CompileDistrictFunFacts(game.Player.Location.Map.District);
            for (int i = 0; i < funFacts.Length; i++)
                graveyard.Append(funFacts[i]);
            graveyard.Append("");

            graveyard.Append("> SKILLS");
            if (game.Player.Sheet.SkillTable.Skills == null)
            {
                graveyard.Append(string.Format("{0} was a jack of all trades. Or an incompetent.", heOrShe));
            }
            else
            {
                foreach (Skill sk in game.Player.Sheet.SkillTable.Skills)
                {
                    graveyard.Append(string.Format("{0}-{1}.", sk.Level, Skills.Name(sk.ID)));
                }
            }
            graveyard.Append(" ");

            graveyard.Append("> INVENTORY");
            if (game.Player.Inventory.IsEmpty)
            {
                graveyard.Append(string.Format("{0} was humble. Or dirt poor.", heOrShe));
            }
            else
            {
                foreach (Item it in game.Player.Inventory.Items)
                {
                    string desc = game.DescribeItemShort(it);
                    if (it.IsEquipped)
                        graveyard.Append(string.Format("- {0} (equipped).", desc));
                    else
                        graveyard.Append(string.Format("- {0}.", desc));
                }
            }
            graveyard.Append(" ");

            graveyard.Append("> FOLLOWERS");
            if (game.Session.Scoring.FollowersWhendDied == null || game.Session.Scoring.FollowersWhendDied.Count == 0)
            {
                graveyard.Append(string.Format("{0} was doing fine alone. Or everyone else was dead.", heOrShe));
            }
            else
            {
                // names.
                StringBuilder sb = new StringBuilder(string.Format("{0} was leading", heOrShe));
                bool firstFo = true;
                int i = 0;
                int count = game.Session.Scoring.FollowersWhendDied.Count;
                foreach (Actor fo in game.Session.Scoring.FollowersWhendDied)
                {
                    if (firstFo)
                        sb.Append(" ");
                    else
                    {
                        if (i == count)
                            sb.Append(".");
                        else if (i == count - 1)
                            sb.Append(" and ");
                        else
                            sb.Append(", ");
                    }
                    sb.Append(fo.TheName);
                    ++i;
                    firstFo = false;
                }
                sb.Append(".");
                graveyard.Append(sb.ToString());

                // skills.
                foreach (Actor fo in game.Session.Scoring.FollowersWhendDied)
                {
                    graveyard.Append(string.Format("{0} skills : ", fo.Name));
                    if (fo.Sheet.SkillTable != null && fo.Sheet.SkillTable.Skills != null)
                    {
                        foreach (Skill sk in fo.Sheet.SkillTable.Skills)
                        {
                            graveyard.Append(string.Format("{0}-{1}.", sk.Level, Skills.Name(sk.ID)));
                        }
                    }
                }
            }
            graveyard.Append(" ");

            graveyard.Append("> EVENTS");
            if (game.Session.Scoring.HasNoEvents)
            {
                graveyard.Append(string.Format("{0} had a quiet life. Or dull and boring.", heOrShe));
            }
            else
            {
                foreach (Scoring.GameEventData ev in game.Session.Scoring.Events)
                {
                    WorldTime evTime = new WorldTime { TurnCounter = ev.Turn };
                    graveyard.Append(string.Format("- {0,13} : {1}", evTime.ToString(), ev.Text));
                }
            }
            graveyard.Append(" ");

            graveyard.Append("> CUSTOM OPTIONS");
            graveyard.Append(string.Format("- difficulty rating of {0}%.", (int)(100 * game.Session.Scoring.DifficultyRating)));
            if (RogueGame.Options.IsPermadeathOn)
                graveyard.Append(string.Format("- {0} : yes.", GameOptions.Name(GameOptions.IDs.GAME_PERMADEATH)));
            if (!RogueGame.Options.AllowUndeadsEvolution && Rules.HasEvolution(game.Session.GameMode)) // alpha10 only if manually disabled
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION), RogueGame.Options.AllowUndeadsEvolution ? "yes" : "no"));
            if (RogueGame.Options.CitySize != GameOptions.DEFAULT_CITY_SIZE)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_CITY_SIZE), RogueGame.Options.CitySize));
            if (RogueGame.Options.DayZeroUndeadsPercent != GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT)
                graveyard.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT), RogueGame.Options.DayZeroUndeadsPercent));
            if (RogueGame.Options.DistrictSize != GameOptions.DEFAULT_DISTRICT_SIZE)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_DISTRICT_SIZE), RogueGame.Options.DistrictSize));
            if (RogueGame.Options.MaxCivilians != GameOptions.DEFAULT_MAX_CIVILIANS)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_CIVILIANS), RogueGame.Options.MaxCivilians));
            if (RogueGame.Options.MaxUndeads != GameOptions.DEFAULT_MAX_UNDEADS)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_UNDEADS), RogueGame.Options.MaxUndeads));
            if (!RogueGame.Options.NPCCanStarveToDeath)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH), RogueGame.Options.NPCCanStarveToDeath ? "yes" : "no"));
            if (RogueGame.Options.StarvedZombificationChance != GameOptions.DEFAULT_STARVED_ZOMBIFICATION_CHANCE)
                graveyard.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE), RogueGame.Options.StarvedZombificationChance));
            if (!RogueGame.Options.RevealStartingDistrict)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT), RogueGame.Options.RevealStartingDistrict ? "yes" : "no"));
            if (RogueGame.Options.SimulateDistricts != GameOptions.DEFAULT_SIM_DISTRICTS)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_DISTRICTS), GameOptions.Name(RogueGame.Options.SimulateDistricts)));
            if (RogueGame.Options.SimulateWhenSleeping)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_SLEEP), RogueGame.Options.SimulateWhenSleeping ? "yes" : "no"));
            if (RogueGame.Options.ZombieInvasionDailyIncrease != GameOptions.DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE)
                graveyard.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE), RogueGame.Options.ZombieInvasionDailyIncrease));
            if (RogueGame.Options.ZombificationChance != GameOptions.DEFAULT_ZOMBIFICATION_CHANCE)
                graveyard.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE), RogueGame.Options.ZombificationChance));
            if (RogueGame.Options.MaxReincarnations != GameOptions.DEFAULT_MAX_REINCARNATIONS)
                graveyard.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_REINCARNATIONS), RogueGame.Options.MaxReincarnations));
            graveyard.Append(" ");

            graveyard.Append("> R.I.P");
            graveyard.Append(string.Format("May {0} soul rest in peace.", hisOrHer));
            graveyard.Append(string.Format("For {0} body is now a meal for evil.", hisOrHer));
            graveyard.Append("The End.");

            graveyard.FormatLines(Ui.TEXTFILE_CHARS_PER_LINE);

            /////////////////////
            // Save to graveyard
            /////////////////////
            string graveFile = GetNewGraveyardFilename();
            confirmError = !graveyard.Save(graveFile);

            line = 0;
        }

        string AorAn(string name)
        {
            char c = name[0];
            return (c == 'a' || c == 'e' || c == 'i' || c == 'u' ? "an " : "a ") + name;
        }

        string GetNewGraveyardFilename()
        {
            int i = 0;
            while (true)
            {
                string filename = $"{RogueGame.GraveyardPath}grave_{i:D3}.txt";
                if (!File.Exists(filename))
                    return filename;
                ++i;
            }
        }

        public override void Draw()
        {
            int gx = 0, gy = 0;

            if (confirmError)
            {
                ui.Clear(Color.Black);
                ui.DrawStringBold(Color.Yellow, "Saving post mortem to graveyard...", 0, 0);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.Red, "Could not save to graveyard.", 0, gy);
                ui.DrawFootnote(Color.White, "press ENTER");
            }
            else
            {
                // header.
                ui.Clear(Color.Black);
                ui.DrawHeader();
                gy += Ui.BOLD_LINE_SPACING;

                // text.
                int linesThisPage = 0, iLine = line;
                ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy);
                gy += Ui.BOLD_LINE_SPACING;
                while (linesThisPage < Ui.TEXTFILE_LINES_PER_PAGE && iLine < graveyard.FormatedLines.Count)
                {
                    string str = graveyard.FormatedLines[iLine];
                    ui.DrawStringBold(Color.White, str, gx, gy);
                    gy += Ui.BOLD_LINE_SPACING;
                    ++iLine;
                    ++linesThisPage;
                }

                // foot.
                ui.DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, Ui.CANVAS_HEIGHT - 2 * Ui.BOLD_LINE_SPACING);
                if (iLine < graveyard.FormatedLines.Count)
                    ui.DrawFootnote(Color.White, "press ENTER for more, ESC to skip");
                else
                    ui.DrawFootnote(Color.White, "press ENTER to leave, ESC to skip");
            }
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();
            if (key == Key.Enter)
            {
                if (confirmError)
                {
                    confirmError = false;
                    return;
                }

                line += Ui.TEXTFILE_LINES_PER_PAGE;
                if (line >= graveyard.FormatedLines.Count)
                {
                    game.PopState();
                    game.PushState<MainMenuState>();

                    /////////////
                    // Hi Score?
                    /////////////
                    StringBuilder skillsSb = new StringBuilder();
                    if (game.Player.Sheet.SkillTable.Skills != null)
                    {
                        foreach (Skill sk in game.Player.Sheet.SkillTable.Skills)
                        {
                            skillsSb.AppendFormat("{0}-{1} ", sk.Level, Skills.Name(sk.ID));
                        }
                    }
                    HiScore newHiScore = HiScore.FromScoring(name, game.Session.Scoring, skillsSb.ToString());
                    if (game.HiScoreTable.Register(newHiScore))
                    {
                        HiScoreTable.Save(game.HiScoreTable, RogueGame.HiScoreFile);
                        game.PushState<HiScoresState>();
                    }
                }
            }
        }
    }
}
