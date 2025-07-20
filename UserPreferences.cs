using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลความชอบและนิสัยส่วนตัวของผู้ใช้
    /// </summary>
    public class UserPreferences
    {
        // ข้อมูลพื้นฐาน
        public string UserName { get; set; } = "คุณ";
        public int Age { get; set; } = 25;
        public string Gender { get; set; } = "ไม่ระบุ";
        
        // ความชอบ/ไม่ชอบ (คะแนน -5 ถึง 5, ลบ = ไม่ชอบ, บวก = ชอบ)
        public Dictionary<string, int> ActivityPreferences { get; set; } = new Dictionary<string, int>
        {
            { "กีฬา", 0 },
            { "อ่านหนังสือ", 0 },
            { "ดูหนัง", 0 },
            { "ฟังเพลง", 0 },
            { "เล่นเกม", 0 },
            { "ทำอาหาร", 0 },
            { "ท่องเที่ยว", 0 },
            { "ช้อปปิ้ง", 0 },
            { "นอนพักผ่อน", 0 },
            { "ทำสมาธิ", 0 }
        };
        
        // ความชอบ/ไม่ชอบด้านสภาพแวดล้อม
        public Dictionary<string, int> EnvironmentPreferences { get; set; } = new Dictionary<string, int>
        {
            { "อากาศร้อน", 0 },
            { "อากาศเย็น", 0 },
            { "ที่มีคนเยอะ", 0 },
            { "ที่เงียบสงบ", 0 },
            { "กลางแจ้ง", 0 },
            { "ในร่ม", 0 }
        };
        
        // ข้อมูลสุขภาพ
        public Dictionary<string, bool> HealthConditions { get; set; } = new Dictionary<string, bool>
        {
            { "โรคหัวใจ", false },
            { "โรคภูมิแพ้", false },
            { "โรคหอบหืด", false },
            { "โรคเบาหวาน", false },
            { "โรคความดัน", false },
            { "ปวดหลังเรื้อรัง", false }
        };
        
        // ข้อมูลเพิ่มเติม
        public List<string> Allergies { get; set; } = new List<string>();
        public List<string> CustomPreferences { get; set; } = new List<string>();
        public List<string> CustomDislikes { get; set; } = new List<string>();
        
        /// <summary>
        /// ตรวจสอบว่าผู้ใช้ชอบกิจกรรมนี้หรือไม่
        /// </summary>
        public bool LikesActivity(string activity)
        {
            if (ActivityPreferences.TryGetValue(activity, out int score))
            {
                return score > 0;
            }
            
            // ตรวจสอบใน CustomPreferences
            return CustomPreferences.Any(p => p.Contains(activity, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// ตรวจสอบว่าผู้ใช้ไม่ชอบกิจกรรมนี้หรือไม่
        /// </summary>
        public bool DislikesActivity(string activity)
        {
            if (ActivityPreferences.TryGetValue(activity, out int score))
            {
                return score < 0;
            }
            
            // ตรวจสอบใน CustomDislikes
            return CustomDislikes.Any(p => p.Contains(activity, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// ตรวจสอบว่าผู้ใช้ชอบสภาพแวดล้อมนี้หรือไม่
        /// </summary>
        public bool LikesEnvironment(string environment)
        {
            if (EnvironmentPreferences.TryGetValue(environment, out int score))
            {
                return score > 0;
            }
            
            return CustomPreferences.Any(p => p.Contains(environment, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// บันทึกข้อมูลลงไฟล์
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception)
            {
                // ไม่สามารถบันทึกได้
            }
        }
        
        /// <summary>
        /// โหลดข้อมูลจากไฟล์
        /// </summary>
        public static UserPreferences LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var preferences = JsonSerializer.Deserialize<UserPreferences>(json);
                    return preferences ?? new UserPreferences();
                }
            }
            catch (Exception)
            {
                // ไม่สามารถโหลดได้
            }
            
            return new UserPreferences();
        }
    }
}