using System;

namespace PersonalAI
{
    public class EmotionEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Text { get; set; } = string.Empty;
        public string EmotionLabel { get; set; } = string.Empty;
        public float StressLevel { get; set; }
        public float PositivityScore { get; set; }
        public string AIResponse { get; set; } = string.Empty;
    }
}