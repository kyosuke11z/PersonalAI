using System;
using System.Windows;
using System.Windows.Controls;

namespace PersonalAI
{
    public partial class ThemeSettingsWindow : Window
    {
        private ThemeType _selectedTheme;

        public ThemeSettingsWindow()
        {
            InitializeComponent();
            LoadCurrentTheme();
            
            // ปรับสีตามธีมที่เลือก
            UpdateThemePreview();
        }

        private void LoadCurrentTheme()
        {
            _selectedTheme = ThemeManager.Instance.CurrentTheme;
            
            switch (_selectedTheme)
            {
                case ThemeType.Light:
                    LightThemeRadio.IsChecked = true;
                    break;
                case ThemeType.Blue:
                    BlueThemeRadio.IsChecked = true;
                    break;
                case ThemeType.Dark:
                default:
                    DarkThemeRadio.IsChecked = true;
                    break;
            }
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag != null)
            {
                if (Enum.TryParse<ThemeType>(radioButton.Tag.ToString(), out var theme))
                {
                    _selectedTheme = theme;
                    UpdateThemePreview();
                }
            }
        }
        
        private void UpdateThemePreview()
        {
            // กำหนดสีพื้นหลังและสีข้อความตามธีมที่เลือก
            switch (_selectedTheme)
            {
                case ThemeType.Light:
                    // กำหนดสีพื้นหลังเป็นสีสว่าง
                    this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 247)); // #F5F5F7
                    
                    // กำหนดสีข้อความหัวข้อเป็นสีน้ำเงินม่วง
                    HeaderText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(78, 79, 235)); // #4E4FEB
                    SubHeaderText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)); // #000000
                    
                    // กำหนดสีข้อความของ RadioButton
                    DarkThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                    LightThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                    BlueThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                    
                    // กำหนดสีข้อความของปุ่ม
                    ApplyButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    CloseButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    break;
                    
                case ThemeType.Blue:
                    // กำหนดสีพื้นหลังเป็นสีฟ้าเข้ม
                    this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(10, 25, 41)); // #0A1929
                    
                    // กำหนดสีข้อความหัวข้อเป็นสีฟ้าอ่อน
                    HeaderText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 178, 255)); // #66B2FF
                    SubHeaderText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    
                    // กำหนดสีข้อความของ RadioButton
                    DarkThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    LightThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    BlueThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    
                    // กำหนดสีข้อความของปุ่ม
                    ApplyButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    CloseButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    break;
                    
                case ThemeType.Dark:
                default:
                    // กำหนดสีพื้นหลังเป็นสีดำ
                    this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 27, 34)); // #1A1B22
                    
                    // กำหนดสีข้อความหัวข้อเป็นสีน้ำเงินม่วง
                    HeaderText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(78, 79, 235)); // #4E4FEB
                    SubHeaderText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    
                    // กำหนดสีข้อความของ RadioButton
                    DarkThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    LightThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    BlueThemeRadio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    
                    // กำหนดสีข้อความของปุ่ม
                    ApplyButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    CloseButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    break;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.CurrentTheme = _selectedTheme;
            MessageBox.Show("ธีมถูกเปลี่ยนเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}