using RogueSurvivor.Engine.Interfaces;

namespace RogueSurvivor.Engine.GameStates
{
    abstract class GameState
    {
        public IGame game;
        public IRogueUI ui;
        public IMusicManager musicManager;

        public abstract void Enter();
        public abstract void Update();
        public abstract void Draw();
    }
}
