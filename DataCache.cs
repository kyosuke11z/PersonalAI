using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับจัดการการแคชข้อมูล
    /// </summary>
    public class DataCache
    {
        private static readonly Dictionary<string, CacheItem> _cache = new();
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        
        /// <summary>
        /// รับข้อมูลจากแคช หรือเรียกใช้ฟังก์ชันเพื่อดึงข้อมูลหากไม่มีในแคช
        /// </summary>
        public static async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            // ตรวจสอบว่ามีข้อมูลในแคชหรือไม่
            if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
            {
                return (T)item.Value;
            }
            
            // ถ้าไม่มีข้อมูลในแคช ให้ล็อคเพื่อป้องกันการเรียกใช้ factory พร้อมกัน
            await _semaphore.WaitAsync();
            
            try
            {
                // ตรวจสอบอีกครั้งหลังจากล็อค (เผื่อมีเธรดอื่นสร้างข้อมูลไปแล้ว)
                if (_cache.TryGetValue(key, out item) && !item.IsExpired)
                {
                    return (T)item.Value;
                }
                
                // เรียกใช้ factory เพื่อดึงข้อมูล
                var value = await factory();
                
                // กำหนดเวลาหมดอายุ (ค่าเริ่มต้น 5 นาที)
                var expirationTime = expiration ?? TimeSpan.FromMinutes(5);
                
                // เก็บข้อมูลในแคช
                _cache[key] = new CacheItem
                {
                    Value = value,
                    ExpirationTime = DateTime.Now.Add(expirationTime)
                };
                
                return value;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// ลบข้อมูลจากแคช
        /// </summary>
        public static void Remove(string key)
        {
            _cache.Remove(key);
        }
        
        /// <summary>
        /// ล้างแคชทั้งหมด
        /// </summary>
        public static void Clear()
        {
            _cache.Clear();
        }
        
        /// <summary>
        /// ล้างข้อมูลที่หมดอายุออกจากแคช
        /// </summary>
        public static void CleanExpiredItems()
        {
            var expiredKeys = new List<string>();
            
            foreach (var kvp in _cache)
            {
                if (kvp.Value.IsExpired)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }
            
            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }
        }
        
        /// <summary>
        /// คลาสสำหรับเก็บข้อมูลในแคช
        /// </summary>
        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime ExpirationTime { get; set; }
            public bool IsExpired => DateTime.Now > ExpirationTime;
        }
    }
}