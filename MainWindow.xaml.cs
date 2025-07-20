using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalAI
{
    public partial class MainWindow : Window
    {
        private readonly EmotionAnalysisService _emotionService;
        private readonly DataService _dataService;
        private readonly AIService _aiService;
        private readonly ChatAIService _chatService;
        private readonly AdvancedEmotionAnalyzer _emotionAnalyzer;
        private readonly AdvancedRecommendationService _recommendationService;
        private readonly EmotionNotificationService _notificationService;
        private List<EmotionEntry> _recentEntries = new();
        private MusicGameItem? _recommendedMusic;
        private MusicGameItem? _recommendedGame;

        public MainWindow(
            EmotionAnalysisService emotionService, 
            DataService dataService, 
            AIService aiService,
            ChatAIService chatService,
            AdvancedEmotionAnalyzer emotionAnalyzer,
            AdvancedRecommendationService recommendationService,
            EmotionNotificationService notificationService)
        {
            InitializeComponent();
            _emotionService = emotionService;
            _dataService = dataService;
            _aiService = aiService;
            _chatService = chatService;
            _emotionAnalyzer = emotionAnalyzer;
            _recommendationService = recommendationService;
            _notificationService = notificationService;
            
            // Set window title
            Title = "ผู้ช่วย AI ส่วนตัวของฉัน - 2025 Edition";
            
            // Update emotion history when loaded
            Loaded += MainWindow_Loaded;
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateEmotionHistoryAsync();
        }
        
        private async Task UpdateEmotionHistoryAsync()
        {
            _recentEntries = await _emotionService.GetRecentEntriesAsync();
            EmotionHistoryListBox.ItemsSource = _recentEntries;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendButton.IsEnabled = false;
                var prompt = PromptTextBox.Text.Trim();

                if (string.IsNullOrEmpty(prompt))
                {
                    UXHelper.ShowMessage("กรุณาป้อนความรู้สึกหรือความคิดของคุณ");
                    return;
                }
                
                // ตรวจสอบความปลอดภัยของข้อความ
                if (!SecurityHelper.IsSafeContent(prompt))
                {
                    UXHelper.ShowError("ข้อความของคุณมีเนื้อหาที่ไม่เหมาะสม กรุณาแก้ไขและลองอีกครั้ง");
                    return;
                }
                
                // ตรวจสอบการเชื่อมต่ออินเทอร์เน็ต
                if (!UXHelper.IsInternetConnected())
                {
                    UXHelper.ShowError("ไม่มีการเชื่อมต่ออินเทอร์เน็ต กรุณาตรวจสอบการเชื่อมต่อของคุณและลองอีกครั้ง");
                    return;
                }

                ResponseTextBox.Text = "กำลังวิเคราะห์อารมณ์ของคุณ...";
                var entry = await _emotionService.AnalyzeEmotionAsync(prompt);
                
                // ขอคำแนะนำเพลงและเกม
                var recommendations = await _emotionService.GetRecommendationsAsync(entry.EmotionLabel);
                _recommendedMusic = recommendations.music;
                _recommendedGame = recommendations.game;
                
                var result = $"อารมณ์: {entry.EmotionLabel}\n" +
                             $"ระดับความเครียด: {entry.StressLevel}/10\n" +
                             $"ความเป็นบวก: {entry.PositivityScore}/10\n\n" +
                             $"{entry.AIResponse}";
                
                ResponseTextBox.Text = result;
                PromptTextBox.Text = string.Empty;
                
                // อัปเดตส่วนแสดงคำแนะนำเพลง
                MusicTitleTextBlock.Text = _recommendedMusic.Title;
                MusicArtistTextBlock.Text = _recommendedMusic.Artist;
                MusicGenreTextBlock.Text = $"แนว: {_recommendedMusic.Genre}";
                
                // อัปเดตส่วนแสดงคำแนะนำเกม
                GameTitleTextBlock.Text = _recommendedGame.Title;
                GameArtistTextBlock.Text = _recommendedGame.Artist;
                GameGenreTextBlock.Text = $"แนว: {_recommendedGame.Genre}";
                
                // Update emotion history
                await UpdateEmotionHistoryAsync();
            }
            catch (Exception ex)
            {
                ResponseTextBox.Text = string.Empty;
                UXHelper.ShowError($"เกิดข้อผิดพลาด: {ex.Message}");
            }
            finally
            {
                SendButton.IsEnabled = true;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseTextBox.Text = string.Empty;
        }
        
        /// <summary>
        /// เปิดหน้าต่างแสดงกราฟแนวโน้มอารมณ์และความเครียด
        /// </summary>
        private void TrendsButton_Click(object sender, RoutedEventArgs e)
        {
            var trendsWindow = new EmotionTrendsWindow(_emotionService, _dataService);
            trendsWindow.Owner = this;
            trendsWindow.ShowDialog();
        }
        
        /// <summary>
        /// เทรนโมเดลวิเคราะห์ความรู้สึก
        /// </summary>
        private async void TrainButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TrainButton.IsEnabled = false;
                TrainButton.Content = "กำลังเทรน...";
                
                await _emotionService.TrainSentimentModelAsync();
                
                UXHelper.ShowMessage("เทรนโมเดลเสร็จสิ้น ระบบจะวิเคราะห์ความรู้สึกได้แม่นยำขึ้น");
            }
            catch (Exception ex)
            {
                UXHelper.ShowError($"เกิดข้อผิดพลาดในการเทรนโมเดล: {ex.Message}");
            }
            finally
            {
                TrainButton.IsEnabled = true;
                TrainButton.Content = "เทรนโมเดล";
            }
        }
        
        /// <summary>
        /// เปิดหน้าต่างตั้งค่า
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_aiService, _dataService);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }
        
        /// <summary>
        /// เปิดหน้าต่างตั้งค่านิสัยส่วนตัว
        /// </summary>
        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            var preferencesWindow = new UserPreferencesWindow();
            preferencesWindow.Owner = this;
            preferencesWindow.ShowDialog();
        }
        
        /// <summary>
        /// เปิดหน้าต่างแชทกับ AI
        /// </summary>
        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var chatWindow = new ChatWindow(
                _chatService,
                _emotionAnalyzer,
                _recommendationService,
                _notificationService);
            chatWindow.Owner = this;
            chatWindow.ShowDialog();
        }
    }
}