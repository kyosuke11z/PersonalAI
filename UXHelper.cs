using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PersonalAI
{
    public static class UXHelper
    {
        // แสดงข้อความแจ้งเตือนแบบเรียบง่าย
        public static void ShowMessage(string message)
        {
            MessageBox.Show(message, "My Personal AI Companion", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // แสดงข้อความแจ้งเตือนข้อผิดพลาด
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // แสดงข้อความยืนยัน
        public static bool Confirm(string message)
        {
            return MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        // สร้างแอนิเมชันสำหรับ Control
        public static void AnimateFadeIn(UIElement element, double duration = 0.3)
        {
            element.Opacity = 0;
            element.Visibility = Visibility.Visible;

            DoubleAnimation animation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(duration)
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        // สร้างแอนิเมชันสำหรับการเปลี่ยนสี
        public static void AnimateColorChange(Control control, Color targetColor, double duration = 0.5)
        {
            if (control.Background is not SolidColorBrush currentBrush)
                return;

            Color startColor = currentBrush.Color;
            
            ColorAnimation animation = new()
            {
                From = startColor,
                To = targetColor,
                Duration = TimeSpan.FromSeconds(duration)
            };

            SolidColorBrush brush = new(startColor);
            control.Background = brush;
            
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        // ตรวจสอบการเชื่อมต่ออินเทอร์เน็ต
        public static bool IsInternetConnected()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = client.GetAsync("http://www.google.com").Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // แปลงข้อความอารมณ์เป็นสี
        public static Color GetColorForEmotion(string emotion)
        {
            return emotion.ToLower() switch
            {
                "happy" => Colors.Yellow,
                "excited" => Colors.Orange,
                "relaxed" => Colors.LightBlue,
                "sad" => Colors.LightBlue,
                "angry" => Colors.Red,
                "anxious" => Colors.Purple,
                _ => Colors.Gray
            };
        }

        // ตัดข้อความให้สั้นลงถ้ายาวเกินไป
        public static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}