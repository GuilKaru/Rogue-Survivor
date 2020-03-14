using RogueSurvivor.Engine.Interfaces;
using RogueSurvivor.Gameplay;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSurvivor.Engine.GameStates
{
    class SelectSkillState : GameState
    {
        Skills.IDs[] allSkills;
        string[] menuEntries;
        string[] skillDesc;
        int selected;
        Skills.IDs confirmSkill;
        bool confirmChoice;

        public override void Init()
        {
            allSkills = new Skills.IDs[(int)Skills.IDs._LAST_LIVING + 1];
            menuEntries = new string[allSkills.Length + 1];
            skillDesc = new string[allSkills.Length + 1];
            menuEntries[0] = "*Random*";
            skillDesc[0] = "(picks a skill at random for you)";
            for (int i = (int)Skills.IDs._FIRST_LIVING; i < (int)Skills.IDs._LAST_LIVING + 1; i++)
            {
                allSkills[i] = (Skills.IDs)i;
                menuEntries[i + 1] = Skills.Name(allSkills[i]);
                skillDesc[i + 1] = string.Format("{0} max - {1}", Skills.MaxSkillLevel(i), Skills.DescribeSkillShort(allSkills[i]));
            }
        }

        public override void Enter()
        {
            selected = 0;
            confirmChoice = false;
        }

        public override void Draw()
        {
            ui.Clear(Color.Black);
            int gx, gy;
            gx = gy = 0;
            ui.DrawStringBold(Color.Yellow, string.Format("[{0}] New {1} Character - Choose Starting Skill",
                Session.DescGameMode(game.Session.GameMode),
                game.Session.charGen.IsMale ? "Male" : "Female"), gx, gy);
            gy += 2 * Ui.BOLD_LINE_SPACING;
            ui.DrawMenuOrOptions(selected, Color.White, menuEntries, Color.LightGray, skillDesc, gx, ref gy);
            ui.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");

            if (confirmChoice)
            {
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.White, string.Format("Skill : {0}.", Skills.Name(confirmSkill)), gx, gy);
                gy += Ui.BOLD_LINE_SPACING;
                ui.DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy);
            }
        }

        public override void Update(double dt)
        {
            Key key = ui.ReadKey();

            if (confirmChoice)
            {
                if (key == Key.Y)
                {
                    game.Session.charGen.StartingSkill = confirmSkill;
                    game.PopState();
                }
                else if (key == Key.N || key == Key.Escape)
                    confirmChoice = false;
                return;
            }

            switch (key)
            {
                case Key.Up:
                    if (selected > 0)
                        --selected;
                    else
                        selected = menuEntries.Length - 1;
                    break;

                case Key.Down:
                    selected = (selected + 1) % menuEntries.Length;
                    break;

                case Key.Escape:
                    game.PopState();
                    break;

                case Key.Enter:
                    if (selected == 0) // random
                        confirmSkill = Skills.RollLiving(game.Session.charGenRoller);
                    else
                        confirmSkill = (Skills.IDs)(selected - 1 + (int)Skills.IDs._FIRST);
                    confirmChoice = true;
                    break;
            }
        }
    }
}
