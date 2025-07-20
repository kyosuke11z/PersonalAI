using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalAI
{
    public class EmotionAnalysisService
    {
        private readonly AIService _aiService;
        private readonly DataService _dataService;
        private readonly RecommendationService _recommendationService;
        private readonly SentimentAnalyzer _sentimentAnalyzer;
        private readonly RecommendationEngine _recommendationEngine;
        private readonly AdvancedEmotionAnalyzer _advancedEmotionAnalyzer;
        private readonly RelaxationActivityRecommender _relaxationRecommender;
        private readonly UserPreferences _userPreferences;

        public EmotionAnalysisService(AIService aiService, DataService dataService, RecommendationService recommendationService)
        {
            _aiService = aiService;
            _dataService = dataService;
            _recommendationService = recommendationService;
            _sentimentAnalyzer = new SentimentAnalyzer();
            _recommendationEngine = new RecommendationEngine(dataService, aiService);
            
            // โหลดการตั้งค่านิสัยส่วนตัว
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalAICompanion");
            string preferencesPath = Path.Combine(appDataPath, "user_preferences.json");
            _userPreferences = UserPreferences.LoadFromFile(preferencesPath);
            
            // สร้างอ็อบเจ็กต์สำหรับวิเคราะห์อารมณ์และแนะนำกิจกรรม
            _advancedEmotionAnalyzer = new AdvancedEmotionAnalyzer(aiService);
            _relaxationRecommender = new RelaxationActivityRecommender(aiService, _userPreferences);
        }

        public async Task<EmotionEntry> AnalyzeEmotionAsync(string text)
        {
            try
            {
                // วิเคราะห์อารมณ์แบบละเอียด
                var detailedEmotion = await _advancedEmotionAnalyzer.AnalyzeEmotionDetailedAsync(text);
                
                // แนะนำกิจกรรมผ่อนคลาย
                var activityRecommendation = await _relaxationRecommender.RecommendActivitiesAsync(detailedEmotion);
                
                // สร้างข้อความตอบกลับ
                string activities = string.Join("\n- ", activityRecommendation.RecommendedActivities);
                string aiResponse = $"{activityRecommendation.Message}\n\nกิจกรรมที่แนะนำ:\n- {activities}";
                
                // สร้างข้อมูลอารมณ์
                var entry = new EmotionEntry
                {
                    Text = text,
                    EmotionLabel = detailedEmotion.PrimaryEmotion ?? "ไม่แน่ชัด",
                    StressLevel = detailedEmotion.StressLevel,
                    PositivityScore = detailedEmotion.Positivity,
                    Timestamp = DateTime.Now,
                    AIResponse = aiResponse
                };
                
                // บันทึกลงฐานข้อมูล
                await _dataService.SaveEmotionEntryAsync(entry);
                
                return entry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"เกิดข้อผิดพลาดในการวิเคราะห์อารมณ์แบบละเอียด: {ex.Message}");
                
                // ใช้วิธีเดิมถ้ามีข้อผิดพลาด
                // วิเคราะห์ความรู้สึกเบื้องต้นด้วย ML.NET
                var sentimentResult = _sentimentAnalyzer.AnalyzeSentiment(text);
                
                // ปรับปรุงคำถามที่ส่งไป AI โดยใช้ผลการวิเคราะห์เบื้องต้น
                string sentimentHint = sentimentResult.IsPositive ? 
                    $"(ข้อความนี้ดูเหมือนจะมีความรู้สึกเชิงบวก ด้วยคะแนน {sentimentResult.Score:F2})" : 
                    $"(ข้อความนี้ดูเหมือนจะมีความรู้สึกเชิงลบ ด้วยคะแนน {1 - sentimentResult.Score:F2})";
                
                var prompt = $"วิเคราะห์ข้อความต่อไปนี้และให้ข้อมูล:\n" +
                             $"1. ป้ายกำกับอารมณ์หนึ่งอย่าง (เช่น มีความสุข, เศร้า, วิตกกังวล) เป็นภาษาไทยเท่านั้น\n" +
                             $"2. คะแนนระดับความเครียดจาก 0-10 (0 คือไม่มีความเครียด, 10 คือความเครียดสูงมาก)\n" +
                             $"3. คะแนนความเป็นบวกจาก 0-10 (0 คือเชิงลบมาก, 10 คือเชิงบวกมาก)\n\n" +
                             $"ข้อความ: \"{text}\"\n\n" +
                             $"{sentimentHint}\n\n" +
                             $"ตอบในรูปแบบ JSON เท่านั้น: {{\"emotion\": \"label\", \"stress\": score, \"positivity\": score}}";

                var responseText = await _aiService.GenerateResponseAsync(prompt);
                
                try
                {
                    // พยายามแยก JSON จากการตอบกลับ
                    var jsonStart = responseText.IndexOf('{');
                    var jsonEnd = responseText.LastIndexOf('}');
                    
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonString = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                        var analysisResult = JsonSerializer.Deserialize<JsonElement>(jsonString);
                        
                        string emotion = "ไม่แน่ชัด";
                        float stress = 5.0f;
                        float positivity = 5.0f;
                        
                        if (analysisResult.TryGetProperty("emotion", out var emotionElement))
                        {
                            emotion = emotionElement.GetString() ?? "ไม่แน่ชัด";
                        }
                        
                        if (analysisResult.TryGetProperty("stress", out var stressElement))
                        {
                            stress = stressElement.GetSingle();
                        }
                        
                        if (analysisResult.TryGetProperty("positivity", out var positivityElement))
                        {
                            positivity = positivityElement.GetSingle();
                        }
                        
                        var suggestion = await GetSuggestionAsync(emotion, stress);
                        
                        var entryResult = new EmotionEntry
                        {
                            Text = text,
                            EmotionLabel = emotion,
                            StressLevel = stress,
                            PositivityScore = positivity,
                            Timestamp = DateTime.Now,
                            AIResponse = suggestion
                        };
                        
                        // บันทึกลงฐานข้อมูล
                        await _dataService.SaveEmotionEntryAsync(entryResult);
                        return entryResult;
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"เกิดข้อผิดพลาดในการแยกวิเคราะห์ JSON: {innerEx.Message}");
                }
                
                // ถ้าไม่สามารถแยก JSON ได้ ใช้ค่าเริ่มต้น
                var defaultEntry = new EmotionEntry
                {
                    Text = text,
                    EmotionLabel = "ไม่แน่ชัด",
                    StressLevel = 5.0f,
                    PositivityScore = 5.0f,
                    Timestamp = DateTime.Now,
                    AIResponse = responseText
                };
                
                // บันทึกลงฐานข้อมูล
                await _dataService.SaveEmotionEntryAsync(defaultEntry);
                return defaultEntry;
            }
        }

        public async Task<string> GetSuggestionAsync(string emotionLabel, float stressLevel)
        {
            var prompt = $"จากอารมณ์ปัจจุบันของผู้ใช้ ({emotionLabel}) และระดับความเครียด ({stressLevel}/10) " +
                         $"กรุณาให้ข้อความให้กำลังใจสั้นๆ และคำแนะนำสำหรับกิจกรรมผ่อนคลายหรือวิธีรับมือ " +
                         $"ตอบเป็นภาษาไทยเท่านั้น ไม่เกิน 100 คำ และทำให้เป็นกันเองและให้กำลังใจ";

            return await _aiService.GenerateResponseAsync(prompt);
        }

        public async Task<List<EmotionEntry>> GetRecentEntriesAsync(int count = 7)
        {
            return await _dataService.GetRecentEmotionEntriesAsync(count);
        }
        
        /// <summary>
        /// เทรนโมเดลวิเคราะห์ความรู้สึกจากข้อมูลที่มีอยู่
        /// </summary>
        public async Task TrainSentimentModelAsync()
        {
            // ดึงข้อมูลอารมณ์ทั้งหมดเพื่อใช้เทรนโมเดล
            var allEntries = await _dataService.GetRecentEmotionEntriesAsync(100);
            await _sentimentAnalyzer.TrainModelAsync(allEntries);
        }
        
        public async Task<(MusicGameItem music, MusicGameItem game)> GetRecommendationsAsync(string emotion)
        {
            try
            {
                // ใช้ RecommendationEngine เพื่อให้คำแนะนำที่เหมาะสมกับผู้ใช้มากขึ้น
                var personalizedMusic = await _recommendationEngine.GetPersonalizedRecommendationsAsync(emotion, ItemType.Music, 1);
                var personalizedGames = await _recommendationEngine.GetPersonalizedRecommendationsAsync(emotion, ItemType.Game, 1);
                
                // ถ้าไม่มีคำแนะนำที่เหมาะสม ให้ใช้ RecommendationService แบบเดิม
                var music = personalizedMusic.FirstOrDefault() ?? 
                    await _recommendationService.GetRecommendationAsync(emotion, ItemType.Music);
                    
                var game = personalizedGames.FirstOrDefault() ?? 
                    await _recommendationService.GetRecommendationAsync(emotion, ItemType.Game);
                    
                return (music, game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"เกิดข้อผิดพลาดในการแนะนำ: {ex.Message}");
                
                // ใช้วิธีเดิมถ้ามีข้อผิดพลาด
                var music = await _recommendationService.GetRecommendationAsync(emotion, ItemType.Music);
                var game = await _recommendationService.GetRecommendationAsync(emotion, ItemType.Game);
                return (music, game);
            }
        }
    }
}