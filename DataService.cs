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
            return entry;
        }

        // ดึงข้อมูลอารมณ์ล่าสุด
        public async Task<List<EmotionEntry>> GetRecentEmotionEntriesAsync(int count = 10)
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
        }

        // ดึงข้อมูลอารมณ์ตามช่วงเวลา
        public async Task<List<EmotionEntry>> GetEmotionEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.EmotionEntries
                .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        // บันทึกข้อมูลเพลงหรือเกม
        public async Task<MusicGameItem> SaveMusicGameItemAsync(MusicGameItem item)
        {
            _dbContext.MusicGameItems.Add(item);
            await _dbContext.SaveChangesAsync();
            return item;
        }

        // ดึงข้อมูลเพลงหรือเกมตามประเภท
        public async Task<List<MusicGameItem>> GetMusicGameItemsByTypeAsync(ItemType type, int count = 10)
        {
            return await _dbContext.MusicGameItems
                .Where(i => i.Type == type)
                .OrderByDescending(i => i.DateAdded)
                .Take(count)
                .ToListAsync();
        }

        // ดึงข้อมูลเพลงหรือเกมตามอารมณ์
        public async Task<List<MusicGameItem>> GetMusicGameItemsByEmotionAsync(string emotion, ItemType type)
        {
            return await _dbContext.MusicGameItems
                .Where(i => i.Type == type && 
                           (i.EmotionTag.Contains(emotion) || emotion.Contains(i.EmotionTag)))
                .OrderByDescending(i => i.DateAdded)
                .ToListAsync();
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