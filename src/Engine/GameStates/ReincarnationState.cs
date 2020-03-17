using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSurvivor.Engine.GameStates
{
    class ReincarnationState : GameState
    {
        enum State
        {
            NoMore,
            Ask,
            Wait,
            Select
        }

        State state;
        int selected, countLivings, countUndead, countFollower;
        Actor randomR, livingR, undeadR, followerR, killerR, zombifiedR;
        string[] funFacts, entries, values;

        public override void Init()
        {
            // play music.
            game.MusicManager.PlayLooping(GameMusics.LIMBO, MusicPriority.PRIORITY_EVENT);

            // ask question or no more lives left.
            if (game.Session.Scoring.ReincarnationNumber >= RogueGame.Options.MaxReincarnations)
            {
                // no more lives left.
                state = State.NoMore;
            }
            else
            {
                // one more life available.
                state = State.Ask;
            }
        }

        public override void Draw()
        {
            int gx = 0, gy = 0;
            ui.Clear(Color.Black);

            switch (state)
            {
                case State.Ask:
                case State.NoMore:
                    // show screen.
                    ui.DrawStringBold(Color.Yellow, "Limbo", gx, gy);
                    gy += 2 * Ui.BOLD_LINE_SPACING;
                    ui.DrawStringBold(Color.White, string.Format("Leave body {0}/{1}.", (1 + game.Session.Scoring.ReincarnationNumber), (1 + RogueGame.Options.MaxReincarnations)), gx, gy);
                    gy += Ui.BOLD_LINE_SPACING;
                    ui.DrawStringBold(Color.White, "Remember lives.", gx, gy);
                    gy += Ui.BOLD_LINE_SPACING;
                    ui.DrawStringBold(Color.White, "Remember purpose.", gx, gy);
                    gy += Ui.BOLD_LINE_SPACING;
                    ui.DrawStringBold(Color.White, "Clear again.", gx, gy);
                    gy += Ui.BOLD_LINE_SPACING;

                    // ask question or no more lives left.
                    if (state == State.NoMore)
                    {
                        // no more lives left.
                        ui.DrawStringBold(Color.LightGreen, "Humans interesting.", gx, gy);
                        gy += Ui.BOLD_LINE_SPACING;
                        ui.DrawStringBold(Color.LightGreen, "Time to leave.", gx, gy);
                        gy += Ui.BOLD_LINE_SPACING;
                        gy += 2 * Ui.BOLD_LINE_SPACING;
                        ui.DrawStringBold(Color.Yellow, "No more reincarnations left.", gx, gy);
                        ui.DrawFootnote(Color.White, "press ENTER");
                    }
                    else
                    {
                        // one more life available.
                        ui.DrawStringBold(Color.White, "Leave?", gx, gy);
                        gy += Ui.BOLD_LINE_SPACING;
                        ui.DrawStringBold(Color.White, "Live?", gx, gy);

                        gy += 2 * Ui.BOLD_LINE_SPACING;
                        ui.DrawStringBold(Color.Yellow, "Reincarnate? Y to confirm, N to cancel.", gx, gy);
                    }
                    break;

                case State.Wait:
                    ui.DrawStringBold(Color.Yellow, "Reincarnation - Purgatory", 0, 0);
                    ui.DrawStringBold(Color.White, "(preparing reincarnations, please wait...)", 0, 2 * Ui.BOLD_LINE_SPACING);
                    break;

                case State.Select:
                    ui.DrawStringBold(Color.Yellow, "Reincarnation - Choose Avatar", gx, gy);
                    gy += 2 * Ui.BOLD_LINE_SPACING;

                    ui.DrawMenuOrOptions(selected, Color.White, entries, Color.LightGreen, values, gx, ref gy);
                    gy += 2 * Ui.BOLD_LINE_SPACING;

                    ui.DrawStringBold(Color.Pink, ".-* District Fun Facts! *-.", gx, gy);
                    gy += Ui.BOLD_LINE_SPACING;
                    ui.DrawStringBold(Color.Pink, string.Format("at current date : {0}.", new WorldTime(game.Session.WorldTime.TurnCounter).ToString()), gx, gy);
                    gy += 2 * Ui.BOLD_LINE_SPACING;
                    for (int i = 0; i < funFacts.Length; i++)
                    {
                        ui.DrawStringBold(Color.Pink, funFacts[i], gx, gy);
                        gy += Ui.BOLD_LINE_SPACING;
                    }

                    ui.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel and end game");
                    break;
            }
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();
            switch (state)
            {
                case State.NoMore:
                    if (key == Key.Enter)
                        game.PopState();
                    break;

                case State.Ask:
                    if (key == Key.Y)
                        state = State.Wait;
                    else if (key == Key.N || key == Key.Escape)
                        game.PopState();
                    break;

                case State.Wait:
                    PrepareReincarnations();
                    break;

                case State.Select:
                    switch (key)
                    {
                        case Key.Up:
                            if (selected > 0)
                                --selected;
                            else
                                selected = entries.Length - 1;
                            break;

                        case Key.Down:
                            selected = (selected + 1) % entries.Length;
                            break;

                        case Key.Escape:
                            game.PopState();
                            break;

                        case Key.Enter:
                            {
                                Actor avatar = null;
                                switch (selected)
                                {
                                    case 0: // random actor
                                        avatar = randomR;
                                        break;
                                    case 1: // random survivor
                                        avatar = livingR;
                                        break;
                                    case 2: // random undead
                                        avatar = undeadR;
                                        break;
                                    case 3: // random follower
                                        avatar = followerR;
                                        break;
                                    case 4: // killer
                                        avatar = killerR;
                                        break;
                                    case 5: // zombified
                                        avatar = zombifiedR;
                                        break;
                                }
                                if (avatar != null)
                                {
                                    // !FIXME
                                }
                                game.PopState();
                                break;
                            }
                    }
                    break;
            }
        }

        void PrepareReincarnations()
        {
            // Decide available reincarnation targets.
            randomR = game.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_ACTOR, out _);
            livingR = game.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_LIVING, out countLivings);
            undeadR = game.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_UNDEAD, out countUndead);
            followerR = game.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_FOLLOWER, out countFollower);
            killerR = game.FindReincarnationAvatar(GameOptions.ReincMode.KILLER, out _);
            zombifiedR = game.FindReincarnationAvatar(GameOptions.ReincMode.ZOMBIFIED, out _);

            // Get fun facts.
            funFacts = game.CompileDistrictFunFacts(game.Player.Location.Map.District);

            // Reincarnate.
            // Choose avatar from a set of reincarnation modes.
            entries = new string[]
            {
                GameOptions.Name(GameOptions.ReincMode.RANDOM_ACTOR),
                GameOptions.Name(GameOptions.ReincMode.RANDOM_LIVING),
                GameOptions.Name(GameOptions.ReincMode.RANDOM_UNDEAD),
                GameOptions.Name(GameOptions.ReincMode.RANDOM_FOLLOWER),
                GameOptions.Name(GameOptions.ReincMode.KILLER),
                GameOptions.Name(GameOptions.ReincMode.ZOMBIFIED)
            };
            values = new string[]
            {
                DescribeAvatar(randomR),
                string.Format("{0}   (out of {1} possibilities)", DescribeAvatar(livingR), countLivings),
                string.Format("{0}   (out of {1} possibilities)", DescribeAvatar(undeadR), countUndead),
                string.Format("{0}   (out of {1} possibilities)", DescribeAvatar(followerR), countFollower),
                DescribeAvatar(killerR),
                DescribeAvatar(zombifiedR)
            };

            state = State.Select;
            selected = 0;
        }

        string DescribeAvatar(Actor a)
        {
            if (a == null)
                return "(N/A)";
            bool isLeader = a.CountFollowers > 0;
            bool isFollower = a.HasLeader;
            return string.Format("{0}, a {1}{2}", a.Name, a.Model.Name, isLeader ? ", leader" : isFollower ? ", follower" : "");
        }
    }
}
