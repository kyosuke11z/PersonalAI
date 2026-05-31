using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonalAI
{
    public partial class ChatWindow : Window
    {
        private readonly ChatAIService _chatService;
        private readonly AdvancedEmotionAnalyzer _emotionAnalyzer;
        private readonly AdvancedRecommendationService _recommendationService;
        private readonly EmotionNotificationService _notificationService;

        private readonly ObservableCollection<ChatDisplayMessage> _chatMessages = new();
        private DetailedEmotionResult? _currentEmotion;
        private CancellationTokenSource? _streamCts;

        public ChatWindow(
            ChatAIService chatService,
            AdvancedEmotionAnalyzer emotionAnalyzer,
            AdvancedRecommendationService recommendationService,
            EmotionNotificationService notificationService)
        {
            InitializeComponent();
            _chatService = chatService;
            _emotionAnalyzer = emotionAnalyzer;
            _recommendationService = recommendationService;
            _notificationService = notificationService;

            ChatMessagesControl.ItemsSource = _chatMessages;

            var personality = AIPersonality.Instance;
            AddAIMessage(personality.Greeting);
            LoadChatHistory();
        }

        private void LoadChatHistory()
        {
            var history = _chatService.GetChatHistory();
            if (history.Count > 0)
                _chatMessages.Clear();

            foreach (var msg in history)
            {
                if (msg.Role == "user") AddUserMessage(msg.Content);
                else AddAIMessage(msg.Content);
            }
            ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            _chatMessages.Add(new ChatDisplayMessage
            {
                Content = message,
                IsUserMessage = true,
                Timestamp = DateTime.Now
            });
            ScrollToBottom();
        }

        private ChatDisplayMessage AddAIMessage(string message)
        {
            var entry = new ChatDisplayMessage
            {
                Content = message,
                IsUserMessage = false,
                Timestamp = DateTime.Now
            };
            _chatMessages.Add(entry);
            ScrollToBottom();
            return entry;
        }

        private void AddEmotionMessage(string emotion, int stressLevel)
        {
            _chatMessages.Add(new ChatDisplayMessage
            {
                EmotionText = $"{emotion} (ความเครียด: {stressLevel}/10)",
                EmotionIcon = GetEmotionIcon(emotion),
                IsEmotion = true,
                Timestamp = DateTime.Now
            });
            ScrollToBottom();
        }

        private static string GetEmotionIcon(string emotion) => emotion.ToLower() switch
        {
            var e when e.Contains("สุข") || e.Contains("ดี") || e.Contains("สนุก") => "😊",
            var e when e.Contains("เศร้า") || e.Contains("เสียใจ") => "😢",
            var e when e.Contains("โกรธ") || e.Contains("หงุดหงิด") => "😠",
            var e when e.Contains("กังวล") || e.Contains("กลัว") || e.Contains("เครียด") => "😰",
            var e when e.Contains("เหนื่อย") || e.Contains("อ่อนล้า") => "😩",
            _ => "😐"
        };

        private void ScrollToBottom() =>
            ChatScrollViewer.Dispatcher.InvokeAsync(() => ChatScrollViewer.ScrollToEnd());

        /// <summary>
        /// ส่งข้อความพร้อม streaming — ตัวอักษรไหลออกมาทีละ chunk แบบ real-time
        /// </summary>
        private async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            if (!SecurityHelper.IsSafeContent(message))
            {
                MessageBox.Show("ข้อความของคุณมีเนื้อหาที่ไม่เหมาะสม", "คำเตือน",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddUserMessage(message);

            try
            {
                // วิเคราะห์อารมณ์ก่อน (non-streaming)
                _currentEmotion = await _emotionAnalyzer.AnalyzeEmotionDetailedAsync(message);
                AddEmotionMessage(_currentEmotion.PrimaryEmotion ?? "ไม่แน่ชัด", _currentEmotion.StressLevel);
                await _notificationService.CheckAndNotifyAsync(_currentEmotion);

                // สร้าง placeholder message สำหรับ streaming
                var streamingEntry = AddAIMessage("...");

                // ยกเลิก stream เก่าถ้ามี
                _streamCts?.Cancel();
                _streamCts = new CancellationTokenSource();
                var token = _streamCts.Token;

                var sb = new StringBuilder();
                bool firstChunk = true;

                await foreach (var chunk in _chatService.SendMessageStreamingAsync(message, token))
                {
                    if (token.IsCancellationRequested) break;

                    sb.Append(chunk);

                    // อัปเดต UI บน Dispatcher thread
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (firstChunk)
                        {
                            streamingEntry.Content = chunk;
                            firstChunk = false;
                        }
                        else
                        {
                            streamingEntry.Content = sb.ToString();
                        }
                        ScrollToBottom();
                    });
                }

                // ถ้าอารมณ์เป็นลบ ให้แนะนำกิจกรรม
                if (_currentEmotion.StressLevel > 7 || _currentEmotion.Positivity < 3)
                {
                    var (music, game) = _recommendationService.GetRecommendations("", _currentEmotion);
                    AddAIMessage(
                        $"ฉันขอแนะนำกิจกรรมที่อาจช่วยให้คุณรู้สึกดีขึ้น:\n\n" +
                        $"🎵 เพลง: {music.Title} โดย {music.Artist}\n" +
                        $"🎮 เกม: {game.Title} โดย {game.Artist}");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                AddAIMessage($"ขออภัย เกิดข้อผิดพลาด: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;
            try
            {
                await SendMessageAsync(MessageTextBox.Text);
                MessageTextBox.Text = string.Empty;
            }
            finally
            {
                SendButton.IsEnabled = true;
                MessageTextBox.Focus();
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                if (SendButton.IsEnabled)
                    SendButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "คุณต้องการล้างประวัติการแชททั้งหมดหรือไม่?",
                "ยืนยันการล้างแชท",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _streamCts?.Cancel();
                _chatService.ClearHistory();
                _chatMessages.Clear();
                var personality = AIPersonality.Instance;
                AddAIMessage("ประวัติการแชทถูกล้างแล้ว " + personality.Greeting);
            }
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e) { }

        protected override void OnClosed(EventArgs e)
        {
            _streamCts?.Cancel();
            base.OnClosed(e);
        }
    }

    public class ChatDisplayMessage : INotifyPropertyChanged
    {
        private string _content = "";

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public DateTime Timestamp { get; set; }
        public bool IsUserMessage { get; set; }
        public bool IsEmotion { get; set; }
        public string EmotionText { get; set; } = "";
        public string EmotionIcon { get; set; } = "😐";

        public string FormattedTime => Timestamp.ToString("HH:mm");
        public Visibility UserMessageVisibility => IsUserMessage && !IsEmotion ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AIMessageVisibility => !IsUserMessage && !IsEmotion ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EmotionVisibility => IsEmotion ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
