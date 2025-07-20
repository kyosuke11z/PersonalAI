using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalAI
{
    /// <summary>
    /// เอ็นจิ้นสำหรับแนะนำเพลงและเกมโดยใช้ข้อมูลอารมณ์และประวัติการใช้งาน
    /// </summary>
    public class RecommendationEngine
    {
        private readonly DataService _dataService;
        private readonly AIService _aiService;
        private readonly Dictionary<string, int> _userPreferences = new();
        
        // น้ำหนักสำหรับการคำนวณคะแนนแนะนำ
        private const float EMOTION_WEIGHT = 0.5f;
        private const float HISTORY_WEIGHT = 0.3f;
        private const float GENRE_WEIGHT = 0.2f;
        
        public RecommendationEngine(DataService dataService, AIService aiService)
        {
            _dataService = dataService;
            _aiService = aiService;
        }
        
        /// <summary>
        /// วิเคราะห์ประวัติการใช้งานเพื่อสร้างโปรไฟล์ความชอบของผู้ใช้
        /// </summary>
        public async Task AnalyzeUserPreferencesAsync()
        {
            // ดึงประวัติอารมณ์ทั้งหมด
            var emotions = await _dataService.GetRecentEmotionEntriesAsync(50);
            
            // ดึงประวัติการแนะนำทั้งหมด
            var musicItems = await _dataService.GetMusicGameItemsByTypeAsync(ItemType.Music, 50);
            var gameItems = await _dataService.GetMusicGameItemsByTypeAsync(ItemType.Game, 50);
            
            // รีเซ็ตความชอบ
            _userPreferences.Clear();
            
            // วิเคราะห์แนวเพลงที่ชอบ
            foreach (var item in musicItems)
            {
                if (!string.IsNullOrEmpty(item.Genre))
                {
                    if (_userPreferences.ContainsKey(item.Genre))
                    {
                        _userPreferences[item.Genre]++;
                    }
                    else
                    {
                        _userPreferences[item.Genre] = 1;
                    }
                }
            }
            
            // วิเคราะห์แนวเกมที่ชอบ
            foreach (var item in gameItems)
            {
                if (!string.IsNullOrEmpty(item.Genre))
                {
                    if (_userPreferences.ContainsKey(item.Genre))
                    {
                        _userPreferences[item.Genre]++;
                    }
                    else
                    {
                        _userPreferences[item.Genre] = 1;
                    }
                }
            }
        }
        
        /// <summary>
        /// แนะนำเพลงหรือเกมตามอารมณ์ปัจจุบันและประวัติความชอบ
        /// </summary>
        public async Task<List<MusicGameItem>> GetPersonalizedRecommendationsAsync(string currentEmotion, ItemType type, int count = 3)
        {
            // วิเคราะห์ความชอบของผู้ใช้
            await AnalyzeUserPreferencesAsync();
            
            // ดึงรายการทั้งหมดตามประเภท
            var allItems = await _dataService.GetMusicGameItemsByTypeAsync(type, 100);
            
            // คำนวณคะแนนสำหรับแต่ละรายการ
            var scoredItems = new List<(MusicGameItem item, float score)>();
            
            foreach (var item in allItems)
            {
                float emotionScore = CalculateEmotionScore(item.EmotionTag, currentEmotion);
                float genreScore = CalculateGenreScore(item.Genre);
                
                // คำนวณคะแนนรวม
                float totalScore = (emotionScore * EMOTION_WEIGHT) + (genreScore * GENRE_WEIGHT);
                scoredItems.Add((item, totalScore));
            }
            
            // ถ้ามีรายการน้อยกว่าที่ต้องการ ให้สร้างเพิ่ม
            if (scoredItems.Count < count)
            {
                int needMore = count - scoredItems.Count;
                for (int i = 0; i < needMore; i++)
                {
                    var newItem = await GenerateNewRecommendationAsync(currentEmotion, type);
                    if (newItem != null)
                    {
                        await _dataService.SaveMusicGameItemAsync(newItem);
                        scoredItems.Add((newItem, 0.5f)); // ให้คะแนนปานกลาง
                    }
                }
            }
            
            // เรียงลำดับตามคะแนนและเลือกตามจำนวนที่ต้องการ
            return scoredItems
                .OrderByDescending(x => x.score)
                .Take(count)
                .Select(x => x.item)
                .ToList();
        }
        
        /// <summary>
        /// คำนวณความคล้ายคลึงระหว่างอารมณ์
        /// </summary>
        private float CalculateEmotionScore(string itemEmotion, string currentEmotion)
        {
            if (string.IsNullOrEmpty(itemEmotion) || string.IsNullOrEmpty(currentEmotion))
                return 0.3f; // คะแนนปานกลางถ้าไม่มีข้อมูล
                
            // ถ้าตรงกันพอดี
            if (itemEmotion.Equals(currentEmotion, StringComparison.OrdinalIgnoreCase))
                return 1.0f;
                
            // ถ้ามีส่วนที่ซ้อนทับกัน
            if (itemEmotion.Contains(currentEmotion, StringComparison.OrdinalIgnoreCase) || 
                currentEmotion.Contains(itemEmotion, StringComparison.OrdinalIgnoreCase))
                return 0.8f;
                
            // กลุ่มอารมณ์ที่คล้ายกัน
            var positiveEmotions = new[] { "happy", "excited", "relaxed", "peaceful", "content", "joyful" };
            var negativeEmotions = new[] { "sad", "angry", "anxious", "depressed", "frustrated", "stressed" };
            
            bool itemIsPositive = positiveEmotions.Any(e => itemEmotion.Contains(e, StringComparison.OrdinalIgnoreCase));
            bool currentIsPositive = positiveEmotions.Any(e => currentEmotion.Contains(e, StringComparison.OrdinalIgnoreCase));
            
            bool itemIsNegative = negativeEmotions.Any(e => itemEmotion.Contains(e, StringComparison.OrdinalIgnoreCase));
            bool currentIsNegative = negativeEmotions.Any(e => currentEmotion.Contains(e, StringComparison.OrdinalIgnoreCase));
            
            // ถ้าทั้งคู่เป็นอารมณ์บวกหรือลบเหมือนกัน
            if ((itemIsPositive && currentIsPositive) || (itemIsNegative && currentIsNegative))
                return 0.6f;
                
            return 0.2f; // คะแนนต่ำถ้าอารมณ์ต่างกันมาก
        }
        
        /// <summary>
        /// คำนวณคะแนนตามแนวที่ผู้ใช้ชอบ
        /// </summary>
        private float CalculateGenreScore(string genre)
        {
            if (string.IsNullOrEmpty(genre) || _userPreferences.Count == 0)
                return 0.5f; // คะแนนปานกลางถ้าไม่มีข้อมูล
                
            // หาค่าสูงสุดของความชอบ
            int maxPreference = _userPreferences.Values.Max();
            
            // ถ้าผู้ใช้เคยชอบแนวนี้
            if (_userPreferences.TryGetValue(genre, out int preferenceCount))
            {
                return (float)preferenceCount / maxPreference;
            }
            
            return 0.3f; // คะแนนต่ำถ้าไม่เคยชอบแนวนี้
        }
        
        /// <summary>
        /// สร้างคำแนะนำใหม่โดยใช้ AI
        /// </summary>
        private async Task<MusicGameItem?> GenerateNewRecommendationAsync(string emotion, ItemType type)
        {
            string typeStr = type == ItemType.Music ? "เพลง" : "เกม";
            
            // สร้างคำแนะนำที่เฉพาะเจาะจงมากขึ้น
            string prompt = $"ฉันรู้สึก {emotion} ตอนนี้ ช่วยแนะนำ{typeStr}ที่เหมาะกับอารมณ์นี้ " +
                           $"โดยพิจารณาว่าฉันชอบแนว {GetTopUserPreferences(3, type)} " +
                           $"ตอบเป็นภาษาไทยในรูปแบบ JSON เท่านั้น: {{\"title\": \"ชื่อ{typeStr}\", \"artist\": \"ศิลปิน/ผู้สร้าง\", " +
                           $"\"genre\": \"แนว\", \"description\": \"คำอธิบายสั้นๆ\"}}";

            var response = await _aiService.GenerateResponseAsync(prompt);
            
            try
            {
                // พยายามแยก JSON จากการตอบกลับ
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    
                    if (result != null)
                    {
                        string itemTitle = result.TryGetValue("title", out var title) ? title : $"แนะนำ{typeStr}สำหรับอารมณ์ {emotion}";
                        string itemArtist = result.TryGetValue("artist", out var artist) ? artist : "AI แนะนำ";
                        string itemGenre = result.TryGetValue("genre", out var genreVal) ? genreVal : GetTopUserPreferences(1, type);
                        string itemDesc = result.TryGetValue("description", out var desc) ? desc : $"{typeStr}ที่เหมาะกับอารมณ์ {emotion}";
                        
                        return new MusicGameItem
                        {
                            Title = itemTitle,
                            Artist = itemArtist,
                            Genre = itemGenre,
                            Description = itemDesc,
                            Type = type,
                            EmotionTag = emotion,
                            DateAdded = DateTime.Now
                        };
                    }
                }
            }
            catch (Exception)
            {
                // ถ้าการแยก JSON ล้มเหลว
                return null;
            }
            
            return null;
        }
        
        /// <summary>
        /// ดึงแนวที่ผู้ใช้ชอบมากที่สุด
        /// </summary>
        private string GetTopUserPreferences(int count, ItemType type)
        {
            if (_userPreferences.Count == 0)
                return type == ItemType.Music ? "Rock" : "Adventure";
                
            var topGenres = _userPreferences
                .OrderByDescending(p => p.Value)
                .Take(count)
                .Select(p => p.Key);
                
            return string.Join(", ", topGenres);
        }
    }
}