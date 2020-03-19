using RogueSurvivor.Data;
using RogueSurvivor.Engine.Items;
using System;

namespace RogueSurvivor.Engine.Actions
{
    class ActionSprayOdorSuppressor : ActorAction
    {
        readonly ItemSprayScent m_Spray;
        readonly Actor m_SprayOn;

        public ActionSprayOdorSuppressor(Actor actor, RogueGame game, ItemSprayScent spray, Actor sprayOn)
            : base(actor, game)
        {
            if (sprayOn == null)
                throw new ArgumentNullException("target");

            m_Spray = spray;
            m_SprayOn = sprayOn;
        }

        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorSprayOdorSuppressor(m_Actor, m_Spray, m_SprayOn, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoSprayOdorSuppressor(m_Actor, m_Spray, m_SprayOn);
        }
    }
}
