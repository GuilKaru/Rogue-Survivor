using Microsoft.Xna.Framework.Media;
using RogueSurvivor.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace RogueSurvivor.Engine
{
    class MusicManager : IMusicManager
    {
        private int m_Volume;
        Dictionary<string, Song> m_Musics;
        Song m_CurrentMusic;

        public bool IsMusicEnabled { get; set; }

        public int Volume
        {
            get { return m_Volume; }
            set
            {
                m_Volume = value;
                MediaPlayer.Volume = ((float)value) / 100;
            }
        }

        public string Music { get; private set; }
        public int Priority { get; private set; }

        public bool IsPlaying
        {
            get
            {
                return m_CurrentMusic != null && MediaPlayer.State == MediaState.Playing;
            }
        }

        public bool HasEnded
        {
            get
            {
                return m_CurrentMusic != null && MediaPlayer.State != MediaState.Playing;
            }
        }

        public MusicManager()
        {
            m_Musics = new Dictionary<string, Song>();
            m_Volume = 100;
            Priority = MusicPriority.PRIORITY_NULL;
        }

        string FullName(string fileName)
        {
            return $"file:///{Directory.GetCurrentDirectory()}\\{fileName}.mp3";
        }

        public bool Load(string musicname, string filename)
        {
            filename = FullName(filename);
            try
            {
                Song song = Song.FromUri(musicname, new Uri(filename));
                m_Musics.Add(musicname, song);
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.INIT, string.Format("Failed to load music file {0} exception {1}.", filename, e.ToString()));
            }

            return true;
        }

        /// <summary>
        /// Restart playing a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void Play(string musicname, int priority)
        {
            if (!IsMusicEnabled)
                return;

            Song music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("playing music {0}.", musicname));
                MediaPlayer.Play(music);
                MediaPlayer.IsRepeating = false;
                Music = musicname;
                Priority = priority;
                m_CurrentMusic = music;
            }
        }

        /// <summary>
        /// Restart playing in a loop a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        public void PlayLooping(string musicname, int priority)
        {
            if (!IsMusicEnabled)
                return;

            Song music;
            if (m_Musics.TryGetValue(musicname, out music))
            {
                Logger.WriteLine(Logger.Stage.RUN, string.Format("playing looping music {0}.", musicname));
                MediaPlayer.Play(music);
                MediaPlayer.IsRepeating = true;
                Music = musicname;
                Priority = priority;
                m_CurrentMusic = music;
            }
        }

        public void Stop()
        {
            if (m_CurrentMusic != null)
            {
                MediaPlayer.Stop();
                m_CurrentMusic = null;
            }
            Music = "";
            Priority = MusicPriority.PRIORITY_NULL;
        }
    }
}
