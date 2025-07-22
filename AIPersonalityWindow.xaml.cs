using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PersonalAI
{
    public partial class AIPersonalityWindow : Window
    {
        private AIPersonality _personality;

        public AIPersonalityWindow()
        {
            InitializeComponent();
            _personality = AIPersonality.Instance;
            LoadPersonalitySettings();
            
            // ใช้ธีมปัจจุบัน
            this.Background = Application.Current.Resources["PrimaryBackgroundBrush"] as System.Windows.Media.Brush;
        }

        /// <summary>
        /// โหลดการตั้งค่าบุคลิกภาพปัจจุบัน
        /// </summary>
        private void LoadPersonalitySettings()
        {
            // ข้อมูลพื้นฐาน
            NameTextBox.Text = _personality.Name;
            
            switch (_personality.Gender)
            {
                case "ชาย":
                    GenderComboBox.SelectedIndex = 0;
                    break;
                case "หญิง":
                    GenderComboBox.SelectedIndex = 1;
                    break;
                default:
                    GenderComboBox.SelectedIndex = 2;
                    break;
            }
            
            AgeTextBox.Text = _personality.Age.ToString();
            
            // ลักษณะบุคลิกภาพ
            FriendlinessSlider.Value = _personality.Friendliness;
            FormalitySlider.Value = _personality.Formality;
            HumorSlider.Value = _personality.Humor;
            EnthusiasmSlider.Value = _personality.Enthusiasm;
            EmpathySlider.Value = _personality.Empathy;
            
            // คำพูดประจำตัว
            GreetingTextBox.Text = _personality.Greeting;
            FarewellTextBox.Text = _personality.Farewell;
            
            // ความสนใจและความชอบ
            InterestsTextBox.Text = string.Join(", ", _personality.Interests);
            PreferencesTextBox.Text = string.Join(", ", _personality.Preferences);
            
            // อัปเดตค่าตัวเลขของสไลเดอร์
            UpdateSliderValueTexts();
        }

        /// <summary>
        /// อัปเดตค่าตัวเลขของสไลเดอร์
        /// </summary>
        private void UpdateSliderValueTexts()
        {
            FriendlinessValueText.Text = FriendlinessSlider.Value.ToString("0");
            FormalityValueText.Text = FormalitySlider.Value.ToString("0");
            HumorValueText.Text = HumorSlider.Value.ToString("0");
            EnthusiasmValueText.Text = EnthusiasmSlider.Value.ToString("0");
            EmpathyValueText.Text = EmpathySlider.Value.ToString("0");
        }

        /// <summary>
        /// จัดการเหตุการณ์เมื่อค่าสไลเดอร์เปลี่ยน
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                UpdateSliderValueTexts();
            }
        }

        /// <summary>
        /// จัดการเหตุการณ์เมื่อคลิกปุ่มบุคลิกภาพที่กำหนดไว้ล่วงหน้า
        /// </summary>
        private void PredefinedPersonality_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string personalityName = button.Tag.ToString();
                
                var result = MessageBox.Show(
                    $"คุณต้องการใช้บุคลิกภาพ \"{personalityName}\" หรือไม่? การตั้งค่าปัจจุบันจะถูกแทนที่",
                    "ยืนยันการใช้บุคลิกภาพที่กำหนดไว้ล่วงหน้า",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    _personality.ApplyPredefinedPersonality(personalityName);
                    LoadPersonalitySettings();
                    MessageBox.Show($"ใช้บุคลิกภาพ \"{personalityName}\" เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// จัดการเหตุการณ์เมื่อคลิกปุ่มบันทึก
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ข้อมูลพื้นฐาน
                _personality.Name = NameTextBox.Text.Trim();
                
                switch (GenderComboBox.SelectedIndex)
                {
                    case 0:
                        _personality.Gender = "ชาย";
                        break;
                    case 1:
                        _personality.Gender = "หญิง";
                        break;
                    default:
                        _personality.Gender = "ไม่ระบุ";
                        break;
                }
                
                if (int.TryParse(AgeTextBox.Text, out int age))
                {
                    _personality.Age = Math.Max(1, Math.Min(age, 100));
                }
                
                // ลักษณะบุคลิกภาพ
                _personality.Friendliness = (int)FriendlinessSlider.Value;
                _personality.Formality = (int)FormalitySlider.Value;
                _personality.Humor = (int)HumorSlider.Value;
                _personality.Enthusiasm = (int)EnthusiasmSlider.Value;
                _personality.Empathy = (int)EmpathySlider.Value;
                
                // คำพูดประจำตัว
                _personality.Greeting = GreetingTextBox.Text.Trim();
                _personality.Farewell = FarewellTextBox.Text.Trim();
                
                // ความสนใจและความชอบ
                _personality.Interests = InterestsTextBox.Text.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
                
                _personality.Preferences = PreferencesTextBox.Text.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
                
                // บันทึกการตั้งค่า
                _personality.SaveSettings();
                
                MessageBox.Show("บันทึกการตั้งค่าบุคลิกภาพเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// จัดการเหตุการณ์เมื่อคลิกปุ่มยกเลิก
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}