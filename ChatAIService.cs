using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalAI
{
    /// <summary>
    /// บริการแชทกับ AI
    /// </summary>
    public class ChatAIService
    {
        private readonly AIService _aiService;
        private readonly UserPreferences _userPreferences;
        private readonly List<ChatMessage> _chatHistory = new();
        
        // จำนวนข้อความสูงสุดที่จะเก็บในประวัติ
        private const int MaxHistoryLength = 10;
        
        public ChatAIService(AIService aiService, UserPreferences userPreferences)
        {
            _aiService = aiService;
            _userPreferences = userPreferences;
        }
        
        /// <summary>
        /// ส่งข้อความและรับการตอบกลับจาก AI
        /// </summary>
        public async Task<string> SendMessageAsync(string message)
        {
            // เพิ่มข้อความของผู้ใช้ลงในประวัติ
            _chatHistory.Add(new ChatMessage
            {
                Role = "user",
                Content = message,
                Timestamp = DateTime.Now
            });
            
            // สร้าง prompt โดยรวมประวัติการแชทและข้อมูลผู้ใช้
            var prompt = BuildPrompt(message);
            
            // ส่งไปยัง AI และรับการตอบกลับ
            var response = await _aiService.GenerateResponseAsync(prompt);
            
            // เพิ่มการตอบกลับของ AI ลงในประวัติ
            _chatHistory.Add(new ChatMessage
            {
                Role = "assistant",
                Content = response,
                Timestamp = DateTime.Now
            });
            
            // จำกัดขนาดของประวัติ
            TrimHistory();
            
            return response;
        }
        
        /// <summary>
        /// สร้าง prompt สำหรับ AI โดยรวมประวัติการแชทและข้อมูลผู้ใช้
        /// </summary>
        private string BuildPrompt(string currentMessage)
        {
            var personality = AIPersonality.Instance;
            
            // เพิ่มคำแนะนำบุคลิกภาพ
            var prompt = personality.GeneratePersonalityPrompt();
            prompt += "\n\n";
            
            // เพิ่มข้อมูลผู้ใช้
            prompt += $"ข้อมูลผู้ใช้:\n";
            prompt += $"- ชื่อ: {_userPreferences.UserName}\n";
            prompt += $"- อายุ: {_userPreferences.Age}\n";
            prompt += $"- เพศ: {_userPreferences.Gender}\n";
            
            // เพิ่มความชอบ/ไม่ชอบ
            prompt += "- สิ่งที่ชอบ: ";
            var likes = new List<string>();
            foreach (var activity in _userPreferences.ActivityPreferences)
            {
                if (activity.Value > 3)
                {
                    likes.Add(activity.Key);
                }
            }
            prompt += string.Join(", ", likes.Count > 0 ? likes : new List<string> { "ไม่ระบุ" });
            prompt += "\n";
            
            prompt += "- สิ่งที่ไม่ชอบ: ";
            var dislikes = new List<string>();
            foreach (var activity in _userPreferences.ActivityPreferences)
            {
                if (activity.Value < -3)
                {
                    dislikes.Add(activity.Key);
                }
            }
            prompt += string.Join(", ", dislikes.Count > 0 ? dislikes : new List<string> { "ไม่ระบุ" });
            prompt += "\n\n";
            
            // เพิ่มประวัติการแชท
            prompt += "ประวัติการสนทนา:\n";
            foreach (var message in _chatHistory.GetRange(0, Math.Min(_chatHistory.Count, MaxHistoryLength - 1)))
            {
                prompt += $"{message.Role}: {message.Content}\n\n";
            }
            
            // เพิ่มข้อความปัจจุบัน
            prompt += $"user: {currentMessage}\n\n";
            prompt += "assistant: ";
            
            return prompt;
        }
        
        /// <summary>
        /// จำกัดขนาดของประวัติการแชท
        /// </summary>
        private void TrimHistory()
        {
            if (_chatHistory.Count > MaxHistoryLength * 2)
            {
                _chatHistory.RemoveRange(0, _chatHistory.Count - MaxHistoryLength);
            }
        }
        
        /// <summary>
        /// ล้างประวัติการแชท
        /// </summary>
        public void ClearHistory()
        {
            _chatHistory.Clear();
        }
        
        /// <summary>
        /// รับประวัติการแชททั้งหมด
        /// </summary>
        public List<ChatMessage> GetChatHistory()
        {
            return new List<ChatMessage>(_chatHistory);
        }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บข้อความในการแชท
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}