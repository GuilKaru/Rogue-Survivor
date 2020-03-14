using System;
using System.Collections.Generic;
using System.IO;

namespace RogueSurvivor.Engine
{
    class TextFile
    {
        List<string> m_RawLines;
        List<string> m_FormatedLines;

        public IEnumerable<string> RawLines
        {
            get { return m_RawLines; }
        }

        public List<string> FormatedLines
        {
            get { return m_FormatedLines; }
        }

        public TextFile()
        {
            m_RawLines = new List<string>();
        }

        public bool Load(string fileName)
        {
            try
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("Loading text file {0}...", fileName));
                StreamReader inStream = File.OpenText(fileName);
                m_RawLines = new List<string>();
                while (!inStream.EndOfStream)
                {
                    string line = inStream.ReadLine();
                    m_RawLines.Add(line);
                }
                inStream.Close();

                Logger.WriteLine(Logger.Stage.RUN, string.Format("done!", fileName));
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("Loading exception: {0}", e.ToString()));
                return false;
            }
        }

        public bool Save(string fileName)
        {
            try
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("Saving text file {0}...", fileName));
                File.WriteAllLines(fileName, m_RawLines.ToArray());
                Logger.WriteLine(Logger.Stage.RUN, string.Format("done!", fileName));
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("Saving exception: {0}", e.ToString()));
                return false;
            }
        }

        public void Append(string line)
        {
            m_RawLines.Add(line);
        }

        public void FormatLines(int charsPerLine)
        {
            if (m_RawLines == null || m_RawLines.Count == 0)
                return;

            m_FormatedLines = new List<string>(m_RawLines.Count);
            for (int iRawLine = 0; iRawLine < m_RawLines.Count; iRawLine++)
            {
                string rawLine = m_RawLines[iRawLine];
                while (rawLine.Length > charsPerLine)
                {
                    string head = rawLine.Substring(0, charsPerLine);
                    string rest = rawLine.Remove(0, charsPerLine);
                    m_FormatedLines.Add(head);
                    rawLine = rest;
                }
                m_FormatedLines.Add(rawLine);
            }
        }
    }
}
