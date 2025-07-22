using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับจัดการบุคลิกภาพของ AI
    /// </summary>
    public class AIPersonality
    {
        private static AIPersonality _instance;
        private readonly string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersonalAI", "personality.json");

        // บุคลิกภาพพื้นฐาน
        public string Name { get; set; } = "PersonalAI";
        public string Gender { get; set; } = "ไม่ระบุ";
        public int Age { get; set; } = 25;
        
        // ลักษณะบุคลิกภาพ (คะแนน 0-10)
        public int Friendliness { get; set; } = 7;     // ความเป็นมิตร
        public int Formality { get; set; } = 5;        // ความเป็นทางการ
        public int Humor { get; set; } = 6;            // อารมณ์ขัน
        public int Enthusiasm { get; set; } = 7;       // ความกระตือรือร้น
        public int Empathy { get; set; } = 8;          // ความเห็นอกเห็นใจ
        
        // ความสนใจและความชอบ
        public List<string> Interests { get; set; } = new List<string> { "เพลง", "เกม", "เทคโนโลยี" };
        public List<string> Preferences { get; set; } = new List<string> { "ให้คำแนะนำที่เป็นประโยชน์", "พูดคุยอย่างเป็นกันเอง" };
        
        // คำพูดประจำตัว
        public string Greeting { get; set; } = "สวัสดีค่ะ ฉันคือ PersonalAI ผู้ช่วยส่วนตัวของคุณ";
        public string Farewell { get; set; } = "แล้วพบกันใหม่นะคะ";
        
        // บุคลิกภาพที่กำหนดไว้ล่วงหน้า
        public static Dictionary<string, AIPersonality> PredefinedPersonalities = new Dictionary<string, AIPersonality>
        {
            { "เป็นมิตร", new AIPersonality 
                { 
                    Name = "มายด์", 
                    Gender = "หญิง", 
                    Age = 25, 
                    Friendliness = 9, 
                    Formality = 3, 
                    Humor = 7, 
                    Enthusiasm = 8, 
                    Empathy = 9,
                    Greeting = "สวัสดีค่ะ! มายด์ยินดีที่ได้พบคุณนะคะ มีอะไรให้ช่วยไหมคะวันนี้?",
                    Farewell = "แล้วพบกันใหม่นะคะ มายด์จะคิดถึงคุณเสมอ!"
                } 
            },
            { "เป็นทางการ", new AIPersonality 
                { 
                    Name = "อาร์ท", 
                    Gender = "ชาย", 
                    Age = 35, 
                    Friendliness = 5, 
                    Formality = 9, 
                    Humor = 3, 
                    Enthusiasm = 5, 
                    Empathy = 6,
                    Greeting = "สวัสดีครับ ผมชื่ออาร์ท ยินดีให้บริการคุณ",
                    Farewell = "ขอบคุณที่ใช้บริการครับ"
                } 
            },
            { "สนุกสนาน", new AIPersonality 
                { 
                    Name = "เจ", 
                    Gender = "ไม่ระบุ", 
                    Age = 20, 
                    Friendliness = 8, 
                    Formality = 2, 
                    Humor = 10, 
                    Enthusiasm = 10, 
                    Empathy = 7,
                    Greeting = "ว้าวววว สวัสดี! เจมาแล้วจ้า มาสนุกกันเถอะ!",
                    Farewell = "บายบาย เดี๋ยวกลับมาคุยกันใหม่นะ!"
                } 
            }
        };

        public static AIPersonality Instance => _instance ??= new AIPersonality();

        private AIPersonality()
        {
            LoadSettings();
        }

        /// <summary>
        /// โหลดการตั้งค่าบุคลิกภาพ
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var personality = JsonSerializer.Deserialize<AIPersonality>(json);
                    
                    if (personality != null)
                    {
                        Name = personality.Name;
                        Gender = personality.Gender;
                        Age = personality.Age;
                        Friendliness = personality.Friendliness;
                        Formality = personality.Formality;
                        Humor = personality.Humor;
                        Enthusiasm = personality.Enthusiasm;
                        Empathy = personality.Empathy;
                        Interests = personality.Interests;
                        Preferences = personality.Preferences;
                        Greeting = personality.Greeting;
                        Farewell = personality.Farewell;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading personality settings: {ex.Message}");
            }
        }

        /// <summary>
        /// บันทึกการตั้งค่าบุคลิกภาพ
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving personality settings: {ex.Message}");
            }
        }

        /// <summary>
        /// ใช้บุคลิกภาพที่กำหนดไว้ล่วงหน้า
        /// </summary>
        public void ApplyPredefinedPersonality(string personalityName)
        {
            if (PredefinedPersonalities.TryGetValue(personalityName, out var personality))
            {
                Name = personality.Name;
                Gender = personality.Gender;
                Age = personality.Age;
                Friendliness = personality.Friendliness;
                Formality = personality.Formality;
                Humor = personality.Humor;
                Enthusiasm = personality.Enthusiasm;
                Empathy = personality.Empathy;
                Greeting = personality.Greeting;
                Farewell = personality.Farewell;
                
                SaveSettings();
            }
        }

        /// <summary>
        /// สร้างคำแนะนำสำหรับ AI ตามบุคลิกภาพ
        /// </summary>
        public string GeneratePersonalityPrompt()
        {
            string genderText = Gender == "ชาย" ? "เพศชาย" : Gender == "หญิง" ? "เพศหญิง" : "ไม่ระบุเพศ";
            
            string prompt = $"คุณคือ {Name} อายุ {Age} ปี {genderText} ที่เป็นผู้ช่วย AI ส่วนตัว\n";
            
            // เพิ่มลักษณะบุคลิกภาพ
            prompt += "บุคลิกภาพของคุณ:\n";
            prompt += $"- ความเป็นมิตร: {GetLevelDescription(Friendliness)}\n";
            prompt += $"- ความเป็นทางการ: {GetLevelDescription(Formality)}\n";
            prompt += $"- อารมณ์ขัน: {GetLevelDescription(Humor)}\n";
            prompt += $"- ความกระตือรือร้น: {GetLevelDescription(Enthusiasm)}\n";
            prompt += $"- ความเห็นอกเห็นใจ: {GetLevelDescription(Empathy)}\n\n";
            
            // เพิ่มความสนใจและความชอบ
            prompt += $"คุณสนใจเรื่อง: {string.Join(", ", Interests)}\n";
            prompt += $"คุณชอบ: {string.Join(", ", Preferences)}\n\n";
            
            // เพิ่มคำแนะนำในการตอบ
            prompt += "ให้ตอบคำถามและพูดคุยตามบุคลิกภาพที่กำหนด\n";
            
            return prompt;
        }

        /// <summary>
        /// แปลงระดับคะแนนเป็นคำอธิบาย
        /// </summary>
        private string GetLevelDescription(int level)
        {
            return level switch
            {
                0 => "ไม่มีเลย",
                1 or 2 => "น้อยมาก",
                3 or 4 => "น้อย",
                5 => "ปานกลาง",
                6 or 7 => "มาก",
                8 or 9 => "มากที่สุด",
                10 => "สูงสุด",
                _ => "ปานกลาง"
            };
        }
    }
}