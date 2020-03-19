using RogueSurvivor.Data;
using RogueSurvivor.Engine;
using RogueSurvivor.Engine.Actions;
using RogueSurvivor.Engine.AI;
using RogueSurvivor.Gameplay.AI.Sensors;
using RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// CHAR Guard AI.
    /// </summary>
    class CHARGuardAI : OrderableAI
    {
        const int LOS_MEMORY = 10;

        static string[] FIGHT_EMOTES =
        {
            "Go away",
            "Damn it I'm trapped!",
            "Hey"
        };

        LOSSensor m_LOSSensor;
        MemorizedSensor m_MemorizedSensor;

        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS);
            m_MemorizedSensor = new MemorizedSensor(m_LOSSensor, LOS_MEMORY);
        }

        public override void TakeControl(Actor actor)
        {
            base.TakeControl(actor);
        }

        protected override List<Percept> UpdateSensors(RogueGame game)
        {
            return m_MemorizedSensor.Sense(game, m_Actor);
        }

        protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
        {
            List<Percept> mapPercepts = FilterSameMap(game, percepts);

            // don't run by default.
            m_Actor.IsRunning = false;

            // 0. Equip best item
            ActorAction bestEquip = BehaviorEquipBestItems(game, true, true);
            if (bestEquip != null)
            {
                return bestEquip;
            }

            // 1. Follow order
            if (this.Order != null)
            {
                ActorAction orderAction = ExecuteOrder(game, this.Order, mapPercepts, null);
                if (orderAction == null)
                    SetOrder(null);
                else
                {
                    m_Actor.Activity = Activity.FOLLOWING_ORDER;
                    return orderAction;
                }
            }

            ///////////////////////////////////////
            // 1 fire at nearest enemy.
            // 2 hit adjacent enemy.
            // 3 warn trepassers.
            // 4 shout
            // 5 rest if tired
            // 6 charge enemy
            // 7 sleep when sleepy.
            // 8 follow leader.
            // 9 wander in CHAR office.
            // 10 wander.
            //////////////////////////////////////

            // don't run by default.
            m_Actor.IsRunning = false;

            // get data.
            List<Percept> allEnemies = FilterEnemies(game, mapPercepts);
            List<Percept> currentEnemies = FilterCurrent(game, allEnemies);
            bool checkOurLeader = m_Actor.HasLeader && !DontFollowLeader;
            bool hasAnyEnemies = allEnemies != null;

            // 1 fire at nearest enemy.
            if (currentEnemies != null)
            {
                List<Percept> fireTargets = FilterFireTargets(game, currentEnemies);
                if (fireTargets != null)
                {
                    Percept nearestTarget = FilterNearest(game, fireTargets);
                    Actor targetActor = nearestTarget.Percepted as Actor;

                    ActorAction fireAction = BehaviorRangedAttack(game, nearestTarget);
                    if (fireAction != null)
                    {
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = targetActor;
                        return fireAction;
                    }
                }
            }

            // 2 hit adjacent enemy
            if (currentEnemies != null)
            {
                Percept nearestEnemy = FilterNearest(game, currentEnemies);
                Actor targetActor = nearestEnemy.Percepted as Actor;

                // fight or flee?
                RouteFinder.SpecialActions allowedChargeActions = RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS;
                ActorAction fightOrFlee = BehaviorFightOrFlee(game, currentEnemies, true, true, ActorCourage.COURAGEOUS, FIGHT_EMOTES, allowedChargeActions);
                if (fightOrFlee != null)
                {
                    return fightOrFlee;
                }
            }

            // 3 warn trepassers.
            List<Percept> nonEnemies = FilterNonEnemies(game, mapPercepts);
            if (nonEnemies != null)
            {
                List<Percept> trespassers = Filter(game, nonEnemies, (p) =>
                {
                    Actor other = (p.Percepted as Actor);
                    if (other.Faction == game.Factions.TheCHARCorporation)
                        return false;

                    if (p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter)
                        return false;

                    return game.IsInCHARProperty(other.Location);
                });
                if (trespassers != null)
                {
                    // Hey YOU!
                    Actor trespasser = FilterNearest(game, trespassers).Percepted as Actor;

                    game.DoMakeAggression(m_Actor, trespasser);

                    m_Actor.Activity = Activity.FIGHTING;
                    m_Actor.TargetActor = trespasser;
                    return new ActionSay(m_Actor, game, trespasser, "Hey YOU!", RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER);
                }
            }

            // 4 shout
            if (hasAnyEnemies)
            {
                if (nonEnemies != null)
                {
                    ActorAction shoutAction = BehaviorWarnFriends(game, nonEnemies, FilterNearest(game, allEnemies).Percepted as Actor);
                    if (shoutAction != null)
                    {
                        m_Actor.Activity = Activity.IDLE;
                        return shoutAction;
                    }
                }
            }

            // 5 rest if tired
            ActorAction restAction = BehaviorRestIfTired(game);
            if (restAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor, game);
            }

            // 6 charge/chase enemy
            if (allEnemies != null)
            {
                Percept chasePercept = FilterNearest(game, allEnemies);

                // cheat a bit for good chasing behavior.
                if (m_Actor.Location == chasePercept.Location)
                {
                    // memorized location reached, chase now the actor directly (cheat so they appear more intelligent)
                    Actor chasedActor = chasePercept.Percepted as Actor;
                    chasePercept = new Percept(chasedActor, m_Actor.Location.Map.LocalTime.TurnCounter, chasedActor.Location);
                }

                // chase only if reachable
                if (CanReachSimple(game, chasePercept.Location.Position, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP))
                {
                    // chase.
                    ActorAction chargeAction = BehaviorChargeEnemy(game, chasePercept, false, false);
                    if (chargeAction != null)
                    {
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = chasePercept.Percepted as Actor;
                        return chargeAction;
                    }
                }
            }

            // 7 sleep when sleepy
            if (game.Rules.IsActorSleepy(m_Actor) && !hasAnyEnemies)
            {
                ActorAction sleepAction = BehaviorSleep(game, m_LOSSensor.FOV);
                if (sleepAction != null)
                {
                    if (sleepAction is ActionSleep)
                        m_Actor.Activity = Activity.SLEEPING;
                    return sleepAction;
                }
            }

            // 8 follow leader
            if (checkOurLeader)
            {
                Point lastKnownLeaderPosition = m_Actor.Leader.Location.Position;
                bool isLeaderVisible = m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
                ActorAction followAction = BehaviorFollowActor(game, m_Actor.Leader, lastKnownLeaderPosition, isLeaderVisible, 1);
                if (followAction != null)
                {
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
                    return followAction;
                }
            }

            // 9 wander in CHAR office.
            ActorAction wanderInOfficeAction = BehaviorWander(game, (loc) => RogueGame.IsInCHAROffice(loc), null);
            if (wanderInOfficeAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return wanderInOfficeAction;
            }

            // 10 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(game, null);
        }
    }
}
