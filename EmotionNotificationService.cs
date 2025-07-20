using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalAI
{
    /// <summary>
    /// บริการแจ้งเตือนเกี่ยวกับอารมณ์และสุขภาพจิต
    /// </summary>
    public class EmotionNotificationService
    {
        private readonly EmotionAnalysisService _emotionService;
        private readonly AdvancedEmotionAnalyzer _advancedAnalyzer;
        private readonly DataService _dataService;
        
        // ตัวแปรเก็บสถานะการแจ้งเตือน
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private bool _hasHighStressNotified = false;
        private bool _hasLowPositivityNotified = false;
        private bool _hasEmergencyNotified = false;
        
        // ระยะเวลาขั้นต่ำระหว่างการแจ้งเตือน (6 ชั่วโมง)
        private readonly TimeSpan _notificationCooldown = TimeSpan.FromHours(6);
        
        public EmotionNotificationService(
            EmotionAnalysisService emotionService, 
            AdvancedEmotionAnalyzer advancedAnalyzer,
            DataService dataService)
        {
            _emotionService = emotionService;
            _advancedAnalyzer = advancedAnalyzer;
            _dataService = dataService;
        }
        
        /// <summary>
        /// ตรวจสอบและแจ้งเตือนตามอารมณ์ล่าสุด
        /// </summary>
        public async Task CheckAndNotifyAsync(DetailedEmotionResult latestEmotion)
        {
            // ตรวจสอบว่าผ่านระยะเวลาขั้นต่ำหรือยัง
            if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
            {
                return;
            }
            
            // ตรวจสอบกรณีฉุกเฉิน
            if (_advancedAnalyzer.IsEmergencyEmotion(latestEmotion) && !_hasEmergencyNotified)
            {
                ShowEmergencyNotification(latestEmotion);
                _hasEmergencyNotified = true;
                _lastNotificationTime = DateTime.Now;
                return;
            }
            
            // ตรวจสอบแนวโน้มอารมณ์
            var recentEntries = await _dataService.GetRecentEmotionEntriesAsync(14);
            var trend = _advancedAnalyzer.AnalyzeEmotionTrend(recentEntries);
            
            // ตรวจสอบความเครียดสูงต่อเนื่อง
            if (trend.HasHighStress && trend.StressTrend > 0 && !_hasHighStressNotified)
            {
                ShowHighStressNotification();
                _hasHighStressNotified = true;
                _lastNotificationTime = DateTime.Now;
                return;
            }
            
            // ตรวจสอบความเป็นบวกต่ำต่อเนื่อง
            if (trend.HasLowPositivity && trend.PositivityTrend < 0 && !_hasLowPositivityNotified)
            {
                ShowLowPositivityNotification();
                _hasLowPositivityNotified = true;
                _lastNotificationTime = DateTime.Now;
                return;
            }
            
            // รีเซ็ตสถานะการแจ้งเตือนถ้าอารมณ์กลับมาปกติ
            if (!trend.HasHighStress && !trend.HasLowPositivity && trend.IsStable)
            {
                ResetNotificationStatus();
            }
        }
        
        /// <summary>
        /// แสดงการแจ้งเตือนกรณีฉุกเฉิน
        /// </summary>
        private void ShowEmergencyNotification(DetailedEmotionResult emotion)
        {
            string message = "ระบบตรวจพบว่าคุณอาจกำลังประสบปัญหาทางอารมณ์ที่รุนแรง\n\n" +
                            "หากคุณกำลังรู้สึกแย่มากๆ โปรดพิจารณาติดต่อสายด่วนสุขภาพจิต 1323 หรือพูดคุยกับคนที่คุณไว้ใจ\n\n" +
                            "คำแนะนำ: " + _advancedAnalyzer.GetRecommendationBasedOnEmotion(emotion);
            
            MessageBox.Show(
                message,
                "การแจ้งเตือนสุขภาพจิต - สำคัญ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
        
        /// <summary>
        /// แสดงการแจ้งเตือนกรณีความเครียดสูงต่อเนื่อง
        /// </summary>
        private void ShowHighStressNotification()
        {
            string message = "ระบบตรวจพบว่าคุณมีระดับความเครียดสูงต่อเนื่องในช่วงที่ผ่านมา\n\n" +
                            "คำแนะนำ:\n" +
                            "- ลองทำกิจกรรมผ่อนคลาย เช่น การหายใจลึกๆ หรือการทำสมาธิ\n" +
                            "- พักผ่อนให้เพียงพอ\n" +
                            "- ออกกำลังกายเบาๆ\n" +
                            "- พูดคุยกับคนที่คุณไว้ใจ";
            
            MessageBox.Show(
                message,
                "การแจ้งเตือนสุขภาพจิต - ความเครียดสูง",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        
        /// <summary>
        /// แสดงการแจ้งเตือนกรณีความเป็นบวกต่ำต่อเนื่อง
        /// </summary>
        private void ShowLowPositivityNotification()
        {
            string message = "ระบบตรวจพบว่าคุณมีระดับความเป็นบวกต่ำต่อเนื่องในช่วงที่ผ่านมา\n\n" +
                            "คำแนะนำ:\n" +
                            "- ลองทำกิจกรรมที่คุณชอบและทำให้คุณมีความสุข\n" +
                            "- พบปะพูดคุยกับเพื่อนหรือคนที่คุณรัก\n" +
                            "- ออกไปสัมผัสธรรมชาติหรือแสงแดด\n" +
                            "- ฟังเพลงที่ทำให้คุณรู้สึกดี";
            
            MessageBox.Show(
                message,
                "การแจ้งเตือนสุขภาพจิต - ความเป็นบวกต่ำ",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        
        /// <summary>
        /// รีเซ็ตสถานะการแจ้งเตือน
        /// </summary>
        private void ResetNotificationStatus()
        {
            _hasHighStressNotified = false;
            _hasLowPositivityNotified = false;
            _hasEmergencyNotified = false;
        }
        
        /// <summary>
        /// แนะนำกิจกรรมผ่อนคลายตามระดับความเครียด
        /// </summary>
        public List<string> GetRelaxationActivities(int stressLevel)
        {
            var activities = new List<string>();
            
            if (stressLevel >= 8)
            {
                // กิจกรรมสำหรับความเครียดสูงมาก
                activities.Add("การหายใจลึกๆ ช้าๆ เป็นเวลา 5-10 นาที");
                activities.Add("การทำสมาธิแบบง่ายๆ โดยจดจ่อกับลมหายใจ");
                activities.Add("การเดินสั้นๆ ในที่โล่งหรือสวนสาธารณะ");
                activities.Add("การพูดคุยกับคนที่คุณไว้ใจ");
            }
            else if (stressLevel >= 5)
            {
                // กิจกรรมสำหรับความเครียดปานกลาง
                activities.Add("การฟังเพลงที่ผ่อนคลาย");
                activities.Add("การอ่านหนังสือที่คุณชอบ");
                activities.Add("การออกกำลังกายเบาๆ เช่น โยคะหรือการยืดเส้น");
                activities.Add("การดื่มชาอุ่นๆ และพักผ่อน");
            }
            else
            {
                // กิจกรรมสำหรับความเครียดต่ำ
                activities.Add("การทำงานอดิเรกที่คุณชอบ");
                activities.Add("การดูภาพยนตร์หรือซีรีส์ที่สนุก");
                activities.Add("การเล่นเกมที่คุณชอบ");
                activities.Add("การพบปะเพื่อนฝูง");
            }
            
            return activities;
        }
    }
}