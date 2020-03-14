using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;

namespace RogueSurvivor.Engine.GameStates
{
    class LoadState : LoadScreenState, IGameLoader
    {
        public override void Enter()
        {
            Logger.WriteLine(Logger.Stage.INIT, "Preparing items to load...");
            GameImages.LoadResources(this, ui.Graphics);
            game.Init(this);
        }

        public override void Draw()
        {
            Draw("Loading Rogue Survivor, please wait...");
        }

        public override void Update(double dt)
        {
            if (Process())
                game.SetState<MainMenuState>(dispose: true);
        }

        public void LoadImage(string text)
        {
            Action(() => GameImages.Load(text));
        }
    }
}
