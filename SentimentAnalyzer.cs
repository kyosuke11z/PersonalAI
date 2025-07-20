using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับวิเคราะห์ความรู้สึกเบื้องต้นโดยใช้ ML.NET
    /// </summary>
    public class SentimentAnalyzer
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;
        private readonly string _modelPath;
        
        public SentimentAnalyzer()
        {
            _mlContext = new MLContext();
            
            // กำหนดที่เก็บโมเดลในโฟลเดอร์ AppData ของผู้ใช้
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PersonalAICompanion");
                
            // สร้างโฟลเดอร์ถ้ายังไม่มี
            Directory.CreateDirectory(appDataPath);
            
            _modelPath = Path.Combine(appDataPath, "sentiment_model.zip");
            
            // โหลดโมเดลถ้ามีอยู่แล้ว
            if (File.Exists(_modelPath))
            {
                LoadModel();
            }
        }
        
        /// <summary>
        /// โหลดโมเดลที่เคยเทรนไว้แล้ว
        /// </summary>
        private void LoadModel()
        {
            try
            {
                _model = _mlContext.Model.Load(_modelPath, out _);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            }
            catch (Exception)
            {
                _model = null;
                _predictionEngine = null;
            }
        }
        
        /// <summary>
        /// เทรนโมเดลใหม่จากข้อมูลที่มีอยู่
        /// </summary>
        public Task TrainModelAsync(List<EmotionEntry> trainingData)
        {
            if (trainingData.Count < 10)
            {
                // ต้องมีข้อมูลอย่างน้อย 10 รายการ
                return Task.CompletedTask;
            }
            
            // แปลงข้อมูลให้อยู่ในรูปแบบที่เหมาะสม
            var sentimentData = trainingData.Select(e => new SentimentData
            {
                Text = e.Text,
                // กำหนดให้ positivity > 5 เป็นความรู้สึกเชิงบวก
                Sentiment = e.PositivityScore > 5 ? true : false
            }).ToList();
            
            // สร้าง dataset
            var dataView = _mlContext.Data.LoadFromEnumerable(sentimentData);
            
            // แบ่งข้อมูลสำหรับเทรนและทดสอบ
            var dataSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            
            // กำหนด pipeline สำหรับการเทรนโมเดล
            var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "Features", 
                    inputColumnName: nameof(SentimentData.Text))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());
                
            // เทรนโมเดล
            _model = pipeline.Fit(dataSplit.TrainSet);
            
            // บันทึกโมเดล
            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
            
            // สร้าง prediction engine
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// วิเคราะห์ความรู้สึกของข้อความ
        /// </summary>
        public SentimentResult AnalyzeSentiment(string text)
        {
            // ถ้ายังไม่มีโมเดล ให้ใช้การวิเคราะห์อย่างง่าย
            if (_predictionEngine == null)
            {
                return SimpleAnalysis(text);
            }
            
            // ใช้โมเดลทำนาย
            var prediction = _predictionEngine.Predict(new SentimentData { Text = text });
            
            return new SentimentResult
            {
                IsPositive = prediction.Prediction,
                Score = prediction.Probability,
                Text = text
            };
        }
        
        /// <summary>
        /// วิเคราะห์อย่างง่ายโดยนับคำที่มีความหมายเชิงบวกและลบ
        /// </summary>
        private SentimentResult SimpleAnalysis(string text)
        {
            // คำที่มีความหมายเชิงบวก
            var positiveWords = new[] 
            { 
                "ดี", "สุข", "สนุก", "รัก", "ชอบ", "เยี่ยม", "ยอด", "เจ๋ง", "เจ๋ง", "สบาย", 
                "happy", "good", "great", "excellent", "love", "like", "enjoy", "fun", "amazing", "awesome" 
            };
            
            // คำที่มีความหมายเชิงลบ
            var negativeWords = new[] 
            { 
                "แย่", "เศร้า", "เสียใจ", "โกรธ", "เครียด", "กังวล", "กลัว", "เกลียด", "ผิดหวัง", "ท้อแท้",
                "sad", "bad", "angry", "stress", "worry", "fear", "hate", "disappoint", "terrible", "awful" 
            };
            
            // นับคำ
            int positiveCount = positiveWords.Count(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
            int negativeCount = negativeWords.Count(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
            
            // คำนวณคะแนน
            bool isPositive = positiveCount >= negativeCount;
            float score = 0.5f;
            
            if (positiveCount + negativeCount > 0)
            {
                score = (float)positiveCount / (positiveCount + negativeCount);
            }
            
            return new SentimentResult
            {
                IsPositive = isPositive,
                Score = score,
                Text = text
            };
        }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลที่ใช้ในการเทรนโมเดล
    /// </summary>
    public class SentimentData
    {
        [LoadColumn(0)]
        public string Text { get; set; } = string.Empty;
        
        [LoadColumn(1), ColumnName("Label")]
        public bool Sentiment { get; set; }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บผลลัพธ์การทำนาย
    /// </summary>
    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }
        
        public float Probability { get; set; }
        
        public float Score { get; set; }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บผลลัพธ์การวิเคราะห์ความรู้สึก
    /// </summary>
    public class SentimentResult
    {
        public bool IsPositive { get; set; }
        public float Score { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}