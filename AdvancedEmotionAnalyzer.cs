using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับวิเคราะห์อารมณ์แบบละเอียด
    /// </summary>
    public class AdvancedEmotionAnalyzer
    {
        private readonly AIService _aiService;
        
        // อารมณ์หลักและอารมณ์ย่อย
        private readonly Dictionary<string, List<string>> _emotionCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            { "มีความสุข", new List<string> { "สนุกสนาน", "ตื่นเต้น", "พึงพอใจ", "ภูมิใจ", "อิ่มเอม", "สงบสุข" } },
            { "เศร้า", new List<string> { "เสียใจ", "ผิดหวัง", "ท้อแท้", "สิ้นหวัง", "เหงา", "โดดเดี่ยว" } },
            { "โกรธ", new List<string> { "หงุดหงิด", "รำคาญ", "ขุ่นเคือง", "เกลียด", "แค้น", "อาฆาต" } },
            { "กลัว", new List<string> { "วิตกกังวล", "ตื่นตระหนก", "หวาดระแวง", "ประหม่า", "ตกใจ", "หวั่นไหว" } },
            { "ประหลาดใจ", new List<string> { "ตกตะลึง", "ทึ่ง", "งุนงง", "สับสน", "สงสัย", "ฉงน" } },
            { "รังเกียจ", new List<string> { "ขยะแขยง", "รำคาญ", "ไม่พอใจ", "ดูถูก", "ดูแคลน", "เหยียดหยาม" } },
            { "คาดหวัง", new List<string> { "ตั้งตารอ", "มีความหวัง", "กระตือรือร้น", "ใจจดใจจ่อ", "ตื่นเต้น", "อยากรู้อยากเห็น" } },
            { "ไว้วางใจ", new List<string> { "มั่นใจ", "เชื่อมั่น", "อุ่นใจ", "สบายใจ", "ปลอดภัย", "มั่นคง" } }
        };
        
        // แปลงอารมณ์เป็นอารมณ์พื้นฐาน 5 ประเภท
        private readonly Dictionary<string, string> _emotionToBasicEmotion = new(StringComparer.OrdinalIgnoreCase)
        {
            { "มีความสุข", "มีความสุข" },
            { "สนุกสนาน", "มีความสุข" },
            { "ตื่นเต้น", "มีความสุข" },
            { "พึงพอใจ", "มีความสุข" },
            { "ภูมิใจ", "มีความสุข" },
            { "อิ่มเอม", "มีความสุข" },
            { "สงบสุข", "มีความสุข" },
            
            { "เศร้า", "เศร้า" },
            { "เสียใจ", "เศร้า" },
            { "ผิดหวัง", "เศร้า" },
            { "ท้อแท้", "เศร้า" },
            { "สิ้นหวัง", "เศร้า" },
            { "เหงา", "เศร้า" },
            { "โดดเดี่ยว", "เศร้า" },
            
            { "โกรธ", "โกรธ" },
            { "หงุดหงิด", "โกรธ" },
            { "รำคาญ", "โกรธ" },
            { "ขุ่นเคือง", "โกรธ" },
            { "เกลียด", "โกรธ" },
            { "แค้น", "โกรธ" },
            { "อาฆาต", "โกรธ" },
            
            { "กลัว", "กังวล" },
            { "วิตกกังวล", "กังวล" },
            { "ตื่นตระหนก", "กังวล" },
            { "หวาดระแวง", "กังวล" },
            { "ประหม่า", "กังวล" },
            { "ตกใจ", "กังวล" },
            { "หวั่นไหว", "กังวล" },
            
            { "ประหลาดใจ", "เฉยๆ" },
            { "ตกตะลึง", "เฉยๆ" },
            { "ทึ่ง", "เฉยๆ" },
            { "งุนงง", "เฉยๆ" },
            { "สับสน", "เฉยๆ" },
            { "สงสัย", "เฉยๆ" },
            { "ฉงน", "เฉยๆ" },
            
            { "รังเกียจ", "โกรธ" },
            { "ขยะแขยง", "โกรธ" },
            { "ไม่พอใจ", "โกรธ" },
            { "ดูถูก", "โกรธ" },
            { "ดูแคลน", "โกรธ" },
            { "เหยียดหยาม", "โกรธ" },
            
            { "คาดหวัง", "มีความสุข" },
            { "ตั้งตารอ", "มีความสุข" },
            { "มีความหวัง", "มีความสุข" },
            { "กระตือรือร้น", "มีความสุข" },
            { "ใจจดใจจ่อ", "มีความสุข" },
            { "อยากรู้อยากเห็น", "มีความสุข" },
            
            { "ไว้วางใจ", "มีความสุข" },
            { "มั่นใจ", "มีความสุข" },
            { "เชื่อมั่น", "มีความสุข" },
            { "อุ่นใจ", "มีความสุข" },
            { "สบายใจ", "มีความสุข" },
            { "ปลอดภัย", "มีความสุข" },
            { "มั่นคง", "มีความสุข" }
        };
        
        // กิจกรรมแนะนำตามอารมณ์
        private readonly Dictionary<string, List<string>> _emotionRecommendations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "มีความสุข", new List<string> { 
                "ทำกิจกรรมที่คุณชอบต่อไป", 
                "แบ่งปันความสุขกับคนรอบข้าง", 
                "บันทึกความรู้สึกดีๆ ไว้ในไดอารี่", 
                "ถ่ายรูปช่วงเวลาดีๆ เก็บไว้", 
                "ฟังเพลงที่ชอบและเต้นตามจังหวะ" 
            }},
            { "เศร้า", new List<string> { 
                "พูดคุยกับคนที่ไว้ใจ", 
                "ออกไปเดินสูดอากาศบริสุทธิ์", 
                "ฟังเพลงที่ช่วยให้รู้สึกดีขึ้น", 
                "ดูหนังหรือซีรีส์ตลก", 
                "เขียนความรู้สึกลงในไดอารี่" 
            }},
            { "โกรธ", new List<string> { 
                "หายใจลึกๆ ช้าๆ 10 ครั้ง", 
                "ออกกำลังกายเพื่อระบายพลังงาน", 
                "เขียนความรู้สึกลงในกระดาษแล้วฉีกทิ้ง", 
                "นับถอยหลังจาก 10 ถึง 1", 
                "ไปอยู่ในที่เงียบสงบสักพัก" 
            }},
            { "กังวล", new List<string> { 
                "ฝึกการหายใจแบบลึก", 
                "ทำสมาธิ 5-10 นาที", 
                "เขียนสิ่งที่กังวลลงในกระดาษ", 
                "ทำกิจกรรมที่ต้องใช้สมาธิ เช่น วาดรูป", 
                "ดื่มชาอุ่นๆ และฟังเพลงเบาๆ" 
            }},
            { "เฉยๆ", new List<string> { 
                "ลองทำกิจกรรมใหม่ๆ", 
                "อ่านหนังสือที่น่าสนใจ", 
                "เรียนรู้ทักษะใหม่", 
                "โทรหาเพื่อนที่ไม่ได้คุยนาน", 
                "ปรับเปลี่ยนการตกแต่งห้องหรือโต๊ะทำงาน" 
            }}
        };
        
        public AdvancedEmotionAnalyzer(AIService aiService)
        {
            _aiService = aiService;
        }
        
        /// <summary>
        /// วิเคราะห์อารมณ์แบบละเอียด
        /// </summary>
        public async Task<DetailedEmotionResult> AnalyzeEmotionDetailedAsync(string text)
        {
            var prompt = $"วิเคราะห์อารมณ์และความรู้สึกจากข้อความต่อไปนี้อย่างละเอียด:\n\n" +
                         $"\"{text}\"\n\n" +
                         $"ตอบในรูปแบบ JSON เท่านั้น โดยมีโครงสร้างดังนี้:\n" +
                         $"{{\n" +
                         $"  \"primaryEmotion\": \"อารมณ์หลัก (เช่น มีความสุข, เศร้า, โกรธ, กลัว, ประหลาดใจ, รังเกียจ, คาดหวัง, ไว้วางใจ)\",\n" +
                         $"  \"secondaryEmotion\": \"อารมณ์รอง (เช่น สนุกสนาน, เสียใจ, หงุดหงิด, วิตกกังวล)\",\n" +
                         $"  \"intensity\": ระดับความเข้มข้นของอารมณ์ (1-10),\n" +
                         $"  \"stressLevel\": ระดับความเครียด (1-10),\n" +
                         $"  \"positivity\": ระดับความเป็นบวก (1-10),\n" +
                         $"  \"triggers\": [\"สิ่งกระตุ้นที่ทำให้เกิดอารมณ์นี้\", \"สามารถระบุได้หลายอย่าง\"],\n" +
                         $"  \"needsAttention\": true หรือ false (ต้องการความช่วยเหลือเร่งด่วนหรือไม่),\n" +
                         $"  \"analysis\": \"คำอธิบายสั้นๆ เกี่ยวกับอารมณ์และสาเหตุ\",\n" +
                         $"  \"emotionScores\": {{\n" +
                         $"    \"happy\": คะแนนความสุข (0-10),\n" +
                         $"    \"sad\": คะแนนความเศร้า (0-10),\n" +
                         $"    \"angry\": คะแนนความโกรธ (0-10),\n" +
                         $"    \"anxious\": คะแนนความกังวล (0-10),\n" +
                         $"    \"neutral\": คะแนนความเป็นกลาง (0-10)\n" +
                         $"  }}\n" +
                         $"}}\n\n" +
                         $"ตอบเป็นภาษาไทยเท่านั้น";
            
            var response = await _aiService.GenerateResponseAsync(prompt);
            
            try
            {
                // พยายามแยก JSON จากการตอบกลับ
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var result = JsonSerializer.Deserialize<DetailedEmotionResult>(jsonString);
                    
                    if (result != null)
                    {
                        result.OriginalText = text;
                        
                        // ถ้าไม่มีคะแนนอารมณ์ ให้คำนวณจากอารมณ์หลักและรอง
                        if (result.EmotionScores == null)
                        {
                            result.EmotionScores = CalculateEmotionScores(result);
                        }
                        
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                // ถ้าการแยก JSON ล้มเหลว ใช้ค่าเริ่มต้น
            }
            
            // ค่าเริ่มต้นถ้าไม่สามารถแยก JSON ได้
            return new DetailedEmotionResult
            {
                PrimaryEmotion = "ไม่แน่ชัด",
                SecondaryEmotion = "ไม่แน่ชัด",
                Intensity = 5,
                StressLevel = 5,
                Positivity = 5,
                Triggers = new List<string> { "ไม่สามารถระบุได้" },
                NeedsAttention = false,
                Analysis = "ไม่สามารถวิเคราะห์อารมณ์ได้อย่างชัดเจน",
                OriginalText = text,
                EmotionScores = new EmotionScores
                {
                    Happy = 2,
                    Sad = 2,
                    Angry = 2,
                    Anxious = 2,
                    Neutral = 2
                }
            };
        }
        
        /// <summary>
        /// คำนวณคะแนนอารมณ์พื้นฐาน 5 ประเภทจากอารมณ์หลักและรอง
        /// </summary>
        private EmotionScores CalculateEmotionScores(DetailedEmotionResult emotion)
        {
            var scores = new EmotionScores
            {
                Happy = 2,
                Sad = 2,
                Angry = 2,
                Anxious = 2,
                Neutral = 2
            };
            
            // คำนวณจากอารมณ์หลัก
            if (!string.IsNullOrEmpty(emotion.PrimaryEmotion) && 
                _emotionToBasicEmotion.TryGetValue(emotion.PrimaryEmotion, out var primaryBasic))
            {
                switch (primaryBasic)
                {
                    case "มีความสุข":
                        scores.Happy = Math.Min(10, 5 + emotion.Intensity * 0.5);
                        break;
                    case "เศร้า":
                        scores.Sad = Math.Min(10, 5 + emotion.Intensity * 0.5);
                        break;
                    case "โกรธ":
                        scores.Angry = Math.Min(10, 5 + emotion.Intensity * 0.5);
                        break;
                    case "กังวล":
                        scores.Anxious = Math.Min(10, 5 + emotion.Intensity * 0.5);
                        break;
                    case "เฉยๆ":
                        scores.Neutral = Math.Min(10, 5 + emotion.Intensity * 0.5);
                        break;
                }
            }
            
            // คำนวณจากอารมณ์รอง (มีผลน้อยกว่าอารมณ์หลัก)
            if (!string.IsNullOrEmpty(emotion.SecondaryEmotion) && 
                _emotionToBasicEmotion.TryGetValue(emotion.SecondaryEmotion, out var secondaryBasic))
            {
                switch (secondaryBasic)
                {
                    case "มีความสุข":
                        scores.Happy = Math.Min(10, scores.Happy + emotion.Intensity * 0.3);
                        break;
                    case "เศร้า":
                        scores.Sad = Math.Min(10, scores.Sad + emotion.Intensity * 0.3);
                        break;
                    case "โกรธ":
                        scores.Angry = Math.Min(10, scores.Angry + emotion.Intensity * 0.3);
                        break;
                    case "กังวล":
                        scores.Anxious = Math.Min(10, scores.Anxious + emotion.Intensity * 0.3);
                        break;
                    case "เฉยๆ":
                        scores.Neutral = Math.Min(10, scores.Neutral + emotion.Intensity * 0.3);
                        break;
                }
            }
            
            // ปรับตามระดับความเป็นบวก
            if (emotion.Positivity > 7)
            {
                scores.Happy = Math.Min(10, scores.Happy + 2);
                scores.Sad = Math.Max(0, scores.Sad - 1);
                scores.Angry = Math.Max(0, scores.Angry - 1);
            }
            else if (emotion.Positivity < 3)
            {
                scores.Happy = Math.Max(0, scores.Happy - 1);
                scores.Sad = Math.Min(10, scores.Sad + 1);
                scores.Angry = Math.Min(10, scores.Angry + 1);
            }
            
            // ปรับตามระดับความเครียด
            if (emotion.StressLevel > 7)
            {
                scores.Anxious = Math.Min(10, scores.Anxious + 2);
                scores.Happy = Math.Max(0, scores.Happy - 1);
            }
            else if (emotion.StressLevel < 3)
            {
                scores.Anxious = Math.Max(0, scores.Anxious - 1);
                scores.Neutral = Math.Min(10, scores.Neutral + 1);
            }
            
            return scores;
        }
        
        /// <summary>
        /// ตรวจสอบว่าอารมณ์นี้ต้องการความช่วยเหลือเร่งด่วนหรือไม่
        /// </summary>
        public bool IsEmergencyEmotion(DetailedEmotionResult emotion)
        {
            // ตรวจสอบอารมณ์ที่อาจต้องการความช่วยเหลือเร่งด่วน
            if (emotion.NeedsAttention)
                return true;
                
            // ตรวจสอบระดับความเครียดสูง
            if (emotion.StressLevel >= 9)
                return true;
                
            // ตรวจสอบคำที่อาจบ่งบอกถึงภาวะวิกฤต
            string[] emergencyKeywords = { 
                "ฆ่าตัวตาย", "อยากตาย", "ไม่อยากมีชีวิตอยู่", "ทำร้ายตัวเอง", 
                "ทรมาน", "ทนไม่ไหว", "สิ้นหวัง", "หมดหนทาง", "ไม่เห็นทางออก" 
            };
            
            if (emotion.OriginalText != null && 
                emergencyKeywords.Any(keyword => 
                    emotion.OriginalText.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// แปลงอารมณ์ละเอียดเป็นอารมณ์พื้นฐาน
        /// </summary>
        public EmotionEntry ConvertToBasicEmotion(DetailedEmotionResult detailedEmotion)
        {
            return new EmotionEntry
            {
                Text = detailedEmotion.OriginalText ?? "",
                EmotionLabel = detailedEmotion.PrimaryEmotion ?? "ไม่แน่ชัด",
                StressLevel = detailedEmotion.StressLevel,
                PositivityScore = detailedEmotion.Positivity,
                Timestamp = DateTime.Now,
                AIResponse = detailedEmotion.Analysis ?? ""
            };
        }
        
        /// <summary>
        /// วิเคราะห์แนวโน้มอารมณ์จากประวัติอารมณ์
        /// </summary>
        public EmotionTrend AnalyzeEmotionTrend(List<EmotionEntry> emotionHistory)
        {
            var trend = new EmotionTrend();
            
            // ถ้าไม่มีประวัติอารมณ์หรือมีน้อยเกินไป ให้ถือว่าอารมณ์คงที่
            if (emotionHistory == null || emotionHistory.Count < 3)
            {
                return trend;
            }
            
            // คำนวณแนวโน้มความเครียดและความเป็นบวก
            var recentEntries = emotionHistory.OrderByDescending(e => e.Timestamp).Take(7).ToList();
            
            // คำนวณค่าเฉลี่ยความเครียดและความเป็นบวก
            double avgStress = recentEntries.Average(e => e.StressLevel);
            double avgPositivity = recentEntries.Average(e => e.PositivityScore);
            
            // คำนวณแนวโน้ม (ค่าบวกคือเพิ่มขึ้น ค่าลบคือลดลง)
            if (recentEntries.Count >= 3)
            {
                var oldEntries = recentEntries.Skip(recentEntries.Count / 2).ToList();
                var newEntries = recentEntries.Take(recentEntries.Count / 2).ToList();
                
                double oldAvgStress = oldEntries.Average(e => e.StressLevel);
                double newAvgStress = newEntries.Average(e => e.StressLevel);
                trend.StressTrend = newAvgStress - oldAvgStress;
                
                double oldAvgPositivity = oldEntries.Average(e => e.PositivityScore);
                double newAvgPositivity = newEntries.Average(e => e.PositivityScore);
                trend.PositivityTrend = newAvgPositivity - oldAvgPositivity;
            }
            
            // ตรวจสอบความเครียดสูงต่อเนื่อง
            trend.HasHighStress = avgStress > 7;
            
            // ตรวจสอบความเป็นบวกต่ำต่อเนื่อง
            trend.HasLowPositivity = avgPositivity < 3;
            
            // ตรวจสอบความผันผวนของอารมณ์
            double stressVariance = CalculateVariance(recentEntries.Select(e => (double)e.StressLevel));
            double positivityVariance = CalculateVariance(recentEntries.Select(e => (double)e.PositivityScore));
            
            // ถ้าความแปรปรวนสูง แสดงว่าอารมณ์ไม่คงที่
            trend.IsStable = stressVariance < 4 && positivityVariance < 4;
            
            // ตรวจสอบว่าต้องการความช่วยเหลือหรือไม่
            trend.NeedsAttention = trend.HasHighStress && trend.StressTrend > 0 || 
                                  trend.HasLowPositivity && trend.PositivityTrend < 0 || 
                                  !trend.IsStable;
            
            return trend;
        }
        
        /// <summary>
        /// คำนวณความแปรปรวน (variance) ของข้อมูล
        /// </summary>
        private double CalculateVariance(IEnumerable<double> values)
        {
            var enumerable = values as double[] ?? values.ToArray();
            double avg = enumerable.Average();
            return enumerable.Select(v => Math.Pow(v - avg, 2)).Average();
        }
        
        /// <summary>
        /// ให้คำแนะนำตามอารมณ์
        /// </summary>
        public string GetRecommendationBasedOnEmotion(DetailedEmotionResult emotion)
        {
            // หาอารมณ์พื้นฐาน
            string basicEmotion = "เฉยๆ";
            
            if (!string.IsNullOrEmpty(emotion.PrimaryEmotion) && 
                _emotionToBasicEmotion.TryGetValue(emotion.PrimaryEmotion, out var primary))
            {
                basicEmotion = primary;
            }
            
            // หาคำแนะนำตามอารมณ์พื้นฐาน
            if (_emotionRecommendations.TryGetValue(basicEmotion, out var recommendations))
            {
                // สุ่มคำแนะนำ 2-3 ข้อ
                var random = new Random();
                var selectedCount = Math.Min(random.Next(2, 4), recommendations.Count);
                var selectedRecommendations = recommendations
                    .OrderBy(_ => random.Next())
                    .Take(selectedCount)
                    .ToList();
                
                return string.Join("\n", selectedRecommendations.Select(r => "- " + r));
            }
            
            // ถ้าไม่มีคำแนะนำสำหรับอารมณ์นี้ ให้ใช้คำแนะนำทั่วไป
            return "- พักผ่อนให้เพียงพอ\n- ทำกิจกรรมที่คุณชอบ\n- พูดคุยกับคนที่คุณไว้ใจ";
        }
    }
}