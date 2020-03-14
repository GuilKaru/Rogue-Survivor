using RogueSurvivor.Data;
using RogueSurvivor.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueSurvivor.Engine
{
    class MessageManager
    {
        readonly List<Message> m_Messages = new List<Message>();
        int m_LinesSpacing;
        int m_FadeoutFactor;
        readonly List<Message> m_History;
        int m_HistorySize;

        public int Count
        {
            get { return m_Messages.Count; }
        }

        public IEnumerable<Message> History
        {
            get { return m_History; }
        }

        public MessageManager(int linesSpacing, int fadeoutFactor, int historySize)
        {
            if (linesSpacing < 0)
                throw new ArgumentOutOfRangeException("linesSpacing < 0");
            if (fadeoutFactor < 0)
                throw new ArgumentOutOfRangeException("fadeoutFactor < 0");

            m_LinesSpacing = linesSpacing;
            m_FadeoutFactor = fadeoutFactor;
            m_HistorySize = historySize;
            m_History = new List<Message>(historySize);
        }

        public void Clear()
        {
            m_Messages.Clear();
        }

        public void ClearHistory()
        {
            m_History.Clear();
        }

        public void Add(Message msg)
        {
            m_Messages.Add(msg);
            m_History.Add(msg);
            if (m_History.Count > m_HistorySize)
            {
                m_History.RemoveAt(0);
            }
        }

        public void RemoveLastMessage()
        {
            if (m_Messages.Count == 0)
                return;
            m_Messages.RemoveAt(m_Messages.Count - 1);
        }

        public void Draw(IRogueUI ui, int freshMessagesTurn, int gx, int gy)
        {
            for (int i = 0; i < m_Messages.Count; i++)
            {
                Message msg = m_Messages[i];

                int alpha = Math.Max(64, 255 - m_FadeoutFactor * (m_Messages.Count - 1 - i));
                bool isLatest = (m_Messages[i].Turn >= freshMessagesTurn);
                Color dimmedColor = Color.FromArgb(alpha, msg.Color);

                if (isLatest)
                    ui.DrawStringBold(dimmedColor, msg.Text, gx, gy);
                else
                    ui.DrawString(dimmedColor, msg.Text, gx, gy);

                gy += m_LinesSpacing;
            }
        }
    }
}
