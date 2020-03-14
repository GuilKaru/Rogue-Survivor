using RogueSurvivor.Engine.GameStates;

namespace RogueSurvivor.Engine.Interfaces
{
    interface IGame
    {
        HiScoreTable HiScoreTable { get; }
        GameHintsStatus Hints { get; }
        TextFile Manual { get; }

        string SaveFilePath { get; }
        string HiScoreTextFilePath { get; }

        void Init(IGameLoader gameLoader);
        void Draw();
        bool Update(double dt);
        void SetState<State>(bool dispose = false) where State : GameState;
        void PushState<State>() where State : GameState;
        void PopState();
        void Exit();

        void GetAdvisorHintText(AdvisorHint hint, out string title, out string[] body);
    }
}
