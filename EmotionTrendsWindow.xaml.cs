using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using OxyPlot.Annotations;

namespace PersonalAI
{
    public partial class EmotionTrendsWindow : Window
    {
        private readonly EmotionAnalysisService _emotionService;
        private readonly DataService _dataService;
        private List<EmotionEntry> _emotionData = new();
        private readonly List<string> DateLabels = new();
        private readonly List<string> EmotionLabels = new() { "มีความสุข", "เศร้า", "โกรธ", "กังวล", "เฉยๆ" };
        
        public EmotionTrendsWindow(EmotionAnalysisService emotionService, DataService dataService)
        {
            InitializeComponent();
            _emotionService = emotionService;
            _dataService = dataService;
            
            // ตั้งค่าธีมสีสำหรับกราฟ
            SetupChartTheme();
            
            // โหลดข้อมูลเมื่อเปิดหน้าต่าง
            Loaded += async (s, e) => await LoadDataAndUpdateChartsAsync(7); // เริ่มต้นด้วย 7 วัน
        }
        
        /// <summary>
        /// โหลดข้อมูลและอัปเดตกราฟ
        /// </summary>
        private async Task LoadDataAndUpdateChartsAsync(int days)
        {
            try
            {
                // โหลดข้อมูลตามจำนวนวันที่เลือก
                await LoadDataAsync(days);
                
                // อัปเดตกราฟ
                UpdateCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่ม 7 วัน
        /// </summary>
        private async void WeekButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAndUpdateChartsAsync(7);
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่ม 30 วัน
        /// </summary>
        private async void MonthButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAndUpdateChartsAsync(30);
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่มรายละเอียด
        /// </summary>
        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            // แสดงรายละเอียดเพิ่มเติม (ยังไม่ได้ทำ)
            MessageBox.Show("ฟีเจอร์นี้กำลังอยู่ในระหว่างการพัฒนา", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่มปิด
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// จัดการเหตุการณ์คลิกปุ่มกลับ
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// วิเคราะห์อารมณ์เชิงลึก
        /// </summary>
        private EmotionScores AnalyzeDetailedEmotions()
        {
            var result = new EmotionScores();
            
            if (_emotionData.Count == 0)
                return result;
            
            // นับจำนวนอารมณ์แต่ละประเภท
            foreach (var entry in _emotionData)
            {
                switch (entry.EmotionLabel.ToLower())
                {
                    case var e when e.Contains("สุข") || e.Contains("ดี") || e.Contains("สนุก"):
                        result.Happy += 1;
                        break;
                    case var e when e.Contains("เศร้า") || e.Contains("เสียใจ") || e.Contains("ผิดหวัง"):
                        result.Sad += 1;
                        break;
                    case var e when e.Contains("โกรธ") || e.Contains("หงุดหงิด") || e.Contains("รำคาญ"):
                        result.Angry += 1;
                        break;
                    case var e when e.Contains("กังวล") || e.Contains("กลัว") || e.Contains("เครียด"):
                        result.Anxious += 1;
                        break;
                    case var s when s.Contains("เหนื่อย") || s.Contains("อ่อนล้า"):
                        result.Anxious += 1;
                        break;
                    default:
                        result.Neutral += 1;
                        break;
                }
            }
            
            // คำนวณค่าเฉลี่ยความเครียดและความเป็นบวก
            double totalEntries = _emotionData.Count;
            result.Happy = Math.Round(result.Happy / totalEntries * 10, 1);
            result.Sad = Math.Round(result.Sad / totalEntries * 10, 1);
            result.Angry = Math.Round(result.Angry / totalEntries * 10, 1);
            result.Anxious = Math.Round(result.Anxious / totalEntries * 10, 1);
            result.Neutral = Math.Round(result.Neutral / totalEntries * 10, 1);
            
            return result;
        }
        
        /// <summary>
        /// ตั้งค่าธีมสีสำหรับกราฟ
        /// </summary>
        private void SetupChartTheme()
        {
            // สร้าง PlotModel สำหรับแต่ละกราฟ
            if (PositivityChart != null) PositivityChart.Model = CreatePlotModel();
            if (StressChart != null) StressChart.Model = CreatePlotModel();
            if (EmotionBarChart != null) EmotionBarChart.Model = CreatePlotModel();
            if (EmotionPieChart != null) EmotionPieChart.Model = CreatePlotModel();
        }
        
        /// <summary>
        /// สร้าง PlotModel พื้นฐานสำหรับกราฟ
        /// </summary>
        private PlotModel CreatePlotModel()
        {
            try
            {
                var plotModel = new PlotModel
                {
                    PlotAreaBorderColor = OxyColors.Gray,
                    TextColor = OxyColors.White,
                    PlotAreaBackground = OxyColors.Black,
                    Background = OxyColors.Black
                };
                
                return plotModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการสร้างกราฟ: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                return new PlotModel();
            }
        }
        
        /// <summary>
        /// โหลดข้อมูลอารมณ์ตามจำนวนวันที่กำหนด
        /// </summary>
        private async Task LoadDataAsync(int days)
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-days);
            
            _emotionData = await _dataService.GetEmotionEntriesByDateRangeAsync(startDate, endDate);
        }
        
        /// <summary>
        /// อัปเดตกราฟทั้งหมด
        /// </summary>
        private void UpdateCharts()
        {
            try
            {
                if (_emotionData.Count == 0)
                {
                    ShowNoDataMessage();
                    return;
                }
                
                // ใช้ Task.Run เพื่อสร้างกราฟแบบ asynchronous
                Task.Run(() => {
                    // สร้างกราฟใน background thread
                    var positivityModel = CreatePositivityChartModel();
                    var stressModel = CreateStressChartModel();
                    var barModel = CreateEmotionBarChartModel();
                    var pieModel = CreateEmotionPieChartModel();
                    
                    // อัปเดต UI ใน UI thread
                    Dispatcher.Invoke(() => {
                        if (PositivityChart != null) {
                            PositivityChart.Model = positivityModel;
                            PositivityChart.InvalidatePlot(true);
                        }
                        
                        if (StressChart != null) {
                            StressChart.Model = stressModel;
                            StressChart.InvalidatePlot(true);
                        }
                        
                        if (EmotionBarChart != null) {
                            EmotionBarChart.Model = barModel;
                            EmotionBarChart.InvalidatePlot(true);
                        }
                        
                        if (EmotionPieChart != null) {
                            EmotionPieChart.Model = pieModel;
                            EmotionPieChart.InvalidatePlot(true);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการอัปเดตกราฟ: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// แสดงข้อความเมื่อไม่มีข้อมูล
        /// </summary>
        private void ShowNoDataMessage()
        {
            // ล้างข้อมูลกราฟ
            DateLabels.Clear();
            
            // แสดงข้อความไม่มีข้อมูลในกราฟ
            var plots = new List<OxyPlot.Wpf.PlotView>();
            if (PositivityChart != null) plots.Add(PositivityChart);
            if (StressChart != null) plots.Add(StressChart);
            if (EmotionBarChart != null) plots.Add(EmotionBarChart);
            if (EmotionPieChart != null) plots.Add(EmotionPieChart);
            
            foreach (var plot in plots)
            {
                var model = new PlotModel
                {
                    Title = "ไม่มีข้อมูล",
                    TitleColor = OxyColors.White,
                    PlotAreaBorderColor = OxyColors.Gray,
                    TextColor = OxyColors.White,
                    PlotAreaBackground = OxyColors.Black,
                    Background = OxyColors.Black
                };
                
                plot.Model = model;
                plot.InvalidatePlot(true);
            }
        }
        
        /// <summary>
        /// สร้างโมเดลกราฟความเป็นบวก/ลบ
        /// </summary>
        private PlotModel CreatePositivityChartModel()
        {
            // สร้างโมเดลกราฟพื้นฐาน
            var model = CreatePlotModel();
            model.Title = "แนวโน้มความเป็นบวก/ลบ";
            model.TitleColor = OxyColors.White;
            
            // จัดเรียงข้อมูลตามวันที่
            var sortedData = _emotionData.OrderBy(e => e.Timestamp).ToList();
            
            // สร้างแกน X
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "วันที่",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
            };
            model.Axes.Add(dateAxis);
            
            // สร้างแกน Y
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 10,
                Title = "ระดับความเป็นบวก",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
            };
            model.Axes.Add(valueAxis);
            
            // สร้างชุดข้อมูล
            var positivitySeries = new LineSeries
            {
                Title = "ความเป็นบวก",
                Color = OxyColor.FromRgb(78, 79, 235), // สีฟ้า
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColor.FromRgb(78, 79, 235),
                MarkerFill = OxyColor.FromRgb(78, 79, 235),
                StrokeThickness = 2
            };
            
            // เพิ่มข้อมูลลงในชุดข้อมูล
            foreach (var entry in sortedData)
            {
                positivitySeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(entry.Timestamp), entry.PositivityScore));
            }
            
            model.Series.Add(positivitySeries);
            
            return model;
        }
        
        /// <summary>
        /// สร้างโมเดลกราฟความเครียด
        /// </summary>
        private PlotModel CreateStressChartModel()
        {
            // สร้างโมเดลกราฟพื้นฐาน
            var model = CreatePlotModel();
            model.Title = "ระดับความเครียด";
            model.TitleColor = OxyColors.White;
            
            // จัดเรียงข้อมูลตามวันที่
            var sortedData = _emotionData.OrderBy(e => e.Timestamp).ToList();
            
            // สร้างแกน X
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "วันที่",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
            };
            model.Axes.Add(dateAxis);
            
            // สร้างแกน Y
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 10,
                Title = "ระดับความเครียด",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
            };
            model.Axes.Add(valueAxis);
            
            // สร้างชุดข้อมูล
            var stressSeries = new LineSeries
            {
                Title = "ความเครียด",
                Color = OxyColor.FromRgb(255, 77, 106), // สีชมพู
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColor.FromRgb(255, 77, 106),
                MarkerFill = OxyColor.FromRgb(255, 77, 106),
                StrokeThickness = 2
            };
            
            // เพิ่มข้อมูลลงในชุดข้อมูล
            foreach (var entry in sortedData)
            {
                stressSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(entry.Timestamp), entry.StressLevel));
            }
            
            model.Series.Add(stressSeries);
            
            return model;
        }
        
        /// <summary>
        /// สร้างโมเดลกราฟแท่งอารมณ์ย่อย
        /// </summary>
        private PlotModel CreateEmotionBarChartModel()
        {
            // สร้างโมเดลกราฟพื้นฐาน
            var model = CreatePlotModel();
            model.Title = "อารมณ์ย่อย";
            model.TitleColor = OxyColors.White;
            
            // วิเคราะห์อารมณ์เชิงลึก
            var emotionScores = AnalyzeDetailedEmotions();
            
            // สร้างแกน X
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "อารมณ์",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                ItemsSource = EmotionLabels
            };
            model.Axes.Add(categoryAxis);
            
            // สร้างแกน Y
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 10,
                Title = "ระดับ",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
            };
            model.Axes.Add(valueAxis);
            
            // สร้างชุดข้อมูล
            var barSeries = new BarSeries
            {
                Title = "ระดับอารมณ์",
                FillColor = OxyColor.FromRgb(78, 79, 235),
                StrokeColor = OxyColor.FromRgb(78, 79, 235),
                StrokeThickness = 1
            };
            
            // เพิ่มข้อมูลลงในชุดข้อมูล
            barSeries.Items.Add(new BarItem { Value = emotionScores.Happy });
            barSeries.Items.Add(new BarItem { Value = emotionScores.Sad });
            barSeries.Items.Add(new BarItem { Value = emotionScores.Angry });
            barSeries.Items.Add(new BarItem { Value = emotionScores.Anxious });
            barSeries.Items.Add(new BarItem { Value = emotionScores.Neutral });
            
            model.Series.Add(barSeries);
            
            return model;
        }
        
        /// <summary>
        /// สร้างโมเดลกราฟวงกลมสัดส่วนอารมณ์
        /// </summary>
        private PlotModel CreateEmotionPieChartModel()
        {
            // สร้างโมเดลกราฟพื้นฐาน
            var model = CreatePlotModel();
            model.Title = "สัดส่วนอารมณ์";
            model.TitleColor = OxyColors.White;
            
            // วิเคราะห์อารมณ์เชิงลึก
            var emotionScores = AnalyzeDetailedEmotions();
            
            // สร้างชุดข้อมูล
            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelFormat = "",
                OutsideLabelFormat = "{1}: {0:0.0}",
                AngleSpan = 360,
                StartAngle = 0
            };
            
            // เพิ่มข้อมูลลงในชุดข้อมูล
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[0], emotionScores.Happy) { Fill = OxyColor.FromRgb(78, 79, 235) });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[1], emotionScores.Sad) { Fill = OxyColor.FromRgb(255, 77, 106) });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[2], emotionScores.Angry) { Fill = OxyColor.FromRgb(255, 152, 0) });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[3], emotionScores.Anxious) { Fill = OxyColor.FromRgb(0, 188, 212) });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[4], emotionScores.Neutral) { Fill = OxyColor.FromRgb(158, 158, 158) });
            
            model.Series.Add(pieSeries);
            
            return model;
        }
        

    }
}