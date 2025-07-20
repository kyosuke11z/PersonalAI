namespace PersonalAI
{
    public class RecommendationService
    {
        private readonly AIService _aiService;
        private readonly List<MusicGameItem> _items = new();
        private readonly Random _random = new();

        // ข้อมูลเริ่มต้นสำหรับแนะนำ
        private readonly Dictionary<string, List<string>> _emotionToGenres = new()
        {
            { "happy", new List<string> { "Pop", "Dance", "Upbeat Rock", "Party Games", "Casual Games" } },
            { "sad", new List<string> { "Ballad", "Slow Rock", "Blues", "Story-driven Games", "Puzzle Games" } },
            { "angry", new List<string> { "Metal", "Hard Rock", "Punk", "Action Games", "Fighting Games" } },
            { "anxious", new List<string> { "Ambient", "Classical", "Lo-fi", "Simulation Games", "Farming Games" } },
            { "relaxed", new List<string> { "Jazz", "Acoustic", "Chill", "Adventure Games", "RPG Games" } },
            { "excited", new List<string> { "EDM", "Rock", "Pop Rock", "Racing Games", "Sports Games" } }
        };

        public RecommendationService(AIService aiService)
        {
            _aiService = aiService;
            InitializeDefaultItems();
        }

        private void InitializeDefaultItems()
        {
            // เพิ่มเพลงตัวอย่าง
            AddItem(new MusicGameItem
            {
                Title = "Bohemian Rhapsody",
                Artist = "Queen",
                Type = ItemType.Music,
                Genre = "Rock",
                EmotionTag = "excited",
                Description = "A classic rock anthem with emotional depth",
                DateAdded = DateTime.Now
            });

            AddItem(new MusicGameItem
            {
                Title = "Nothing Else Matters",
                Artist = "Metallica",
                Type = ItemType.Music,
                Genre = "Metal",
                EmotionTag = "relaxed",
                Description = "A slower, melodic metal ballad",
                DateAdded = DateTime.Now
            });

            // เพิ่มเกมตัวอย่าง
            AddItem(new MusicGameItem
            {
                Title = "Stardew Valley",
                Artist = "ConcernedApe",
                Type = ItemType.Game,
                Genre = "Simulation",
                EmotionTag = "relaxed",
                Description = "A relaxing farming simulation game",
                DateAdded = DateTime.Now
            });

            AddItem(new MusicGameItem
            {
                Title = "DOOM Eternal",
                Artist = "id Software",
                Type = ItemType.Game,
                Genre = "FPS",
                EmotionTag = "angry",
                Description = "Fast-paced action shooter",
                DateAdded = DateTime.Now
            });
        }

        public void AddItem(MusicGameItem item)
        {
            item.Id = _items.Count > 0 ? _items.Max(i => i.Id) + 1 : 1;
            _items.Add(item);
        }

        public async Task<MusicGameItem> GetRecommendationAsync(string emotion, ItemType type)
        {
            // 1. ตรวจสอบว่ามีรายการที่ตรงกับอารมณ์หรือไม่
            var matchingItems = _items
                .Where(i => i.Type == type && 
                           (i.EmotionTag.Equals(emotion, StringComparison.OrdinalIgnoreCase) ||
                            i.EmotionTag.Contains(emotion, StringComparison.OrdinalIgnoreCase) ||
                            emotion.Contains(i.EmotionTag, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // 2. ถ้ามี ให้สุ่มเลือกหนึ่งรายการ
            if (matchingItems.Count > 0)
            {
                return matchingItems[_random.Next(matchingItems.Count)];
            }

            // 3. ถ้าไม่มี ให้ขอคำแนะนำจาก AI
            var item = await GenerateRecommendationAsync(emotion, type);
            AddItem(item);
            return item;
        }

        private async Task<MusicGameItem> GenerateRecommendationAsync(string emotion, ItemType type)
        {
            string typeStr = type == ItemType.Music ? "เพลง" : "เกม";
            string prompt = $"จากอารมณ์ '{emotion}' ช่วยแนะนำ{typeStr}ที่เหมาะสมหนึ่งรายการ " +
                           $"ตอบเป็นภาษาไทยในรูปแบบ JSON เท่านั้น: {{\"title\": \"ชื่อ\", \"artist\": \"ผู้สร้าง\", \"genre\": \"แนว\", \"description\": \"คำอธิบายสั้นๆ\"}}";

            var response = await _aiService.GenerateResponseAsync(prompt);
            
            try
            {
                // พยายามแยก JSON จากการตอบกลับ
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    
                    if (result != null)
                    {
                        string itemTitle = result.TryGetValue("title", out var title) ? title : "Unknown Title";
                        string itemArtist = result.TryGetValue("artist", out var artist) ? artist : "Unknown Artist";
                        string itemGenre = result.TryGetValue("genre", out var genreVal) ? genreVal : "Unknown Genre";
                        string itemDesc = result.TryGetValue("description", out var desc) ? desc : "No description available";
                        
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
                // ถ้าการแยก JSON ล้มเหลว ใช้ค่าเริ่มต้น
            }

            // ค่าเริ่มต้นถ้าไม่สามารถแยก JSON ได้
            string genre = GetGenreForEmotion(emotion, type);
            return new MusicGameItem
            {
                Title = $"Recommended {typeStr} for {emotion} mood",
                Artist = "AI Recommendation",
                Genre = genre,
                Description = response,
                Type = type,
                EmotionTag = emotion,
                DateAdded = DateTime.Now
            };
        }

        private string GetGenreForEmotion(string emotion, ItemType type)
        {
            // หาอารมณ์ที่ใกล้เคียงที่สุด
            string closestEmotion = "happy"; // ค่าเริ่มต้น
            foreach (var key in _emotionToGenres.Keys)
            {
                if (emotion.Contains(key, StringComparison.OrdinalIgnoreCase) || 
                    key.Contains(emotion, StringComparison.OrdinalIgnoreCase))
                {
                    closestEmotion = key;
                    break;
                }
            }

            // เลือกประเภทตามอารมณ์
            if (_emotionToGenres.TryGetValue(closestEmotion, out var genres))
            {
                int index = type == ItemType.Music ? 0 : Math.Min(3, genres.Count - 1);
                return genres[Math.Min(index, genres.Count - 1)];
            }

            return type == ItemType.Music ? "Pop" : "Casual";
        }

        public List<MusicGameItem> GetItemsByType(ItemType type, int count = 10)
        {
            return _items
                .Where(i => i.Type == type)
                .OrderByDescending(i => i.DateAdded)
                .Take(count)
                .ToList();
        }
    }
}