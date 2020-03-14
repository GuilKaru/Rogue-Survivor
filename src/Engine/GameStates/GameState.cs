using RogueSurvivor.Engine.Interfaces;

namespace RogueSurvivor.Engine.GameStates
{
    abstract class GameState
    {
        public IGame game;
        public IRogueUI ui;
        public IMusicManager musicManager;

        public virtual void Init() { }
        public virtual void Enter() { }
        public abstract void Update(double dt);
        public abstract void Draw();
    }
}
