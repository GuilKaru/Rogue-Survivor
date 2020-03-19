using RogueSurvivor.Engine;
using System;

namespace RogueSurvivor.Data
{
    [Serializable]
    abstract class ActorController
    {
        protected Actor m_Actor;

        public Actor ControlledActor { get { return m_Actor; } }

        public virtual void TakeControl(Actor actor)
        {
            m_Actor = actor;
        }

        public virtual void LeaveControl()
        {
            m_Actor = null;
        }

        public abstract ActorAction GetAction(RogueGame game);
    }
}
