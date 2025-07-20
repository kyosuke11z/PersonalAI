using System.Collections.Generic;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับเก็บผลการวิเคราะห์อารมณ์แบบละเอียด
    /// </summary>
    public class DetailedEmotionResult
    {
        public string? PrimaryEmotion { get; set; }
        public string? SecondaryEmotion { get; set; }
        public int Intensity { get; set; }
        public int StressLevel { get; set; }
        public int Positivity { get; set; }
        public List<string>? Triggers { get; set; }
        public bool NeedsAttention { get; set; }
        public string? Analysis { get; set; }
        public string? OriginalText { get; set; }
        public EmotionScores? EmotionScores { get; set; }
    }
}