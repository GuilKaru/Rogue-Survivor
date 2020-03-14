using RogueSurvivor.Engine.GameStates;

namespace RogueSurvivor.Engine.Interfaces
{
    interface IGame
    {
        void Init(IGameLoader gameLoader);
        void Draw();
        bool Update();
        void SetState<State>(bool dispose = false) where State : GameState;
        void PushState<State>() where State : GameState;
        void PopState();
        void Exit();
    }
}
