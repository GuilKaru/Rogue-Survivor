using RogueSurvivor.Data;
using RogueSurvivor.Engine.GameStates;
using RogueSurvivor.Gameplay;

namespace RogueSurvivor.Engine.Interfaces
{
    interface IGame
    {
        GameActors Actors { get; }
        HiScoreTable HiScoreTable { get; }
        GameHintsStatus Hints { get; }
        Keybindings KeyBindings { get; }
        TextFile Manual { get; }
        ref GameOptions Options { get; }
        Rules Rules { get; }
        Session Session { get; }
        World World { get; }

        string SaveFilePath { get; }
        string HiScoreTextFilePath { get; }
        string KeyBindingsPath { get; }
        string UserOptionsFilePath { get; }

        void Init(IGameLoader gameLoader);
        void Draw();
        bool Update(double dt);
        void SetState<State>(bool dispose = false) where State : GameState;
        void PushState<State>() where State : GameState;
        void PopState();
        void Exit();

        void ApplyOptions();
        void SetMode(GameMode mode);
        void GetAdvisorHintText(AdvisorHint hint, out string title, out string[] body);
    }
}
