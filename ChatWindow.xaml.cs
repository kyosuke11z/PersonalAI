using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonalAI
{
    /// <summary>
    /// หน้าต่างแชทกับ AI ส่วนตัว
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly ChatAIService _chatService;
        private readonly AdvancedEmotionAnalyzer _emotionAnalyzer;
        private readonly AdvancedRecommendationService _recommendationService;
        private readonly EmotionNotificationService _notificationService;
        
        // คอลเลกชันสำหรับเก็บข้อความแชท
        private ObservableCollection<ChatDisplayMessage> _chatMessages = new();
        
        // อารมณ์ปัจจุบันของผู้ใช้
        private DetailedEmotionResult? _currentEmotion;
        
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
            
            // กำหนดแหล่งข้อมูลสำหรับ ItemsControl
            ChatMessagesControl.ItemsSource = _chatMessages;
            
            // เพิ่มข้อความต้อนรับจาก AI
            AddAIMessage("สวัสดีครับ ฉันเป็น AI ส่วนตัวของคุณ คุณสามารถพูดคุยกับฉันได้เกี่ยวกับความรู้สึกหรือสิ่งที่คุณกำลังประสบอยู่ ฉันพร้อมรับฟังและให้คำแนะนำครับ");
            
            // โหลดประวัติการแชทจากบริการ
            LoadChatHistory();
        }
        
        /// <summary>
        /// โหลดประวัติการแชทจากบริการ
        /// </summary>
        private void LoadChatHistory()
        {
            var history = _chatService.GetChatHistory();
            
            // ถ้ามีประวัติการแชท ให้ล้างข้อความต้อนรับออกก่อน
            if (history.Count > 0)
            {
                _chatMessages.Clear();
            }
            
            // เพิ่มข้อความจากประวัติ
            foreach (var message in history)
            {
                if (message.Role == "user")
                {
                    AddUserMessage(message.Content);
                }
                else if (message.Role == "assistant")
                {
                    AddAIMessage(message.Content);
                }
            }
            
            // เลื่อนไปที่ข้อความล่าสุด
            ScrollToBottom();
        }
        
        /// <summary>
        /// เพิ่มข้อความของผู้ใช้ลงในแชท
        /// </summary>
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
        
        /// <summary>
        /// เพิ่มข้อความของ AI ลงในแชท
        /// </summary>
        private void AddAIMessage(string message)
        {
            _chatMessages.Add(new ChatDisplayMessage
            {
                Content = message,
                IsUserMessage = false,
                Timestamp = DateTime.Now
            });
            
            ScrollToBottom();
        }
        
        /// <summary>
        /// เพิ่มข้อความแสดงอารมณ์ที่ตรวจจับได้
        /// </summary>
        private void AddEmotionMessage(string emotion, int stressLevel)
        {
            var emotionIcon = GetEmotionIcon(emotion);
            
            _chatMessages.Add(new ChatDisplayMessage
            {
                EmotionText = $"{emotion} (ความเครียด: {stressLevel}/10)",
                EmotionIcon = emotionIcon,
                IsEmotion = true,
                Timestamp = DateTime.Now
            });
            
            ScrollToBottom();
        }
        
        /// <summary>
        /// รับไอคอนที่เหมาะสมกับอารมณ์
        /// </summary>
        private string GetEmotionIcon(string emotion)
        {
            return emotion.ToLower() switch
            {
                var e when e.Contains("สุข") || e.Contains("ดี") || e.Contains("สนุก") => "😊",
                var e when e.Contains("เศร้า") || e.Contains("เสียใจ") => "😢",
                var e when e.Contains("โกรธ") || e.Contains("หงุดหงิด") => "😠",
                var e when e.Contains("กังวล") || e.Contains("กลัว") || e.Contains("เครียด") => "😰",
                var e when e.Contains("เหนื่อย") || e.Contains("อ่อนล้า") => "😩",
                _ => "😐"
            };
        }
        
        /// <summary>
        /// เลื่อนไปที่ข้อความล่าสุด
        /// </summary>
        private void ScrollToBottom()
        {
            ChatScrollViewer.Dispatcher.InvokeAsync(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            });
        }
        
        /// <summary>
        /// ส่งข้อความไปยัง AI และรับการตอบกลับ
        /// </summary>
        private async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
                
            // ตรวจสอบความปลอดภัยของข้อความ
            if (!SecurityHelper.IsSafeContent(message))
            {
                MessageBox.Show(
                    "ข้อความของคุณมีเนื้อหาที่ไม่เหมาะสม กรุณาแก้ไขและลองอีกครั้ง",
                    "คำเตือน",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            
            // เพิ่มข้อความของผู้ใช้
            AddUserMessage(message);
            
            try
            {
                // วิเคราะห์อารมณ์จากข้อความ
                _currentEmotion = await _emotionAnalyzer.AnalyzeEmotionDetailedAsync(message);
                
                // เพิ่มข้อความแสดงอารมณ์
                AddEmotionMessage(_currentEmotion.PrimaryEmotion ?? "ไม่แน่ชัด", _currentEmotion.StressLevel);
                
                // ตรวจสอบและแจ้งเตือนตามอารมณ์
                await _notificationService.CheckAndNotifyAsync(_currentEmotion);
                
                // ส่งข้อความไปยัง AI และรับการตอบกลับ
                var response = await _chatService.SendMessageAsync(message);
                
                // เพิ่มการตอบกลับของ AI
                AddAIMessage(response);
                
                // ถ้าอารมณ์เป็นลบ ให้แนะนำกิจกรรม
                if (_currentEmotion.StressLevel > 7 || _currentEmotion.Positivity < 3)
                {
                    var (music, game) = _recommendationService.GetRecommendations("", _currentEmotion);
                    
                    string recommendationMessage = $"ฉันขอแนะนำกิจกรรมที่อาจช่วยให้คุณรู้สึกดีขึ้น:\n\n" +
                                                 $"🎵 เพลง: {music.Title} โดย {music.Artist}\n" +
                                                 $"🎮 เกม: {game.Title} โดย {game.Artist}";
                    
                    // เพิ่มคำแนะนำกิจกรรม
                    AddAIMessage(recommendationMessage);
                }
            }
            catch (Exception ex)
            {
                AddAIMessage($"ขออภัย เกิดข้อผิดพลาด: {ex.Message}");
            }
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่มส่งข้อความ
        /// </summary>
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
        
        /// <summary>
        /// จัดการเหตุการณ์กดปุ่ม Enter ในช่องข้อความ
        /// </summary>
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                
                if (SendButton.IsEnabled)
                {
                    // เรียกใช้ฟังก์ชันส่งข้อความโดยตรง
                    SendButton_Click(sender, new RoutedEventArgs());
                }
            }
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่มล้างแชท
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "คุณต้องการล้างประวัติการแชททั้งหมดหรือไม่?",
                "ยืนยันการล้างแชท",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            
            if (result == MessageBoxResult.Yes)
            {
                _chatService.ClearHistory();
                _chatMessages.Clear();
                
                // เพิ่มข้อความต้อนรับใหม่
                AddAIMessage("ประวัติการแชทถูกล้างแล้ว คุณสามารถเริ่มการสนทนาใหม่ได้เลยครับ");
            }
        }

        // เหตุการณ์เมื่อข้อความในช่องข้อความเปลี่ยนแปลง
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // ไม่มีการดำเนินการใดๆ ในขณะนี้
        }
    }

    /// <summary>
    /// คลาสสำหรับแสดงข้อความในแชท
    /// </summary>
    public class ChatDisplayMessage : INotifyPropertyChanged
    {
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool IsUserMessage { get; set; }
        public bool IsEmotion { get; set; }
        public string EmotionText { get; set; } = "";
        public string EmotionIcon { get; set; } = "😐";
        
        // คุณสมบัติสำหรับการแสดงผล
        public string FormattedTime => Timestamp.ToString("HH:mm");
        
        public Visibility UserMessageVisibility => IsUserMessage && !IsEmotion ? Visibility.Visible : Visibility.Collapsed;
        
        public Visibility AIMessageVisibility => !IsUserMessage && !IsEmotion ? Visibility.Visible : Visibility.Collapsed;
        
        public Visibility EmotionVisibility => IsEmotion ? Visibility.Visible : Visibility.Collapsed;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}