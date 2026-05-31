using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.SDK.Messaging;

namespace PersonalAI
{
    public class ChatAIService
    {
        private readonly AIService _aiService;
        private readonly UserPreferences _userPreferences;

        // เก็บ conversation history เป็น Message objects จริงๆ ไม่ใช่ string
        private readonly List<Message> _history = new();
        private const int MaxHistoryPairs = 10;

        public ChatAIService(AIService aiService, UserPreferences userPreferences)
        {
            _aiService = aiService;
            _userPreferences = userPreferences;
        }

        /// <summary>
        /// ส่งข้อความและรับการตอบกลับแบบ non-streaming
        /// </summary>
        public async Task<string> SendMessageAsync(string userMessage)
        {
            _history.Add(new Message { Role = RoleType.User, Content = userMessage });
            TrimHistory();

            var response = await _aiService.SendMessagesAsync(_history, BuildSystemPrompt());

            _history.Add(new Message { Role = RoleType.Assistant, Content = response });
            return response;
        }

        /// <summary>
        /// ส่งข้อความและรับการตอบกลับแบบ streaming (IAsyncEnumerable)
        /// </summary>
        public async IAsyncEnumerable<string> SendMessageStreamingAsync(
            string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _history.Add(new Message { Role = RoleType.User, Content = userMessage });
            TrimHistory();

            var fullResponse = new System.Text.StringBuilder();

            await foreach (var chunk in _aiService.StreamMessagesAsync(
                _history, BuildSystemPrompt(), cancellationToken))
            {
                fullResponse.Append(chunk);
                yield return chunk;
            }

            _history.Add(new Message { Role = RoleType.Assistant, Content = fullResponse.ToString() });
        }

        private string BuildSystemPrompt()
        {
            var personality = AIPersonality.Instance;

            var lines = new List<string>
            {
                personality.GeneratePersonalityPrompt(),
                "",
                "ข้อมูลผู้ใช้:",
                $"- ชื่อ: {_userPreferences.UserName}",
                $"- อายุ: {_userPreferences.Age}",
                $"- เพศ: {_userPreferences.Gender}",
            };

            var likes = _userPreferences.ActivityPreferences
                .Where(kv => kv.Value > 3)
                .Select(kv => kv.Key)
                .ToList();
            lines.Add($"- สิ่งที่ชอบ: {(likes.Count > 0 ? string.Join(", ", likes) : "ไม่ระบุ")}");

            var dislikes = _userPreferences.ActivityPreferences
                .Where(kv => kv.Value < -3)
                .Select(kv => kv.Key)
                .ToList();
            lines.Add($"- สิ่งที่ไม่ชอบ: {(dislikes.Count > 0 ? string.Join(", ", dislikes) : "ไม่ระบุ")}");

            lines.Add("");
            lines.Add("ตอบเป็นภาษาไทยเสมอ ใช้น้ำเสียงตามบุคลิกภาพที่กำหนด");

            return string.Join("\n", lines);
        }

        private void TrimHistory()
        {
            // เก็บแค่ MaxHistoryPairs คู่ล่าสุด (user + assistant = 1 คู่)
            int maxMessages = MaxHistoryPairs * 2;
            if (_history.Count > maxMessages)
                _history.RemoveRange(0, _history.Count - maxMessages);
        }

        public void ClearHistory() => _history.Clear();

        public List<ChatMessage> GetChatHistory()
        {
            return _history.Select(m => new ChatMessage
            {
                Role = m.Role == RoleType.User ? "user" : "assistant",
                Content = m.Content?.ToString() ?? "",
                Timestamp = DateTime.Now
            }).ToList();
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
