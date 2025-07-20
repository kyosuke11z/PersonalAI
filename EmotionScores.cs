namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับเก็บคะแนนอารมณ์พื้นฐาน 5 ประเภท
    /// </summary>
    public class EmotionScores
    {
        public double Happy { get; set; } = 0;
        public double Sad { get; set; } = 0;
        public double Angry { get; set; } = 0;
        public double Anxious { get; set; } = 0;
        public double Neutral { get; set; } = 0;
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บผลการวิเคราะห์แนวโน้มอารมณ์
    /// </summary>
    public class EmotionTrend
    {
        public double StressTrend { get; set; } = 0;
        public double PositivityTrend { get; set; } = 0;
        public bool IsStable { get; set; } = true;
        public bool HasHighStress { get; set; } = false;
        public bool HasLowPositivity { get; set; } = false;
        public bool NeedsAttention { get; set; } = false;
    }
}