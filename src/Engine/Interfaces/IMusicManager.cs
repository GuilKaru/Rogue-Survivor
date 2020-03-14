namespace RogueSurvivor.Engine.Interfaces
{
    // alpha10 Added concept of music priority, can play only one music at a time, renamed to MusicManager and
    // some cleanup. Concrete classes updated.

    static class MusicPriority
    {
        /// <summary>
        /// Lowest priority when not playing any music.
        /// </summary>
        public const int PRIORITY_NULL = 0;  // must be 0!

        /// <summary>
        /// Medium priority for background musics.
        /// </summary>
        public const int PRIORITY_BGM = 1;

        /// <summary>
        /// High priority for events musics.
        /// </summary>
        public const int PRIORITY_EVENT = 2;
    }

    interface IMusicManager
    {
        bool IsMusicEnabled { get; set; }
        int Volume { get; set; }
        // alpha10
        int Priority { get; }
        string Music { get; }
        bool IsPlaying { get; }
        bool HasEnded { get; }

        bool Load(string musicname, string filename);

        /// <summary>
        /// Restart playing a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        void Play(string musicname, int priority);

        /// <summary>
        /// Restart playing in a loop a music from the beginning if music is enabled.
        /// </summary>
        /// <param name="musicname"></param>
        void PlayLooping(string musicname, int priority);

        // alpha10
        void Stop();
    }
}
