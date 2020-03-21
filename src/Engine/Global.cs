using RogueSurvivor.Gameplay;

namespace RogueSurvivor.Engine
{
    static class Global
    {
        public static GameActors Actors { get; set; }
        public static GameFactions Factions { get; set; }
        public static RogueGame Game { get; set; }
        public static GameItems Items { get; set; }
        public static Rules Rules { get; set; }
        public static Session Session { get; set; }
        public static GameTiles Tiles { get; set; }
    }
}
