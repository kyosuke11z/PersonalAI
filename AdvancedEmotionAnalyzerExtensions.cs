using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalAI
{
    /// <summary>
    /// ส่วนขยายของคลาส AdvancedEmotionAnalyzer
    /// </summary>
    public static class AdvancedEmotionAnalyzerExtensions
    {
        /// <summary>
        /// ให้คำแนะนำตามอารมณ์
        /// </summary>
        public static string GetRecommendationBasedOnEmotion(this AdvancedEmotionAnalyzer analyzer, DetailedEmotionResult emotion)
        {
            // หาอารมณ์ที่มีค่ามากที่สุด
            var scores = emotion.EmotionScores;
            if (scores == null) return "ไม่สามารถให้คำแนะนำได้เนื่องจากข้อมูลไม่เพียงพอ";
            
            double maxScore = Math.Max(
                Math.Max(Math.Max(Math.Max(
                scores.Happy, scores.Sad), scores.Angry), scores.Anxious), scores.Neutral);
            
            if (maxScore == scores.Happy)
            {
                return "คุณกำลังมีความสุข ลองทำกิจกรรมที่ชอบต่อไปเพื่อรักษาความรู้สึกดีๆ นี้";
            }
            else if (maxScore == scores.Sad)
            {
                if (scores.Sad >= 8)
                {
                    return "คุณอาจกำลังรู้สึกเศร้ามาก ลองพูดคุยกับคนที่ไว้ใจหรือผู้เชี่ยวชาญเพื่อระบายความรู้สึก";
                }
                else
                {
                    return "คุณอาจกำลังรู้สึกเศร้า ลองฟังเพลงที่ชอบหรือทำกิจกรรมที่ทำให้รู้สึกดีขึ้น";
                }
            }
            else if (maxScore == scores.Angry)
            {
                if (scores.Angry >= 8)
                {
                    return "คุณอาจกำลังรู้สึกโกรธมาก ลองหายใจลึกๆ และพักสักครู่ก่อนที่จะตัดสินใจทำอะไร";
                }
                else
                {
                    return "คุณอาจกำลังรู้สึกหงุดหงิด ลองออกกำลังกายหรือทำกิจกรรมที่ช่วยระบายความรู้สึก";
                }
            }
            else if (maxScore == scores.Anxious)
            {
                if (scores.Anxious >= 8)
                {
                    return "คุณอาจกำลังรู้สึกกังวลมาก ลองทำสมาธิ หายใจลึกๆ และจดจ่อกับปัจจุบัน";
                }
                else
                {
                    return "คุณอาจกำลังรู้สึกกังวล ลองจดบันทึกสิ่งที่กังวลและวางแผนจัดการทีละอย่าง";
                }
            }
            else
            {
                return "คุณดูเหมือนจะมีอารมณ์ปกติ ลองทำกิจกรรมที่ชอบเพื่อเพิ่มความสุข";
            }
        }
        
        /// <summary>
        /// ตรวจสอบแนวโน้มอารมณ์จากข้อมูลหลายรายการ
        /// </summary>
        public static EmotionTrend AnalyzeEmotionTrend(this AdvancedEmotionAnalyzer analyzer, List<EmotionEntry> entries, int days = 7)
        {
            if (entries == null || entries.Count == 0)
            {
                return new EmotionTrend { IsStable = true };
            }
            
            // กรองข้อมูลตามจำนวนวัน
            var startDate = DateTime.Now.AddDays(-days);
            var filteredEntries = entries.Where(e => e.Timestamp >= startDate).ToList();
            
            if (filteredEntries.Count < 3)
            {
                return new EmotionTrend { IsStable = true };
            }
            
            // จัดเรียงตามเวลา
            var sortedEntries = filteredEntries.OrderBy(e => e.Timestamp).ToList();
            
            // คำนวณแนวโน้มความเครียด
            var stressLevels = sortedEntries.Select(e => e.StressLevel).ToList();
            var stressTrend = CalculateTrend(stressLevels);
            
            // คำนวณแนวโน้มความเป็นบวก
            var positivityScores = sortedEntries.Select(e => e.PositivityScore).ToList();
            var positivityTrend = CalculateTrend(positivityScores);
            
            // ตรวจสอบความผันผวนของอารมณ์
            var stressVariance = CalculateVariance(stressLevels);
            var positivityVariance = CalculateVariance(positivityScores);
            
            // ตรวจสอบความเครียดสูงต่อเนื่อง
            var highStressDays = sortedEntries.Count(e => e.StressLevel >= 8);
            var lowPositivityDays = sortedEntries.Count(e => e.PositivityScore <= 3);
            
            // สร้างผลลัพธ์
            var result = new EmotionTrend
            {
                StressTrend = stressTrend,
                PositivityTrend = positivityTrend,
                IsStable = stressVariance < 4 && positivityVariance < 4,
                HasHighStress = highStressDays >= 3,
                HasLowPositivity = lowPositivityDays >= 3,
                NeedsAttention = (stressTrend > 0.5 && highStressDays >= 2) || (positivityTrend < -0.5 && lowPositivityDays >= 2)
            };
            
            return result;
        }
        
        /// <summary>
        /// คำนวณแนวโน้มของข้อมูล (ค่าบวกคือเพิ่มขึ้น ค่าลบคือลดลง)
        /// </summary>
        private static double CalculateTrend(List<float> values)
        {
            if (values.Count < 2) return 0;
            
            // คำนวณค่าเฉลี่ยของครึ่งแรกและครึ่งหลัง
            int midPoint = values.Count / 2;
            var firstHalf = values.Take(midPoint).Average();
            var secondHalf = values.Skip(midPoint).Average();
            
            // คำนวณแนวโน้ม (ปรับให้อยู่ในช่วง -1 ถึง 1)
            return Math.Min(1, Math.Max(-1, (secondHalf - firstHalf) / 5));
        }
        
        /// <summary>
        /// คำนวณความแปรปรวนของข้อมูล
        /// </summary>
        private static double CalculateVariance(List<float> values)
        {
            if (values.Count < 2) return 0;
            
            double mean = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return sumOfSquares / values.Count;
        }
    }
}