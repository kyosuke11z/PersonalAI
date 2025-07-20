using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace PersonalAI
{
    /// <summary>
    /// หน้าต่างสำหรับตั้งค่านิสัยส่วนตัวของผู้ใช้
    /// </summary>
    public partial class UserPreferencesWindow : Window
    {
        private UserPreferences _userPreferences;
        private readonly string _preferencesPath;
        
        // สร้าง ObservableCollection สำหรับ Binding
        private ObservableCollection<PreferenceItem> _activityPreferences = new ObservableCollection<PreferenceItem>();
        private ObservableCollection<PreferenceItem> _environmentPreferences = new ObservableCollection<PreferenceItem>();
        private ObservableCollection<HealthItem> _healthConditions = new ObservableCollection<HealthItem>();
        
        public UserPreferencesWindow()
        {
            InitializeComponent();
            
            // กำหนดที่เก็บไฟล์การตั้งค่า
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalAICompanion");
                
            // สร้างโฟลเดอร์ถ้ายังไม่มี
            Directory.CreateDirectory(appDataPath);
            
            _preferencesPath = Path.Combine(appDataPath, "user_preferences.json");
            
            // โหลดการตั้งค่า
            _userPreferences = UserPreferences.LoadFromFile(_preferencesPath);
            
            // แสดงข้อมูลในหน้าต่าง
            LoadPreferences();
        }
        
        /// <summary>
        /// โหลดการตั้งค่าจากไฟล์และแสดงในหน้าต่าง
        /// </summary>
        private void LoadPreferences()
        {
            // ข้อมูลพื้นฐาน
            UserNameTextBox.Text = _userPreferences.UserName;
            AgeTextBox.Text = _userPreferences.Age.ToString();
            
            switch (_userPreferences.Gender.ToLower())
            {
                case "ชาย":
                    GenderComboBox.SelectedIndex = 0;
                    break;
                case "หญิง":
                    GenderComboBox.SelectedIndex = 1;
                    break;
                case "อื่นๆ":
                    GenderComboBox.SelectedIndex = 3;
                    break;
                default:
                    GenderComboBox.SelectedIndex = 2; // ไม่ระบุ
                    break;
            }
            
            // สร้าง ObservableCollection จาก Dictionary
            _activityPreferences = new ObservableCollection<PreferenceItem>();
            foreach (var item in _userPreferences.ActivityPreferences)
            {
                _activityPreferences.Add(new PreferenceItem(item.Key, item.Value));
            }
            
            _environmentPreferences = new ObservableCollection<PreferenceItem>();
            foreach (var item in _userPreferences.EnvironmentPreferences)
            {
                _environmentPreferences.Add(new PreferenceItem(item.Key, item.Value));
            }
            
            _healthConditions = new ObservableCollection<HealthItem>();
            foreach (var item in _userPreferences.HealthConditions)
            {
                _healthConditions.Add(new HealthItem(item.Key, item.Value));
            }
            
            // ความชอบ/ไม่ชอบกิจกรรม
            ActivityPreferencesControl.ItemsSource = _activityPreferences;
            
            // ความชอบ/ไม่ชอบสภาพแวดล้อม
            EnvironmentPreferencesControl.ItemsSource = _environmentPreferences;
            
            // ข้อมูลสุขภาพ
            HealthConditionsControl.ItemsSource = _healthConditions;
            
            // ข้อมูลเพิ่มเติม
            AllergiesTextBox.Text = string.Join(", ", _userPreferences.Allergies);
            CustomPreferencesTextBox.Text = string.Join(", ", _userPreferences.CustomPreferences);
            CustomDislikesTextBox.Text = string.Join(", ", _userPreferences.CustomDislikes);
        }
        
        /// <summary>
        /// บันทึกการตั้งค่า
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ข้อมูลพื้นฐาน
                _userPreferences.UserName = UserNameTextBox.Text.Trim();
                
                if (int.TryParse(AgeTextBox.Text, out int age))
                {
                    _userPreferences.Age = age;
                }
                
                if (GenderComboBox.SelectedItem is ComboBoxItem selectedGender)
                {
                    _userPreferences.Gender = selectedGender.Content.ToString() ?? "ไม่ระบุ";
                }
                
                // อัปเดต Dictionary จาก ObservableCollection
                UpdatePreferencesFromUI();
                
                // ข้อมูลเพิ่มเติม
                _userPreferences.Allergies = AllergiesTextBox.Text
                    .Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                    
                _userPreferences.CustomPreferences = CustomPreferencesTextBox.Text
                    .Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                    
                _userPreferences.CustomDislikes = CustomDislikesTextBox.Text
                    .Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                
                // บันทึกลงไฟล์
                _userPreferences.SaveToFile(_preferencesPath);
                
                MessageBox.Show("บันทึกการตั้งค่าเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึกการตั้งค่า: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// อัปเดต Dictionary จากค่าที่แสดงใน UI
        /// </summary>
        private void UpdatePreferencesFromUI()
        {
            // อัปเดตความชอบกิจกรรม
            _userPreferences.ActivityPreferences.Clear();
            foreach (var item in _activityPreferences)
            {
                _userPreferences.ActivityPreferences[item.Key] = item.Value;
            }
            
            // อัปเดตความชอบสภาพแวดล้อม
            _userPreferences.EnvironmentPreferences.Clear();
            foreach (var item in _environmentPreferences)
            {
                _userPreferences.EnvironmentPreferences[item.Key] = item.Value;
            }
            
            // อัปเดตข้อมูลสุขภาพ
            _userPreferences.HealthConditions.Clear();
            foreach (var item in _healthConditions)
            {
                _userPreferences.HealthConditions[item.Key] = item.Value;
            }
        }
        
        /// <summary>
        /// ยกเลิกการตั้งค่า
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}