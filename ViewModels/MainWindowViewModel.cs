using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PersonalAI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly EmotionAnalysisService _emotionService;
        private readonly DataService _dataService;
        private readonly AIService _aiService;
        private readonly ChatAIService _chatService;
        private readonly AdvancedEmotionAnalyzer _emotionAnalyzer;
        private readonly AdvancedRecommendationService _recommendationService;
        private readonly EmotionNotificationService _notificationService;

        // ── Observable Properties ─────────────────────────────────────────────
        [ObservableProperty]
        private string promptText = string.Empty;

        [ObservableProperty]
        private string responseText = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string musicTitle = "—";

        [ObservableProperty]
        private string musicArtist = string.Empty;

        [ObservableProperty]
        private string musicGenre = string.Empty;

        [ObservableProperty]
        private string gameTitle = "—";

        [ObservableProperty]
        private string gameArtist = string.Empty;

        [ObservableProperty]
        private string gameGenre = string.Empty;

        [ObservableProperty]
        private ObservableCollection<EmotionEntry> recentEntries = new();

        // ── Constructor ───────────────────────────────────────────────────────
        public MainWindowViewModel(
            EmotionAnalysisService emotionService,
            DataService dataService,
            AIService aiService,
            ChatAIService chatService,
            AdvancedEmotionAnalyzer emotionAnalyzer,
            AdvancedRecommendationService recommendationService,
            EmotionNotificationService notificationService)
        {
            _emotionService = emotionService;
            _dataService = dataService;
            _aiService = aiService;
            _chatService = chatService;
            _emotionAnalyzer = emotionAnalyzer;
            _recommendationService = recommendationService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync()
        {
            await LoadRecentEntriesAsync();
        }

        // ── Commands ──────────────────────────────────────────────────────────

        [RelayCommand(CanExecute = nameof(CanSend))]
        private async Task SendAsync()
        {
            var prompt = PromptText.Trim();
            if (string.IsNullOrEmpty(prompt)) return;
            if (!SecurityHelper.IsSafeContent(prompt))
            {
                ResponseText = "ข้อความมีเนื้อหาที่ไม่เหมาะสม กรุณาแก้ไขและลองอีกครั้ง";
                return;
            }

            IsBusy = true;
            ResponseText = "กำลังวิเคราะห์อารมณ์ของคุณ...";

            try
            {
                var entry = await _emotionService.AnalyzeEmotionAsync(prompt);
                var (music, game) = await _emotionService.GetRecommendationsAsync(entry.EmotionLabel);

                ResponseText = $"อารมณ์: {entry.EmotionLabel}\n" +
                               $"ระดับความเครียด: {entry.StressLevel}/10\n" +
                               $"ความเป็นบวก: {entry.PositivityScore}/10\n\n" +
                               $"{entry.AIResponse}";

                MusicTitle = music.Title;
                MusicArtist = music.Artist;
                MusicGenre = $"แนว: {music.Genre}";
                GameTitle = game.Title;
                GameArtist = game.Artist;
                GameGenre = $"แนว: {game.Genre}";

                PromptText = string.Empty;
                await LoadRecentEntriesAsync();
            }
            catch (Exception ex)
            {
                ResponseText = string.Empty;
                UXHelper.ShowError($"เกิดข้อผิดพลาด: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSend() => !IsBusy;

        [RelayCommand]
        private void Clear()
        {
            ResponseText = string.Empty;
        }

        [RelayCommand]
        private async Task TrainModelAsync()
        {
            IsBusy = true;
            try
            {
                await _emotionService.TrainSentimentModelAsync();
                UXHelper.ShowMessage("เทรนโมเดลเสร็จสิ้น ระบบจะวิเคราะห์ความรู้สึกได้แม่นยำขึ้น");
            }
            catch (Exception ex)
            {
                UXHelper.ShowError($"เกิดข้อผิดพลาดในการเทรนโมเดล: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Property-change side effects ──────────────────────────────────────

        partial void OnIsBusyChanged(bool value)
        {
            SendCommand.NotifyCanExecuteChanged();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task LoadRecentEntriesAsync()
        {
            var entries = await _emotionService.GetRecentEntriesAsync();
            RecentEntries.Clear();
            foreach (var e in entries)
                RecentEntries.Add(e);
        }

        // ── Expose services for window event handlers ─────────────────────────

        public AIService AIService => _aiService;
        public DataService DataService => _dataService;
        public ChatAIService ChatService => _chatService;
        public AdvancedEmotionAnalyzer EmotionAnalyzer => _emotionAnalyzer;
        public AdvancedRecommendationService RecommendationService => _recommendationService;
        public EmotionNotificationService NotificationService => _notificationService;
        public EmotionAnalysisService EmotionService => _emotionService;
    }
}
