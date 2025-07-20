using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PersonalAI
{
    /// <summary>
    /// หน้าต่างสำหรับตั้งค่าแอปพลิเคชัน
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly AIService _aiService;
        private readonly DataService _dataService;
        private AIServiceOptions _options;
        
        public SettingsWindow(AIService aiService, DataService dataService)
        {
            InitializeComponent();
            
            _aiService = aiService;
            _dataService = dataService;
            _options = _aiService.GetOptions();
            
            // โหลดค่าเริ่มต้น
            LoadSettings();
            
            // เพิ่มอีเวนต์สำหรับสไลเดอร์
            MaxTokensSlider.ValueChanged += (s, e) => 
            {
                MaxTokensTextBlock.Text = $"{(int)MaxTokensSlider.Value} tokens";
            };
        }
        
        /// <summary>
        /// โหลดการตั้งค่าปัจจุบัน
        /// </summary>
        private void LoadSettings()
        {
            AIServerUrlTextBox.Text = _options.ServerUrl;
            APIKeyTextBox.Text = _options.ApiKey;
            
            // เลือกโมเดลที่ใช้อยู่
            switch (_options.ModelName.ToLower())
            {
                case "gamma":
                    ModelComboBox.SelectedIndex = 0;
                    break;
                case "mistral":
                    ModelComboBox.SelectedIndex = 1;
                    break;
                case "dolphin":
                    ModelComboBox.SelectedIndex = 2;
                    break;
                case "llama":
                    ModelComboBox.SelectedIndex = 3;
                    break;
                default:
                    ModelComboBox.SelectedIndex = 0;
                    break;
            }
            
            MaxTokensSlider.Value = _options.MaxTokens;
            MaxTokensTextBlock.Text = $"{_options.MaxTokens} tokens";
        }
        
        /// <summary>
        /// บันทึกการตั้งค่า
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // อัปเดตการตั้งค่า
            _options.ServerUrl = AIServerUrlTextBox.Text.Trim();
            _options.ApiKey = APIKeyTextBox.Text.Trim();
            
            // อัปเดตชื่อโมเดล
            switch (ModelComboBox.SelectedIndex)
            {
                case 0:
                    _options.ModelName = "gamma";
                    break;
                case 1:
                    _options.ModelName = "mistral";
                    break;
                case 2:
                    _options.ModelName = "dolphin";
                    break;
                case 3:
                    _options.ModelName = "llama";
                    break;
            }
            
            _options.MaxTokens = (int)MaxTokensSlider.Value;
            
            // บันทึกการตั้งค่า
            _aiService.UpdateOptions(_options);
            
            UXHelper.ShowMessage("บันทึกการตั้งค่าเรียบร้อยแล้ว");
            Close();
        }
        
        /// <summary>
        /// ยกเลิกการตั้งค่า
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// ทดสอบการเชื่อมต่อกับ AI Server
        /// </summary>
        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var testOptions = new AIServiceOptions
                {
                    ServerUrl = AIServerUrlTextBox.Text.Trim(),
                    ApiKey = APIKeyTextBox.Text.Trim(),
                    ModelName = _options.ModelName,
                    MaxTokens = (int)MaxTokensSlider.Value
                };
                
                var testService = new AIService(testOptions);
                var response = await testService.GenerateResponseAsync("Hello, are you working?");
                
                if (!string.IsNullOrEmpty(response))
                {
                    UXHelper.ShowMessage("การเชื่อมต่อสำเร็จ! AI Server ตอบกลับ: " + response.Substring(0, Math.Min(100, response.Length)) + "...");
                }
                else
                {
                    UXHelper.ShowError("การเชื่อมต่อล้มเหลว: ไม่ได้รับการตอบกลับจาก AI Server");
                }
            }
            catch (Exception ex)
            {
                UXHelper.ShowError($"การเชื่อมต่อล้มเหลว: {ex.Message}");
            }
        }
        
        /// <summary>
        /// นำเข้าข้อมูล
        /// </summary>
        private async void ImportDataButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "เลือกไฟล์ข้อมูลที่จะนำเข้า"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(dialog.FileName);
                    var data = JsonSerializer.Deserialize<ExportData>(json);
                    
                    if (data != null)
                    {
                        // ยืนยันก่อนนำเข้า
                        var result = MessageBox.Show(
                            "การนำเข้าข้อมูลจะแทนที่ข้อมูลปัจจุบันทั้งหมด คุณต้องการดำเนินการต่อหรือไม่?",
                            "ยืนยันการนำเข้าข้อมูล",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);
                            
                        if (result == MessageBoxResult.Yes)
                        {
                            await _dataService.ImportDataAsync(data.EmotionEntries, data.MusicGameItems);
                            UXHelper.ShowMessage("นำเข้าข้อมูลเรียบร้อยแล้ว");
                        }
                    }
                    else
                    {
                        UXHelper.ShowError("รูปแบบไฟล์ไม่ถูกต้อง");
                    }
                }
                catch (Exception ex)
                {
                    UXHelper.ShowError($"เกิดข้อผิดพลาดในการนำเข้าข้อมูล: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// ส่งออกข้อมูล
        /// </summary>
        private async void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "บันทึกข้อมูลเป็นไฟล์",
                FileName = $"personal_ai_data_{DateTime.Now:yyyyMMdd}.json"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var (emotions, items) = await _dataService.ExportDataAsync();
                    
                    var exportData = new ExportData
                    {
                        EmotionEntries = emotions,
                        MusicGameItems = items,
                        ExportDate = DateTime.Now
                    };
                    
                    var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(dialog.FileName, json);
                    
                    UXHelper.ShowMessage("ส่งออกข้อมูลเรียบร้อยแล้ว");
                }
                catch (Exception ex)
                {
                    UXHelper.ShowError($"เกิดข้อผิดพลาดในการส่งออกข้อมูล: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// ล้างข้อมูลทั้งหมด
        /// </summary>
        private async void ClearDataButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "คุณแน่ใจหรือไม่ว่าต้องการล้างข้อมูลทั้งหมด? การกระทำนี้ไม่สามารถย้อนกลับได้",
                "ยืนยันการล้างข้อมูล",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _dataService.ImportDataAsync(new List<EmotionEntry>(), new List<MusicGameItem>());
                    UXHelper.ShowMessage("ล้างข้อมูลเรียบร้อยแล้ว");
                }
                catch (Exception ex)
                {
                    UXHelper.ShowError($"เกิดข้อผิดพลาดในการล้างข้อมูล: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลที่จะส่งออก
    /// </summary>
    public class ExportData
    {
        public List<EmotionEntry> EmotionEntries { get; set; } = new();
        public List<MusicGameItem> MusicGameItems { get; set; } = new();
        public DateTime ExportDate { get; set; }
    }
}