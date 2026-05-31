using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace PersonalAI
{
    public class AIService
    {
        private readonly AIServiceOptions _options;
        private readonly string _configPath;
        private AnthropicClient? _anthropicClient;

        public AIService(AIServiceOptions? options = null)
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalAICompanion");
            Directory.CreateDirectory(appDataPath);
            _configPath = Path.Combine(appDataPath, "ai_settings.json");
            _options = options ?? AIServiceOptions.LoadFromFile(_configPath);
            _anthropicClient = BuildClient();
        }

        private AnthropicClient? BuildClient()
        {
            if (!string.IsNullOrWhiteSpace(_options.AnthropicApiKey))
                return new AnthropicClient(_options.AnthropicApiKey);
            return null;
        }

        /// <summary>
        /// ส่ง prompt เดี่ยว (ไม่มี history) และรับคำตอบแบบ non-streaming
        /// ใช้สำหรับ emotion analysis, suggestions, recommendations
        /// </summary>
        public async Task<string> GenerateResponseAsync(string prompt, string? systemPrompt = null)
        {
            var messages = new List<Message>
            {
                new Message { Role = RoleType.User, Content = prompt }
            };
            return await SendMessagesAsync(messages, systemPrompt);
        }

        /// <summary>
        /// ส่ง messages array จริงๆ (สำหรับ chat ที่มี history) แบบ non-streaming
        /// </summary>
        public async Task<string> SendMessagesAsync(
            List<Message> messages,
            string? systemPrompt = null)
        {
            if (_anthropicClient == null)
                return "ยังไม่ได้ตั้งค่า Anthropic API Key กรุณาไปที่ Settings และใส่ API Key";

            try
            {
                var parameters = BuildParameters(messages, systemPrompt, stream: false);
                var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
                return ExtractText(response);
            }
            catch (Exception ex)
            {
                return $"เกิดข้อผิดพลาด: {ex.Message}";
            }
        }

        /// <summary>
        /// ส่ง messages array และรับ response แบบ streaming (IAsyncEnumerable)
        /// แต่ละ chunk คือ string ย่อยๆ ที่ไหลออกมาทีละชิ้น
        /// </summary>
        public async IAsyncEnumerable<string> StreamMessagesAsync(
            List<Message> messages,
            string? systemPrompt = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_anthropicClient == null)
            {
                yield return "ยังไม่ได้ตั้งค่า Anthropic API Key";
                yield break;
            }

            var parameters = BuildParameters(messages, systemPrompt, stream: true);

            await foreach (var chunk in _anthropicClient.Messages
                .StreamClaudeMessageAsync(parameters, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                var text = chunk?.Delta?.Text;
                if (!string.IsNullOrEmpty(text))
                    yield return text;
            }
        }

        public AIServiceOptions GetOptions() => _options;

        public void UpdateOptions(AIServiceOptions options)
        {
            _options.AnthropicApiKey = options.AnthropicApiKey;
            _options.ModelName = options.ModelName;
            _options.MaxTokens = options.MaxTokens;
            _options.Temperature = options.Temperature;
            _anthropicClient = BuildClient();
            _options.SaveToFile(_configPath);
        }

        private MessageParameters BuildParameters(
            List<Message> messages,
            string? systemPrompt,
            bool stream)
        {
            var p = new MessageParameters
            {
                Messages = messages,
                Model = _options.ModelName,
                MaxTokens = _options.MaxTokens,
                Temperature = (decimal)_options.Temperature,
                Stream = stream
            };

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                p.System = new List<SystemMessage>
                {
                    new SystemMessage(systemPrompt)
                };
            }

            return p;
        }

        private static string ExtractText(MessageResponse response)
        {
            if (response?.Content == null || response.Content.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var block in response.Content)
            {
                if (block is TextContent tc)
                    sb.Append(tc.Text);
                else
                    sb.Append(block.ToString());
            }
            return sb.ToString();
        }
    }
}
