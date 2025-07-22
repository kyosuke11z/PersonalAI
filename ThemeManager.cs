using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Text.Json;

namespace PersonalAI
{
    public enum ThemeType
    {
        Dark,
        Light,
        Blue
    }

    public class ThemeManager
    {
        // สร้าง Light Theme โดยตรงแทนการโหลดจากไฟล์ XAML
        private ResourceDictionary CreateLightTheme()
        {
            ResourceDictionary dict = new ResourceDictionary();
            
            // Colors
            dict.Add("PrimaryBackgroundColor", System.Windows.Media.Color.FromRgb(235, 237, 240));
            dict.Add("SecondaryBackgroundColor", System.Windows.Media.Colors.White);
            dict.Add("AccentColor", System.Windows.Media.Color.FromRgb(25, 118, 210));
            dict.Add("SecondaryAccentColor", System.Windows.Media.Color.FromRgb(211, 47, 47));
            dict.Add("TextPrimaryColor", System.Windows.Media.Color.FromRgb(38, 50, 56));
            dict.Add("TextSecondaryColor", System.Windows.Media.Color.FromRgb(84, 110, 122));
            dict.Add("BorderColor", System.Windows.Media.Color.FromRgb(207, 216, 220));
            dict.Add("ShadowColor", System.Windows.Media.Color.FromRgb(176, 190, 197));
            
            // Brushes
            dict.Add("PrimaryBackgroundBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["PrimaryBackgroundColor"]));
            dict.Add("SecondaryBackgroundBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["SecondaryBackgroundColor"]));
            dict.Add("AccentBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["AccentColor"]));
            dict.Add("SecondaryAccentBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["SecondaryAccentColor"]));
            dict.Add("TextPrimaryBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["TextPrimaryColor"]));
            dict.Add("TextSecondaryBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["TextSecondaryColor"]));
            dict.Add("BorderBrush", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["BorderColor"]));
            
            // GlassBackgroundBrush
            System.Windows.Media.SolidColorBrush glassBackgroundBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)dict["SecondaryBackgroundColor"]);
            glassBackgroundBrush.Opacity = 0.95;
            dict.Add("GlassBackgroundBrush", glassBackgroundBrush);
            
            return dict;
        }
        private static ThemeManager _instance;
        private ThemeType _currentTheme = ThemeType.Dark;
        private readonly string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersonalAI", "settings.json");

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        public ThemeType CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme();
                    SaveSettings();
                }
            }
        }

        private ThemeManager()
        {
            LoadSettings();
        }

        public void ApplyTheme()
        {
            var app = Application.Current;
            
            // ลบเฉพาะธีมเก่าออก แทนที่จะล้างทั้งหมด
            for (int i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dict = app.Resources.MergedDictionaries[i];
                if (dict.Source != null && 
                    (dict.Source.ToString().Contains("/Themes/DarkTheme.xaml") ||
                     dict.Source.ToString().Contains("/Themes/LightTheme.xaml") ||
                     dict.Source.ToString().Contains("/Themes/BlueTheme.xaml")))
                {
                    app.Resources.MergedDictionaries.RemoveAt(i);
                }
            }

            // เพิ่มธีมใหม่
            ResourceDictionary themeDictionary = null;
            
            try
            {
                switch (_currentTheme)
                {
                    case ThemeType.Light:
                        // สร้าง ResourceDictionary โดยตรงแทนการใช้ Source
                        themeDictionary = CreateLightTheme();
                        break;
                    case ThemeType.Blue:
                        themeDictionary = new ResourceDictionary();
                        themeDictionary.Source = new Uri("pack://application:,,,/Themes/BlueTheme.xaml", UriKind.Absolute);
                        break;
                    case ThemeType.Dark:
                    default:
                        themeDictionary = new ResourceDictionary();
                        themeDictionary.Source = new Uri("pack://application:,,,/Themes/DarkTheme.xaml", UriKind.Absolute);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading theme: {ex.Message}");
                // ใช้ธีมเริ่มต้นหากมีข้อผิดพลาด
                themeDictionary = new ResourceDictionary();
                themeDictionary.Source = new Uri("pack://application:,,,/Themes/DarkTheme.xaml", UriKind.Absolute);
            }

            if (themeDictionary != null)
            {
                app.Resources.MergedDictionaries.Add(themeDictionary);
            }
            
            // อัพเดทธีมในหน้าต่างที่เปิดอยู่
            foreach (Window window in Application.Current.Windows)
            {
                if (window.IsLoaded)
                {
                    try
                    {
                        // อัพเดทสีของหน้าต่าง
                        window.Background = app.Resources["PrimaryBackgroundBrush"] as System.Windows.Media.Brush;
                        
                        // อัพเดท ResourceDictionary ของหน้าต่าง
                        if (window.Resources.MergedDictionaries.Count > 0)
                        {
                            // ลบ ResourceDictionary เก่าออก
                            for (int i = window.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
                            {
                                var dict = window.Resources.MergedDictionaries[i];
                                if (dict.Source != null && 
                                    (dict.Source.ToString().Contains("/Themes/DarkTheme.xaml") ||
                                     dict.Source.ToString().Contains("/Themes/LightTheme.xaml") ||
                                     dict.Source.ToString().Contains("/Themes/BlueTheme.xaml")))
                                {
                                    window.Resources.MergedDictionaries.RemoveAt(i);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating theme for window: {ex.Message}");
                    }
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (settings != null && settings.TryGetValue("Theme", out var themeValue))
                    {
                        if (Enum.TryParse<ThemeType>(themeValue.ToString(), out var theme))
                        {
                            _currentTheme = theme;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ใช้ธีมเริ่มต้นหากมีข้อผิดพลาด
                _currentTheme = ThemeType.Dark;
                Console.WriteLine($"Error loading theme settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                
                var settings = new Dictionary<string, object>
                {
                    { "Theme", _currentTheme.ToString() }
                };
                
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving theme settings: {ex.Message}");
            }
        }
    }
}