using System;

namespace PersonalAI
{
    public enum ItemType
    {
        Music,
        Game
    }

    public class MusicGameItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string EmotionTag { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public string Mood { get; set; } = string.Empty;
        public int Rating { get; set; } = 0;
    }
}