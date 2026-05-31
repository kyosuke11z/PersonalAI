using System.IO;
using System.Text.Json;

namespace PersonalAI
{
    public class AIServiceOptions
    {
        public string AnthropicApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "claude-sonnet-4-6";
        public int MaxTokens { get; set; } = 1024;
        public float Temperature { get; set; } = 0.7f;

        public void SaveToFile(string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

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
            catch { }

            return new AIServiceOptions();
        }
    }
}
