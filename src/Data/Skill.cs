using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RogueSurvivor.Data
{
    [Serializable]
    class Skill
    {
        int m_ID;
        int m_Level;
        int m_BuffedLevel; // keep track on how many levels you have buffed the skill.
        public int ID
        {
            get { return m_ID; }
        }

        public int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        public int BuffedLevel
        {
            get { return m_BuffedLevel; }
            set { m_BuffedLevel = value; }
        }

        public Skill(int id)
        {
            m_ID = id;
        }
    }

    [Serializable]
    class SkillTable
    {
        Dictionary<int, Skill> m_Table;   // allocated only if needed (some actors have 0 skills)

        /// <summary>
        /// Get all skills null if no skills.
        /// </summary>
        public IEnumerable<Skill> Skills
        {
            get
            {
                if (m_Table == null)
                    return null;

                return m_Table.Values;
            }
        }

        /// <summary>
        /// List all non-zero skills ids as an array; null if no skills.
        /// </summary>
        public int[] SkillsList
        {
            get
            {
                if (m_Table == null)
                    return null;

                int[] array = new int[CountSkills];
                int i = 0;
                foreach (Skill s in m_Table.Values)
                {
                    array[i++] = s.ID;
                }

                return array;
            }
        }

        /// <summary>
        /// Count non-zero skills.
        /// </summary>
        public int CountSkills
        {
            get
            {
                if (m_Table == null)
                    return 0;

                return m_Table.Values.Count;
            }
        }

        public int CountTotalSkillLevels
        {
            get
            {
                int sum = 0;
                foreach (Skill s in m_Table.Values)
                    sum += s.Level;
                return sum;
            }
        }

        public SkillTable()
        {
        }

        public SkillTable(IEnumerable<Skill> startingSkills)
        {
            if (startingSkills == null)
                throw new ArgumentNullException("startingSkills");

            foreach (Skill sk in startingSkills)
                AddSkill(sk);
        }

        public Skill GetSkill(int id)
        {
            if (m_Table == null)
                return null;

            Skill sk;
            if (m_Table.TryGetValue(id, out sk))
                return sk;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>0 for a missing skill</returns>
        public int GetSkillLevel(int id)
        {
            Skill sk = GetSkill(id);
            if (sk == null)
                return 0;
            return sk.Level;
        }

        public void AddSkill(Skill sk)
        {
            if (m_Table == null)
                m_Table = new Dictionary<int, Skill>(3);

            if (m_Table.ContainsKey(sk.ID))
                throw new ArgumentException("skill of same ID already in table");
            if (m_Table.ContainsValue(sk))
                throw new ArgumentException("skill already in table");

            m_Table.Add(sk.ID, sk);
        }

        public void AddOrIncreaseSkill(int id)
        {
            if (m_Table == null)
                m_Table = new Dictionary<int, Skill>(3);

            Skill sk = GetSkill(id);
            if (sk == null)
            {
                sk = new Skill(id);
                m_Table.Add(id, sk);
            }

            ++sk.Level;
        }

        public void DecOrRemoveSkill(int id)
        {
            if (m_Table == null) return;

            Skill sk = GetSkill(id);
            if (sk == null) return;
            if (--sk.Level <= 0)
            {
                m_Table.Remove(id);
                if (m_Table.Count == 0)
                    m_Table = null;
            }
        }

        //This levels up the skill but keep track of the buff
        public void AddAndBuffSkill(int id, int buff)
        {
            if (m_Table == null)
                m_Table = new Dictionary<int, Skill>(3);

            Skill sk = GetSkill(id);
            if (sk == null)
            {
                sk = new Skill(id);
                m_Table.Add(id, sk);
            }
                sk.BuffedLevel += buff; //Keep track on how much this skill is being buffed
                sk.Level += buff;
        }

        //Decreses or remove a skill depending on the buff
        public void DecAndDebuffSkill(int id)
        {
            if (m_Table == null) return;

            Skill sk = GetSkill(id);
            if (sk == null) return;
            if (sk.Level - sk.BuffedLevel <= 0)
            {
                m_Table.Remove(id);
                if (m_Table.Count == 0)
                    m_Table = null;
            }
            else
            {
                sk.Level -= sk.BuffedLevel;
                sk.BuffedLevel = 0; //Reset buff
            }
        }
    }
}
