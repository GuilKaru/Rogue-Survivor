using RogueSurvivor.Data;
using System;

namespace RogueSurvivor.Engine.Actions
{
    class ActionTakeLead : ActorAction
    {
        readonly Actor m_Target;

        public ActionTakeLead(Actor actor, RogueGame game, Actor target)
            : base(actor, game)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorTakeLead(m_Actor, m_Target);
        }

        public override void Perform()
        {
            // steal lead vs take lead
            if (m_Target.HasLeader)
                m_Game.DoStealLead(m_Actor, m_Target);
            else
                m_Game.DoTakeLead(m_Actor, m_Target);
        }
    }
}
