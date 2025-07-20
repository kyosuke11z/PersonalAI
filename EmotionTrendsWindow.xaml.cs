using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

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
                
                UpdatePositivityChart();
                UpdateStressChart();
                UpdateEmotionBarChart();
                UpdateEmotionPieChart();
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
        /// อัปเดตกราฟแนวโน้มความเป็นบวก/ลบ
        /// </summary>
        private void UpdatePositivityChart()
        {
            if (PositivityChart == null) return;
            
            // จัดเรียงข้อมูลตามวันที่
            var sortedData = _emotionData.OrderBy(e => e.Timestamp).ToList();
            
            // ล้างข้อมูลเก่า
            DateLabels.Clear();
            
            // สร้าง PlotModel ใหม่
            var model = CreatePlotModel();
            model.Title = "ความเป็นบวก/ลบ";
            model.TitleColor = OxyColors.White;
            
            // สร้างข้อมูลสำหรับกราฟ
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Blue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.Blue,
                StrokeThickness = 2
            };
            
            // เพิ่มข้อมูลลงในกราฟ
            for (int i = 0; i < sortedData.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, sortedData[i].PositivityScore));
                DateLabels.Add(sortedData[i].Timestamp.ToString("dd/MM"));
            }
            
            // สร้างแกน X และ Y
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "วันที่",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White
            };
            
            foreach (var label in DateLabels)
            {
                xAxis.Labels.Add(label);
            }
            
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "คะแนน",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White,
                Minimum = 0,
                Maximum = 10
            };
            
            // เพิ่มแกนและข้อมูลลงใน PlotModel
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(lineSeries);
            
            // อัปเดตกราฟ
            PositivityChart.Model = model;
            PositivityChart.InvalidatePlot(true);
        }
        
        /// <summary>
        /// อัปเดตกราฟแนวโน้มความเครียด
        /// </summary>
        private void UpdateStressChart()
        {
            if (StressChart == null) return;
            
            // จัดเรียงข้อมูลตามวันที่
            var sortedData = _emotionData.OrderBy(e => e.Timestamp).ToList();
            
            // สร้าง PlotModel ใหม่
            var model = CreatePlotModel();
            model.Title = "ระดับความเครียด";
            model.TitleColor = OxyColors.White;
            
            // สร้างข้อมูลสำหรับกราฟ
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Red,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerStroke = OxyColors.Red,
                MarkerFill = OxyColors.Red,
                StrokeThickness = 2
            };
            
            // เพิ่มข้อมูลลงในกราฟ
            for (int i = 0; i < sortedData.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, sortedData[i].StressLevel));
            }
            
            // สร้างแกน X และ Y
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "วันที่",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White
            };
            
            foreach (var label in DateLabels)
            {
                xAxis.Labels.Add(label);
            }
            
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "คะแนน",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White,
                Minimum = 0,
                Maximum = 10
            };
            
            // เพิ่มแกนและข้อมูลลงใน PlotModel
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(lineSeries);
            
            // อัปเดตกราฟ
            StressChart.Model = model;
            StressChart.InvalidatePlot(true);
        }
        
        /// <summary>
        /// อัปเดตกราฟแท่งแสดงอารมณ์ย่อย
        /// </summary>
        private void UpdateEmotionBarChart()
        {
            if (EmotionBarChart == null) return;
            
            // วิเคราะห์อารมณ์เชิงลึก
            var emotionAnalysis = AnalyzeDetailedEmotions();
            
            // สร้าง PlotModel ใหม่
            var model = CreatePlotModel();
            model.Title = "อารมณ์ย่อย";
            model.TitleColor = OxyColors.White;
            
            // สร้างข้อมูลสำหรับกราฟแท่ง
            var barSeries = new BarSeries
            {
                IsStacked = false,
                StrokeThickness = 1,
                StrokeColor = OxyColors.White
            };
            
            // กำหนดสีสำหรับแต่ละแท่ง
            var colors = new[] {
                OxyColors.Yellow,
                OxyColors.Blue,
                OxyColors.Red,
                OxyColors.Purple,
                OxyColors.Gray
            };
            
            // เพิ่มข้อมูลลงในกราฟ
            barSeries.Items.Add(new BarItem { Value = emotionAnalysis.Happy, Color = colors[0] });
            barSeries.Items.Add(new BarItem { Value = emotionAnalysis.Sad, Color = colors[1] });
            barSeries.Items.Add(new BarItem { Value = emotionAnalysis.Angry, Color = colors[2] });
            barSeries.Items.Add(new BarItem { Value = emotionAnalysis.Anxious, Color = colors[3] });
            barSeries.Items.Add(new BarItem { Value = emotionAnalysis.Neutral, Color = colors[4] });
            
            // สร้างแกน X และ Y
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "คะแนน",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White
            };
            
            var yAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = "อารมณ์",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineColor = OxyColors.White
            };
            
            foreach (var label in EmotionLabels)
            {
                yAxis.Labels.Add(label);
            }
            
            // เพิ่มแกนและข้อมูลลงใน PlotModel
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(barSeries);
            
            // อัปเดตกราฟ
            EmotionBarChart.Model = model;
            EmotionBarChart.InvalidatePlot(true);
        }
        
        /// <summary>
        /// อัปเดตกราฟวงกลมแสดงสัดส่วนอารมณ์
        /// </summary>
        private void UpdateEmotionPieChart()
        {
            if (EmotionPieChart == null) return;
            
            // วิเคราะห์อารมณ์เชิงลึก
            var emotionAnalysis = AnalyzeDetailedEmotions();
            
            // สร้าง PlotModel ใหม่
            var model = CreatePlotModel();
            model.Title = "สัดส่วนอารมณ์";
            model.TitleColor = OxyColors.White;
            
            // สร้างข้อมูลสำหรับกราฟวงกลม
            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelFormat = "",
                OutsideLabelFormat = "{1}: {0:0.0}",
                AngleSpan = 360,
                StartAngle = 0,
                ExplodedDistance = 0.1
            };
            
            // กำหนดสีสำหรับแต่ละส่วน
            var colors = new[] {
                OxyColors.Yellow,
                OxyColors.Blue,
                OxyColors.Red,
                OxyColors.Purple,
                OxyColors.Gray
            };
            
            // เพิ่มข้อมูลลงในกราฟ
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[0], emotionAnalysis.Happy) { Fill = colors[0] });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[1], emotionAnalysis.Sad) { Fill = colors[1] });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[2], emotionAnalysis.Angry) { Fill = colors[2] });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[3], emotionAnalysis.Anxious) { Fill = colors[3] });
            pieSeries.Slices.Add(new PieSlice(EmotionLabels[4], emotionAnalysis.Neutral) { Fill = colors[4] });
            
            // เพิ่มข้อมูลลงใน PlotModel
            model.Series.Add(pieSeries);
            
            // อัปเดตกราฟ
            EmotionPieChart.Model = model;
            EmotionPieChart.InvalidatePlot(true);
        }
    }
}