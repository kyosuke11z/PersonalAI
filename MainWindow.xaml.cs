using System;
using System.Windows;
using PersonalAI.ViewModels;

namespace PersonalAI
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;

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

            _vm = new MainWindowViewModel(
                emotionService, dataService, aiService,
                chatService, emotionAnalyzer,
                recommendationService, notificationService);

            DataContext = _vm;
            Title = "ผู้ช่วย AI ส่วนตัวของฉัน - 2025 Edition";
            Loaded += async (_, _) => await _vm.InitializeAsync();
        }

        private void TrendsButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new EmotionTrendsWindow(_vm.EmotionService, _vm.DataService);
            w.Owner = this;
            w.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new SettingsWindow(_vm.AIService, _vm.DataService);
            w.Owner = this;
            w.ShowDialog();
        }

        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new UserPreferencesWindow();
            w.Owner = this;
            w.ShowDialog();
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new ChatWindow(
                _vm.ChatService,
                _vm.EmotionAnalyzer,
                _vm.RecommendationService,
                _vm.NotificationService);
            w.Owner = this;
            w.ShowDialog();
        }
    }
}
