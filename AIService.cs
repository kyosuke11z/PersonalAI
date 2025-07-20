using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalAI
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private AIServiceOptions _options;
        private readonly string _configPath;

        public AIService(AIServiceOptions? options = null)
        {
            _httpClient = new HttpClient();
            
            // กำหนดที่เก็บไฟล์การตั้งค่า
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalAICompanion");
                
            // สร้างโฟลเดอร์ถ้ายังไม่มี
            Directory.CreateDirectory(appDataPath);
            
            _configPath = Path.Combine(appDataPath, "ai_settings.json");
            
            // โหลดการตั้งค่าจากไฟล์หรือใช้ค่าที่ให้มา
            _options = options ?? AIServiceOptions.LoadFromFile(_configPath);
            
            // ตั้งค่า API Key ถ้ามี
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
            }
        }

        /// <summary>
        /// ส่งคำถามไปยัง AI และรับคำตอบกลับ
        /// </summary>
        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                var requestData = new
                {
                    model = _options.ModelName,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = _options.MaxTokens,
                    temperature = _options.Temperature,
                    stream = false
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(_options.Endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseString);

                // พยายามหาข้อความตอบกลับในรูปแบบต่างๆ
                if (responseObject.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    // รูปแบบ OpenAI API
                    if (choices[0].TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var messageContent))
                    {
                        return messageContent.GetString() ?? string.Empty;
                    }
                    
                    // รูปแบบ LM Studio
                    if (choices[0].TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? string.Empty;
                    }
                }

                return "ไม่สามารถแยกวิเคราะห์การตอบกลับจาก AI";
            }
            catch (Exception ex)
            {
                return $"เกิดข้อผิดพลาด: {ex.Message}";
            }
        }
        
        /// <summary>
        /// รับการตั้งค่าปัจจุบัน
        /// </summary>
        public AIServiceOptions GetOptions()
        {
            return _options;
        }
        
        /// <summary>
        /// อัปเดตการตั้งค่า
        /// </summary>
        public void UpdateOptions(AIServiceOptions options)
        {
            _options = options;
            
            // อัปเดต API Key ใน HttpClient
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
            }
            
            // บันทึกการตั้งค่าลงไฟล์
            _options.SaveToFile(_configPath);
        }
    }
}