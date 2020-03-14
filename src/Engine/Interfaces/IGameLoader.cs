using System;

namespace RogueSurvivor.Engine.Interfaces
{
    interface IGameLoader
    {
        void CategoryStart(string text);
        void CategoryEnd(string logText = null);
        void LoadImage(string text);
        void Action(Action action);
    }
}
