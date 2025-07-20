using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalAI
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // สร้างบริการพื้นฐาน
            var aiService = new AIService();
            var dataService = new DataService();
            
            // สร้างบริการขั้นสูง
            var advancedEmotionAnalyzer = new AdvancedEmotionAnalyzer(aiService);
            var userPreferences = new UserPreferences();
            var advancedRecommendationService = new AdvancedRecommendationService(aiService, userPreferences, dataService);
            var chatAIService = new ChatAIService(aiService, userPreferences);
            
            // สร้างบริการหลัก
            var recommendationService = new RecommendationService(aiService);
            var emotionAnalysisService = new EmotionAnalysisService(aiService, dataService, recommendationService);
            var emotionNotificationService = new EmotionNotificationService(emotionAnalysisService, advancedEmotionAnalyzer, dataService);
            
            // ลงทะเบียนบริการพื้นฐาน
            services.AddSingleton(aiService);
            services.AddSingleton(dataService);
            services.AddSingleton(userPreferences);
            
            // ลงทะเบียนบริการขั้นสูง
            services.AddSingleton(advancedEmotionAnalyzer);
            services.AddSingleton(advancedRecommendationService);
            services.AddSingleton(chatAIService);
            
            // ลงทะเบียนบริการหลัก
            services.AddSingleton(recommendationService);
            services.AddSingleton(emotionAnalysisService);
            services.AddSingleton(emotionNotificationService);

            // ลงทะเบียนหน้าต่าง
            services.AddSingleton<MainWindow>();
            services.AddTransient<EmotionTrendsWindow>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<ChatWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}