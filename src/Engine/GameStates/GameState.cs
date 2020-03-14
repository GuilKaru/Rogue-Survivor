using RogueSurvivor.Engine.Interfaces;

namespace RogueSurvivor.Engine.GameStates
{
    abstract class GameState
    {
        public RogueGame game;
        public IRogueUI ui;

        public virtual void Init() { }
        public virtual void Enter() { }
        public abstract void Update(double dt);
        public abstract void Draw();
    }
}
