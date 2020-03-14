using RogueSurvivor.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace RogueSurvivor.Engine.GameStates
{
    abstract class LoadScreenState : BaseGameState
    {
        private enum TaskType
        {
            Category,
            CategoryDone,
            Action
        }

        private class Task
        {
            public TaskType type;
            public string text;
            public Action action;
        }

        private List<Task> tasks = new List<Task>();
        private List<string> texts = new List<string>();
        private int offset = 0;
        private Stopwatch stopwatch = new Stopwatch();
        private bool categoryOpen;

        protected void Draw(string title)
        {
            ui.Clear(Color.Black);
            ui.DrawStringBold(Color.Yellow, "Loading Rogue Survivor, please wait...", 0, 0);
            int y = Ui.BOLD_LINE_SPACING;
            foreach (string text in texts)
            {
                ui.DrawStringBold(Color.White, text, 0, y);
                y += Ui.BOLD_LINE_SPACING;
            }
        }

        public override void Update(double dt)
        {
            if (Process())
                game.SetState<MainMenuState>(dispose: true);
        }

        public void Category(string text, Action action)
        {
            CategoryStart(text);
            Action(action);
            CategoryEnd();
        }

        public void CategoryStart(string text)
        {
            tasks.Add(new Task
            {
                type = TaskType.Category,
                text = text
            });
        }

        public void CategoryEnd(string logText = null)
        {
            tasks.Add(new Task
            {
                type = TaskType.CategoryDone,
                text = logText
            });
        }

        public void Action(Action action)
        {
            tasks.Add(new Task
            {
                type = TaskType.Action,
                action = action
            });
        }

        protected bool Process()
        {
            stopwatch.Restart();
            double dt = 0;

            while (offset < tasks.Count)
            {
                Task task = tasks[offset];
                ++offset;

                switch (task.type)
                {
                    case TaskType.Category:
                        texts.Add(task.text);
                        Logger.WriteLine(Logger.Stage.INIT, task.text);
                        categoryOpen = true;
                        return false;
                    case TaskType.CategoryDone:
                        {
                            int index = texts.Count - 1;
                            texts[index] = texts[index] + " done!";
                            if (task.text != null)
                                Logger.WriteLine(Logger.Stage.INIT, task.text);
                            categoryOpen = false;
                        }
                        return false;
                    case TaskType.Action:
                        task.action();
                        break;
                }

                dt += stopwatch.Elapsed.TotalSeconds;
                if (dt >= 0.1)
                {
                    if (categoryOpen)
                    {
                        int index = texts.Count - 1;
                        texts[index] = texts[index] + ".";
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
