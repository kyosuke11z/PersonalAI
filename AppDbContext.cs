using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PersonalAI
{
    public class AppDbContext : DbContext
    {
        public DbSet<EmotionEntry> EmotionEntries { get; set; }
        public DbSet<MusicGameItem> MusicGameItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // กำหนดที่เก็บฐานข้อมูลในโฟลเดอร์ AppData ของผู้ใช้
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalAICompanion");
            
            // สร้างโฟลเดอร์ถ้ายังไม่มี
            Directory.CreateDirectory(appDataPath);
            
            string dbPath = Path.Combine(appDataPath, "personalai.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // กำหนดคุณสมบัติของตาราง EmotionEntries
            modelBuilder.Entity<EmotionEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();
                entity.Property(e => e.EmotionLabel).IsRequired();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // กำหนดคุณสมบัติของตาราง MusicGameItems
            modelBuilder.Entity<MusicGameItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Artist).IsRequired();
                entity.Property(e => e.DateAdded).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}