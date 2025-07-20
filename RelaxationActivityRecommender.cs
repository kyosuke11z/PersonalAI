using System.Text.Json;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับแนะนำกิจกรรมผ่อนคลายตามอารมณ์และความชอบส่วนตัว
    /// </summary>
    public class RelaxationActivityRecommender
    {
        private readonly AIService _aiService;
        private readonly UserPreferences _userPreferences;
        
        // กิจกรรมผ่อนคลายตามประเภทอารมณ์
        private readonly Dictionary<string, List<string>> _defaultActivities = new()
        {
            { "มีความสุข", new List<string> { 
                "ฟังเพลงที่ชอบ", "ไปเดินเล่นในสวน", "ทำกิจกรรมสร้างสรรค์", "พบปะเพื่อนฝูง", "ดูหนังตลก" 
            }},
            { "เศร้า", new List<string> { 
                "เขียนบันทึกความรู้สึก", "ฟังเพลงที่ชอบ", "โทรหาเพื่อนสนิท", "ดูหนังให้กำลังใจ", "ทำสมาธิ" 
            }},
            { "โกรธ", new List<string> { 
                "ออกกำลังกาย", "เขียนระบายความรู้สึก", "ฟังเพลงที่ชอบ", "ทำสมาธิ", "หายใจลึกๆ" 
            }},
            { "กลัว", new List<string> { 
                "ทำสมาธิ", "หายใจลึกๆ", "พูดคุยกับคนที่ไว้ใจ", "เขียนบันทึกความกังวล", "ดูหนังตลก" 
            }},
            { "วิตกกังวล", new List<string> { 
                "ทำสมาธิ", "โยคะ", "หายใจลึกๆ", "เขียนบันทึกความกังวล", "ดื่มชาอุ่นๆ" 
            }},
            { "เครียด", new List<string> { 
                "ออกกำลังกาย", "ทำสมาธิ", "โยคะ", "นวดผ่อนคลาย", "อาบน้ำอุ่น" 
            }},
            { "เหนื่อยล้า", new List<string> { 
                "นอนพักผ่อน", "ดื่มน้ำมากๆ", "ทานอาหารที่มีประโยชน์", "งีบสั้นๆ", "ลดการใช้หน้าจอ" 
            }}
        };
        
        // กิจกรรมที่ต้องแนะนำในกรณีฉุกเฉิน
        private readonly List<string> _emergencyActivities = new()
        {
            "โทรหาสายด่วนสุขภาพจิต 1323",
            "ติดต่อจิตแพทย์หรือนักจิตวิทยา",
            "พูดคุยกับคนที่ไว้ใจ",
            "หลีกเลี่ยงการอยู่คนเดียว",
            "หายใจลึกๆ ช้าๆ"
        };
        
        public RelaxationActivityRecommender(AIService aiService, UserPreferences userPreferences)
        {
            _aiService = aiService;
            _userPreferences = userPreferences;
        }
        
        /// <summary>
        /// แนะนำกิจกรรมผ่อนคลายตามอารมณ์และความชอบส่วนตัว
        /// </summary>
        public async Task<ActivityRecommendation> RecommendActivitiesAsync(DetailedEmotionResult emotion)
        {
            // ตรวจสอบกรณีฉุกเฉิน
            if (IsEmergencySituation(emotion))
            {
                return new ActivityRecommendation
                {
                    EmotionState = emotion,
                    RecommendedActivities = _emergencyActivities,
                    IsEmergency = true,
                    Message = "คุณอาจกำลังเผชิญกับภาวะวิกฤต โปรดพิจารณาขอความช่วยเหลือจากผู้เชี่ยวชาญ"
                };
            }
            
            // สร้างคำแนะนำโดยใช้ AI และข้อมูลความชอบส่วนตัว
            var activities = await GeneratePersonalizedActivitiesAsync(emotion);
            
            return new ActivityRecommendation
            {
                EmotionState = emotion,
                RecommendedActivities = activities,
                IsEmergency = false,
                Message = GetSupportiveMessage(emotion)
            };
        }
        
        /// <summary>
        /// ตรวจสอบว่าเป็นสถานการณ์ฉุกเฉินหรือไม่
        /// </summary>
        private bool IsEmergencySituation(DetailedEmotionResult emotion)
        {
            // ตรวจสอบอารมณ์ที่อาจต้องการความช่วยเหลือเร่งด่วน
            if (emotion.NeedsAttention)
                return true;
                
            // ตรวจสอบระดับความเครียดสูงมาก
            if (emotion.StressLevel >= 9 && emotion.Positivity <= 2)
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
        /// สร้างข้อความให้กำลังใจตามอารมณ์
        /// </summary>
        private string GetSupportiveMessage(DetailedEmotionResult emotion)
        {
            if (emotion.PrimaryEmotion == "มีความสุข")
            {
                return "ดีใจที่คุณรู้สึกดี! ลองทำกิจกรรมเหล่านี้เพื่อรักษาความรู้สึกดีๆ ไว้";
            }
            else if (emotion.PrimaryEmotion == "เศร้า")
            {
                return "ทุกคนมีช่วงเวลาที่รู้สึกเศร้า ลองทำกิจกรรมเหล่านี้เพื่อช่วยให้รู้สึกดีขึ้น";
            }
            else if (emotion.PrimaryEmotion == "โกรธ")
            {
                return "ความโกรธเป็นอารมณ์ปกติ แต่การจัดการกับมันอย่างเหมาะสมเป็นสิ่งสำคัญ ลองทำกิจกรรมเหล่านี้";
            }
            else if (emotion.PrimaryEmotion == "กลัว" || emotion.PrimaryEmotion == "วิตกกังวล")
            {
                return "ความกังวลเป็นเรื่องปกติ ลองทำกิจกรรมเหล่านี้เพื่อช่วยให้รู้สึกสงบและปลอดภัยมากขึ้น";
            }
            else if (emotion.PrimaryEmotion == "เครียด")
            {
                return "ความเครียดสามารถส่งผลต่อทั้งร่างกายและจิตใจ ลองทำกิจกรรมเหล่านี้เพื่อผ่อนคลาย";
            }
            else if (emotion.PrimaryEmotion == "เหนื่อยล้า")
            {
                return "การพักผ่อนอย่างเพียงพอเป็นสิ่งสำคัญ ลองทำกิจกรรมเหล่านี้เพื่อฟื้นฟูพลังงาน";
            }
            
            return "ลองทำกิจกรรมเหล่านี้เพื่อช่วยให้รู้สึกดีขึ้น";
        }
        
        /// <summary>
        /// สร้างคำแนะนำกิจกรรมที่เหมาะกับความชอบส่วนตัว
        /// </summary>
        private async Task<List<string>> GeneratePersonalizedActivitiesAsync(DetailedEmotionResult emotion)
        {
            // สร้างรายการกิจกรรมเริ่มต้นตามอารมณ์
            List<string> baseActivities = new();
            if (_defaultActivities.TryGetValue(emotion.PrimaryEmotion ?? "เครียด", out var activities))
            {
                baseActivities.AddRange(activities);
            }
            else
            {
                // ถ้าไม่มีกิจกรรมสำหรับอารมณ์นี้ ใช้กิจกรรมสำหรับความเครียดแทน
                baseActivities.AddRange(_defaultActivities["เครียด"]);
            }
            
            // กรองกิจกรรมตามความชอบส่วนตัว
            var filteredActivities = baseActivities
                .Where(activity => !_userPreferences.DislikesActivity(activity))
                .ToList();
            
            // ถ้ามีกิจกรรมน้อยเกินไป ขอคำแนะนำจาก AI
            if (filteredActivities.Count < 3)
            {
                var aiActivities = await GetAIRecommendationsAsync(emotion);
                filteredActivities.AddRange(aiActivities);
            }
            
            // ตัดรายการให้เหลือไม่เกิน 5 กิจกรรม
            return filteredActivities.Distinct().Take(5).ToList();
        }
        
        /// <summary>
        /// ขอคำแนะนำกิจกรรมจาก AI
        /// </summary>
        private async Task<List<string>> GetAIRecommendationsAsync(DetailedEmotionResult emotion)
        {
            // สร้างข้อมูลความชอบ/ไม่ชอบสำหรับส่งให้ AI
            var likes = _userPreferences.ActivityPreferences
                .Where(p => p.Value > 0)
                .Select(p => p.Key)
                .ToList();
                
            var dislikes = _userPreferences.ActivityPreferences
                .Where(p => p.Value < 0)
                .Select(p => p.Key)
                .ToList();
                
            // เพิ่มข้อมูลจาก CustomPreferences และ CustomDislikes
            likes.AddRange(_userPreferences.CustomPreferences);
            dislikes.AddRange(_userPreferences.CustomDislikes);
            
            // สร้าง prompt สำหรับ AI
            var prompt = $"แนะนำ 5 กิจกรรมผ่อนคลายที่เหมาะสมสำหรับคนที่กำลังรู้สึก{emotion.PrimaryEmotion} " +
                         $"และมีอารมณ์รอง{emotion.SecondaryEmotion} โดยมีระดับความเครียด {emotion.StressLevel}/10\n\n" +
                         $"สิ่งที่ชอบ: {string.Join(", ", likes)}\n" +
                         $"สิ่งที่ไม่ชอบ: {string.Join(", ", dislikes)}\n\n" +
                         $"ตอบในรูปแบบ JSON เท่านั้น: {{\"activities\": [\"กิจกรรม1\", \"กิจกรรม2\", \"กิจกรรม3\", \"กิจกรรม4\", \"กิจกรรม5\"]}}\n\n" +
                         $"ตอบเป็นภาษาไทยเท่านั้น และแนะนำกิจกรรมที่เฉพาะเจาะจง ไม่กว้างเกินไป";
            
            var response = await _aiService.GenerateResponseAsync(prompt);
            
            try
            {
                // พยายามแยก JSON จากการตอบกลับ
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var result = JsonSerializer.Deserialize<ActivityResponse>(jsonString);
                    
                    if (result?.Activities != null && result.Activities.Count > 0)
                    {
                        return result.Activities;
                    }
                }
            }
            catch (Exception)
            {
                // ถ้าการแยก JSON ล้มเหลว ใช้ค่าเริ่มต้น
            }
            
            // ค่าเริ่มต้นถ้าไม่สามารถแยก JSON ได้
            return new List<string>
            {
                "ฟังเพลงที่ชอบ",
                "ดื่มน้ำอุ่นๆ",
                "หายใจลึกๆ ช้าๆ",
                "นอนพักผ่อนให้เพียงพอ",
                "พูดคุยกับคนที่ไว้ใจ"
            };
        }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บคำแนะนำกิจกรรม
    /// </summary>
    public class ActivityRecommendation
    {
        public DetailedEmotionResult? EmotionState { get; set; }
        public List<string> RecommendedActivities { get; set; } = new();
        public bool IsEmergency { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// คลาสสำหรับรับข้อมูล JSON จาก AI
    /// </summary>
    public class ActivityResponse
    {
        public List<string> Activities { get; set; } = new();
    }
}