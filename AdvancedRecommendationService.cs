using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalAI
{
    /// <summary>
    /// บริการแนะนำกิจกรรมแบบชาญฉลาด
    /// </summary>
    public class AdvancedRecommendationService
    {
        private readonly AIService _aiService;
        private readonly UserPreferences _userPreferences;
        private readonly DataService _dataService;
        
        // ฐานข้อมูลเพลงและเกม
        private readonly List<MusicGameItem> _musicDatabase = new();
        private readonly List<MusicGameItem> _gameDatabase = new();
        
        // ประวัติการแนะนำ
        private readonly List<MusicGameItem> _recentMusicRecommendations = new();
        private readonly List<MusicGameItem> _recentGameRecommendations = new();
        
        public AdvancedRecommendationService(
            AIService aiService,
            UserPreferences userPreferences,
            DataService dataService)
        {
            _aiService = aiService;
            _userPreferences = userPreferences;
            _dataService = dataService;
            
            // โหลดฐานข้อมูลเพลงและเกม
            InitializeDatabases();
        }
        
        /// <summary>
        /// โหลดฐานข้อมูลเพลงและเกม
        /// </summary>
        private void InitializeDatabases()
        {
            // เพลง
            _musicDatabase.AddRange(new List<MusicGameItem>
            {
                new MusicGameItem { Title = "Bohemian Rhapsody", Artist = "Queen", Genre = "Rock", Mood = "หลากหลาย", Rating = (int)9.5f },
                new MusicGameItem { Title = "Happy", Artist = "Pharrell Williams", Genre = "Pop", Mood = "มีความสุข", Rating = (int)8.7f },
                new MusicGameItem { Title = "Someone Like You", Artist = "Adele", Genre = "Pop", Mood = "เศร้า", Rating = (int)9.2f },
                new MusicGameItem { Title = "Eye of the Tiger", Artist = "Survivor", Genre = "Rock", Mood = "มุ่งมั่น", Rating = (int)8.5f },
                new MusicGameItem { Title = "Relaxing Piano", Artist = "Various Artists", Genre = "Classical", Mood = "ผ่อนคลาย", Rating = (int)8.0f },
                new MusicGameItem { Title = "Lose Yourself", Artist = "Eminem", Genre = "Hip Hop", Mood = "มุ่งมั่น", Rating = (int)9.0f },
                new MusicGameItem { Title = "Let It Go", Artist = "Idina Menzel", Genre = "Soundtrack", Mood = "ปลดปล่อย", Rating = (int)8.3f },
                new MusicGameItem { Title = "Numb", Artist = "Linkin Park", Genre = "Rock", Mood = "โกรธ", Rating = (int)8.8f },
                new MusicGameItem { Title = "Weightless", Artist = "Marconi Union", Genre = "Ambient", Mood = "ผ่อนคลาย", Rating = (int)7.9f },
                new MusicGameItem { Title = "Don't Worry Be Happy", Artist = "Bobby McFerrin", Genre = "Jazz", Mood = "มีความสุข", Rating = (int)8.2f }
            });
            
            // เกม
            _gameDatabase.AddRange(new List<MusicGameItem>
            {
                new MusicGameItem { Title = "Animal Crossing", Artist = "Nintendo", Genre = "Simulation", Mood = "ผ่อนคลาย", Rating = (int)9.0f },
                new MusicGameItem { Title = "DOOM Eternal", Artist = "id Software", Genre = "FPS", Mood = "โกรธ", Rating = (int)9.2f },
                new MusicGameItem { Title = "Journey", Artist = "thatgamecompany", Genre = "Adventure", Mood = "สงบ", Rating = (int)9.5f },
                new MusicGameItem { Title = "Stardew Valley", Artist = "ConcernedApe", Genre = "Simulation", Mood = "ผ่อนคลาย", Rating = (int)9.3f },
                new MusicGameItem { Title = "Tetris Effect", Artist = "Monstars Inc.", Genre = "Puzzle", Mood = "มีสมาธิ", Rating = (int)8.8f },
                new MusicGameItem { Title = "Celeste", Artist = "Maddy Makes Games", Genre = "Platformer", Mood = "มุ่งมั่น", Rating = (int)9.4f },
                new MusicGameItem { Title = "The Last of Us", Artist = "Naughty Dog", Genre = "Action-Adventure", Mood = "เศร้า", Rating = (int)9.7f },
                new MusicGameItem { Title = "Untitled Goose Game", Artist = "House House", Genre = "Puzzle", Mood = "สนุกสนาน", Rating = (int)8.5f },
                new MusicGameItem { Title = "Minecraft", Artist = "Mojang", Genre = "Sandbox", Mood = "สร้างสรรค์", Rating = (int)9.6f },
                new MusicGameItem { Title = "Flower", Artist = "thatgamecompany", Genre = "Adventure", Mood = "ผ่อนคลาย", Rating = (int)8.7f }
            });
        }

        /// <summary>
        /// รับคำแนะนำเพลงและเกมตามอารมณ์
        /// </summary>
        public (MusicGameItem music, MusicGameItem game) GetRecommendations(string emotion, DetailedEmotionResult? detailedEmotion = null)
        {
            // ถ้ามีข้อมูลอารมณ์เชิงลึก ให้ใช้ข้อมูลนั้น
            if (detailedEmotion != null)
            {
                return GetAdvancedRecommendations(detailedEmotion);
            }

            // แปลงอารมณ์เป็นคำสำคัญสำหรับการค้นหา
            var moodKeywords = GetMoodKeywords(emotion);

            // ค้นหาเพลงที่เหมาะสม
            var musicRecommendation = FindBestMatch(_musicDatabase, moodKeywords);

            // ค้นหาเกมที่เหมาะสม
            var gameRecommendation = FindBestMatch(_gameDatabase, moodKeywords);

            // เพิ่มลงในประวัติการแนะนำ
            _recentMusicRecommendations.Add(musicRecommendation);
            _recentGameRecommendations.Add(gameRecommendation);

            // จำกัดขนาดของประวัติ
            if (_recentMusicRecommendations.Count > 10)
                _recentMusicRecommendations.RemoveAt(0);
            if (_recentGameRecommendations.Count > 10)
                _recentGameRecommendations.RemoveAt(0);

            return (musicRecommendation, gameRecommendation);
        }

        /// <summary>
        /// รับคำแนะนำเพลงและเกมตามอารมณ์เชิงลึก
        /// </summary>
        private (MusicGameItem music, MusicGameItem game) GetAdvancedRecommendations(DetailedEmotionResult detailedEmotion)
        {
            var scores = detailedEmotion.EmotionScores;
            if (scores == null)
            {
                // ถ้าไม่มีคะแนนอารมณ์ ให้ใช้วิธีปกติ
                return (FindBestMatch(_musicDatabase, new[] { detailedEmotion.PrimaryEmotion ?? "เฉยๆ" }),
                        FindBestMatch(_gameDatabase, new[] { detailedEmotion.PrimaryEmotion ?? "เฉยๆ" }));
            }
            
            // ตรวจสอบอารมณ์หลัก
            string dominantMood;
            if (scores.Happy > scores.Sad && scores.Happy > scores.Angry && scores.Happy > scores.Anxious)
            {
                dominantMood = "มีความสุข";
            }
            else if (scores.Sad > scores.Happy && scores.Sad > scores.Angry && scores.Sad > scores.Anxious)
            {
                dominantMood = "เศร้า";
            }
            else if (scores.Angry > scores.Happy && scores.Angry > scores.Sad && scores.Angry > scores.Anxious)
            {
                dominantMood = "โกรธ";
            }
            else if (scores.Anxious > scores.Happy && scores.Anxious > scores.Sad && scores.Anxious > scores.Angry)
            {
                dominantMood = "กังวล";
            }
            else
            {
                dominantMood = "เฉยๆ";
            }
            
            // ปรับแนะนำตามอารมณ์หลัก
            MusicGameItem musicRecommendation;
            MusicGameItem gameRecommendation;
            
            switch (dominantMood)
            {
                case "มีความสุข":
                    // แนะนำเพลงและเกมที่สนุกสนาน
                    musicRecommendation = FindBestMatch(_musicDatabase, new[] { "มีความสุข", "สนุกสนาน" });
                    gameRecommendation = FindBestMatch(_gameDatabase, new[] { "สนุกสนาน", "สร้างสรรค์" });
                    break;
                    
                case "เศร้า":
                    // แนะนำเพลงที่เข้ากับอารมณ์เศร้า แต่แนะนำเกมที่ช่วยให้อารมณ์ดีขึ้น
                    musicRecommendation = FindBestMatch(_musicDatabase, new[] { "เศร้า", "ซึ้ง" });
                    gameRecommendation = FindBestMatch(_gameDatabase, new[] { "ผ่อนคลาย", "สงบ" });
                    break;
                    
                case "โกรธ":
                    // แนะนำเพลงที่ช่วยระบายอารมณ์ และเกมที่ช่วยระบายความรู้สึก
                    musicRecommendation = FindBestMatch(_musicDatabase, new[] { "มุ่งมั่น", "พลังงาน" });
                    gameRecommendation = FindBestMatch(_gameDatabase, new[] { "FPS", "Action" });
                    break;
                    
                case "กังวล":
                    // แนะนำเพลงและเกมที่ช่วยผ่อนคลาย
                    musicRecommendation = FindBestMatch(_musicDatabase, new[] { "ผ่อนคลาย", "สงบ" });
                    gameRecommendation = FindBestMatch(_gameDatabase, new[] { "ผ่อนคลาย", "สงบ" });
                    break;
                    
                default:
                    // แนะนำเพลงและเกมทั่วไป
                    musicRecommendation = FindBestMatch(_musicDatabase, new[] { "หลากหลาย" });
                    gameRecommendation = FindBestMatch(_gameDatabase, new[] { "สร้างสรรค์" });
                    break;
            }
            
            // ตรวจสอบว่าเคยแนะนำไปแล้วหรือไม่
            if (_recentMusicRecommendations.Any(m => m.Title == musicRecommendation.Title))
            {
                // ถ้าเคยแนะนำแล้ว ให้เลือกเพลงอื่น
                var alternativeMusic = _musicDatabase
                    .Where(m => !_recentMusicRecommendations.Any(r => r.Title == m.Title))
                    .OrderByDescending(m => m.Rating)
                    .FirstOrDefault();
                    
                if (alternativeMusic != null)
                {
                    musicRecommendation = alternativeMusic;
                }
            }
            
            if (_recentGameRecommendations.Any(g => g.Title == gameRecommendation.Title))
            {
                // ถ้าเคยแนะนำแล้ว ให้เลือกเกมอื่น
                var alternativeGame = _gameDatabase
                    .Where(g => !_recentGameRecommendations.Any(r => r.Title == g.Title))
                    .OrderByDescending(g => g.Rating)
                    .FirstOrDefault();
                    
                if (alternativeGame != null)
                {
                    gameRecommendation = alternativeGame;
                }
            }
            
            // เพิ่มลงในประวัติการแนะนำ
            _recentMusicRecommendations.Add(musicRecommendation);
            _recentGameRecommendations.Add(gameRecommendation);
            
            // จำกัดขนาดของประวัติ
            if (_recentMusicRecommendations.Count > 10)
                _recentMusicRecommendations.RemoveAt(0);
            if (_recentGameRecommendations.Count > 10)
                _recentGameRecommendations.RemoveAt(0);
            
            return (musicRecommendation, gameRecommendation);
        }
        
        /// <summary>
        /// แปลงอารมณ์เป็นคำสำคัญสำหรับการค้นหา
        /// </summary>
        private string[] GetMoodKeywords(string emotion)
        {
            emotion = emotion.ToLower();
            
            if (emotion.Contains("สุข") || emotion.Contains("ดี") || emotion.Contains("สนุก"))
            {
                return new[] { "มีความสุข", "สนุกสนาน" };
            }
            else if (emotion.Contains("เศร้า") || emotion.Contains("เสียใจ"))
            {
                return new[] { "เศร้า", "ซึ้ง" };
            }
            else if (emotion.Contains("โกรธ") || emotion.Contains("หงุดหงิด"))
            {
                return new[] { "มุ่งมั่น", "พลังงาน" };
            }
            else if (emotion.Contains("กังวล") || emotion.Contains("เครียด"))
            {
                return new[] { "ผ่อนคลาย", "สงบ" };
            }
            else
            {
                return new[] { "หลากหลาย" };
            }
        }
        
        /// <summary>
        /// ค้นหารายการที่เหมาะสมที่สุดจากฐานข้อมูล
        /// </summary>
        private MusicGameItem FindBestMatch(List<MusicGameItem> database, string[] keywords)
        {
            // กรองรายการที่ตรงกับคำสำคัญ
            var matches = database.Where(item => 
                keywords.Any(keyword => 
                    item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    item.Artist.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    item.Genre.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    item.Mood.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                )
            ).ToList();
            
            // สร้างตัวแปร random สำหรับใช้ในฟังก์ชันนี้
            var random = new Random();
            
            // ถ้าไม่มีรายการที่ตรงกับคำสำคัญ ให้สุ่มจากทั้งหมด
            if (matches.Count == 0)
            {
                return database[random.Next(database.Count)];
            }
            
            // เรียงลำดับตามคะแนนและสุ่มจาก 3 อันดับแรก
            matches = matches.OrderByDescending(item => item.Rating).ToList();
            var topCount = Math.Min(3, matches.Count);
            return matches[random.Next(topCount)];
        }
        
        /// <summary>
        /// เพิ่มรายการเพลงใหม่ลงในฐานข้อมูล
        /// </summary>
        public void AddMusicItem(MusicGameItem item)
        {
            _musicDatabase.Add(item);
        }
        
        /// <summary>
        /// เพิ่มรายการเกมใหม่ลงในฐานข้อมูล
        /// </summary>
        public void AddGameItem(MusicGameItem item)
        {
            _gameDatabase.Add(item);
        }
        
        /// <summary>
        /// รับรายการเพลงทั้งหมด
        /// </summary>
        public List<MusicGameItem> GetAllMusic()
        {
            return new List<MusicGameItem>(_musicDatabase);
        }
        
        /// <summary>
        /// รับรายการเกมทั้งหมด
        /// </summary>
        public List<MusicGameItem> GetAllGames()
        {
            return new List<MusicGameItem>(_gameDatabase);
        }
    }
}