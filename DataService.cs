using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PersonalAI
{
    public class DataService
    {
        private readonly AppDbContext _dbContext;

        public DataService()
        {
            _dbContext = new AppDbContext();
            // สร้างฐานข้อมูลถ้ายังไม่มี
            _dbContext.Database.EnsureCreated();
        }

        // บันทึกข้อมูลอารมณ์
        public async Task<EmotionEntry> SaveEmotionEntryAsync(EmotionEntry entry)
        {
            // เข้ารหัสข้อมูลส่วนตัวก่อนบันทึก
            entry.Text = SecurityHelper.Encrypt(entry.Text);
            entry.AIResponse = SecurityHelper.Encrypt(entry.AIResponse);
            
            _dbContext.EmotionEntries.Add(entry);
            await _dbContext.SaveChangesAsync();
            
            // ล้างแคชที่เกี่ยวข้องกับข้อมูลอารมณ์
            DataCache.Remove($"RecentEmotions_10");
            
            // ล้างแคชข้อมูลตามช่วงวันที่เกี่ยวข้อง
            string today = DateTime.Now.ToString("yyyyMMdd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            string lastWeek = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            string lastMonth = DateTime.Now.AddDays(-30).ToString("yyyyMMdd");
            
            DataCache.Remove($"EmotionsByDateRange_{lastWeek}_{today}");
            DataCache.Remove($"EmotionsByDateRange_{lastMonth}_{today}");
            
            return entry;
        }

        // ดึงข้อมูลอารมณ์ล่าสุด
        public async Task<List<EmotionEntry>> GetRecentEmotionEntriesAsync(int count = 10)
        {
            string cacheKey = $"RecentEmotions_{count}";
            
            return await DataCache.GetOrCreateAsync(cacheKey, async () =>
            {
                var entries = await _dbContext.EmotionEntries
                    .OrderByDescending(e => e.Timestamp)
                    .Take(count)
                    .ToListAsync();
                    
                // ถอดรหัสข้อมูลก่อนส่งคืน
                foreach (var entry in entries)
                {
                    entry.Text = SecurityHelper.Decrypt(entry.Text);
                    entry.AIResponse = SecurityHelper.Decrypt(entry.AIResponse);
                }
                
                return entries;
            }, TimeSpan.FromMinutes(5));
        }

        // ดึงข้อมูลอารมณ์ตามช่วงเวลา
        public async Task<List<EmotionEntry>> GetEmotionEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            string cacheKey = $"EmotionsByDateRange_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            return await DataCache.GetOrCreateAsync(cacheKey, async () =>
            {
                var entries = await _dbContext.EmotionEntries
                    .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                    .OrderByDescending(e => e.Timestamp)
                    .ToListAsync();
                    
                // ถอดรหัสข้อมูลก่อนส่งคืน
                foreach (var entry in entries)
                {
                    if (!string.IsNullOrEmpty(entry.Text))
                        entry.Text = SecurityHelper.Decrypt(entry.Text);
                    if (!string.IsNullOrEmpty(entry.AIResponse))
                        entry.AIResponse = SecurityHelper.Decrypt(entry.AIResponse);
                }
                
                return entries;
            }, TimeSpan.FromMinutes(10));
        }

        // บันทึกข้อมูลเพลงหรือเกม
        public async Task<MusicGameItem> SaveMusicGameItemAsync(MusicGameItem item)
        {
            _dbContext.MusicGameItems.Add(item);
            await _dbContext.SaveChangesAsync();
            
            // ล้างแคชที่เกี่ยวข้องกับข้อมูลเพลงหรือเกม
            DataCache.Remove($"MusicGameItems_{item.Type}_10");
            
            // ล้างแคชที่เกี่ยวข้องกับอารมณ์
            if (!string.IsNullOrEmpty(item.EmotionTag))
            {
                DataCache.Remove($"MusicGameItems_{item.EmotionTag}_{item.Type}");
            }
            
            return item;
        }

        // ดึงข้อมูลเพลงหรือเกมตามประเภท
        public async Task<List<MusicGameItem>> GetMusicGameItemsByTypeAsync(ItemType type, int count = 10)
        {
            string cacheKey = $"MusicGameItems_{type}_{count}";
            
            return await DataCache.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _dbContext.MusicGameItems
                    .Where(i => i.Type == type)
                    .OrderByDescending(i => i.DateAdded)
                    .Take(count)
                    .ToListAsync();
            }, TimeSpan.FromHours(1));
        }

        // ดึงข้อมูลเพลงหรือเกมตามอารมณ์
        public async Task<List<MusicGameItem>> GetMusicGameItemsByEmotionAsync(string emotion, ItemType type)
        {
            string cacheKey = $"MusicGameItems_{emotion}_{type}";
            
            return await DataCache.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _dbContext.MusicGameItems
                    .Where(i => i.Type == type && 
                               (i.EmotionTag.Contains(emotion) || emotion.Contains(i.EmotionTag)))
                    .OrderByDescending(i => i.DateAdded)
                    .ToListAsync();
            }, TimeSpan.FromHours(1));
        }

        // นำเข้าข้อมูล
        public async Task ImportDataAsync(List<EmotionEntry> emotions, List<MusicGameItem> items)
        {
            // ล้างข้อมูลเดิม
            _dbContext.EmotionEntries.RemoveRange(_dbContext.EmotionEntries);
            _dbContext.MusicGameItems.RemoveRange(_dbContext.MusicGameItems);
            
            // เพิ่มข้อมูลใหม่
            await _dbContext.EmotionEntries.AddRangeAsync(emotions);
            await _dbContext.MusicGameItems.AddRangeAsync(items);
            
            await _dbContext.SaveChangesAsync();
            
            // ล้างแคชทั้งหมดเมื่อมีการนำเข้าข้อมูลใหม่
            DataCache.Clear();
        }

        // ส่งออกข้อมูล
        public async Task<(List<EmotionEntry> emotions, List<MusicGameItem> items)> ExportDataAsync()
        {
            var emotions = await _dbContext.EmotionEntries.ToListAsync();
            var items = await _dbContext.MusicGameItems.ToListAsync();
            
            return (emotions, items);
        }
    }
}