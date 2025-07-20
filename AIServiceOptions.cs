using System.IO;
using System.Text.Json;

namespace PersonalAI
{
    public class AIServiceOptions
    {
        public string ServerUrl { get; set; } = "http://localhost:1234/v1";
        public string Endpoint => $"{ServerUrl}/chat/completions";
        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "gamma";
        public int MaxTokens { get; set; } = 1000;
        public float Temperature { get; set; } = 0.7f;
        
        /// <summary>
        /// บันทึกการตั้งค่าลงไฟล์
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
        /// โหลดการตั้งค่าจากไฟล์
        /// </summary>
        public static AIServiceOptions LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var options = JsonSerializer.Deserialize<AIServiceOptions>(json);
                    return options ?? new AIServiceOptions();
                }
            }
            catch (Exception)
            {
                // ไม่สามารถโหลดได้
            }
            
            return new AIServiceOptions();
        }
    }
}