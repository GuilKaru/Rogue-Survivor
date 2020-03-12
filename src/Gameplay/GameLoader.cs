using RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace RogueSurvivor.Gameplay
{
    class GameLoader
    {
        private enum TaskType
        {
            Category,
            CategoryDone,
            Image,
            Action
        }

        private class Task
        {
            public TaskType type;
            public string text;
            public Action action;
        }

        const int BOLD_LINE_SPACING = 14;

        private List<Task> tasks = new List<Task>();
        private List<string> texts = new List<string>();
        private int offset = 0;
        private Stopwatch stopwatch = new Stopwatch();
        private bool categoryOpen;

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

        public void LoadImage(string text)
        {
            tasks.Add(new Task
            {
                type = TaskType.Image,
                text = text
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

        public bool Process()
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
                    case TaskType.Image:
                        GameImages.Load(task.text);
                        break;
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

        public void Draw(IRogueUI ui)
        {
            ui.UI_Clear(Color.Black);
            ui.UI_DrawStringBold(Color.Yellow, "Loading Rogue Survivor, please wait...", 0, 0);
            int y = BOLD_LINE_SPACING;
            foreach (string text in texts)
            {
                ui.UI_DrawStringBold(Color.White, text, 0, y);
                y += BOLD_LINE_SPACING;
            }
        }
    }
}
