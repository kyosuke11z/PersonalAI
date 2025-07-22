using System;
using System.Collections.Generic;

namespace PersonalAI
{
    public class ExportData
    {
        public List<EmotionEntry> EmotionEntries { get; set; } = new List<EmotionEntry>();
        public List<MusicGameItem> MusicGameItems { get; set; } = new List<MusicGameItem>();
        public DateTime ExportDate { get; set; }
    }
}