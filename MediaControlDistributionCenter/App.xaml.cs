using System.IO;
using System.Windows;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SqlSugar;

namespace MediaControlDistributionCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServicesProvider { get; private set; }

        //public static MainWindow MainWindow;
        protected override void OnStartup(StartupEventArgs e)
        {
            // 1. 创建并加载配置文件
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // 设置配置文件的基础路径
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // 加载 appsettings.json 配置文件
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddLocalServices();
            services.AddPageViewServices();
            services.AddPageViewModelServices();

            ServicesProvider = services.BuildServiceProvider();

            // 2. 配置 Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)  // 从配置文件中读取配置
                .CreateLogger();

            LanguageTool.Instance.ChangeLanguageResourceHandle += ChangeLanguageResource;
            LanguageTool.Instance.FindResourceHandle += SICPFindResource;
            LanguageTool.Instance.InitLanguageResourceCache(GetLanguageResourceCache());

            SQLite.InitServer();
            SQLite.InitTables();

            FtpServer server = new FtpServer();
            server.FtpServerStart();

            // 3. 启动主窗口
            var mainWindow = ServicesProvider.GetRequiredService<Login>();
            mainWindow.Show();

            // 4. 启动时记录日志
            Log.Information("Application started.");
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if(e.ExceptionObject is Exception ex)
            {
                HandleException(ex);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void HandleException(Exception ex)
        {
            string message = $"An error occured: {ex.Message}";
            //MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Error(message);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 5. 在应用程序退出时关闭并刷新日志
            Log.CloseAndFlush();
            base.OnExit(e);
        }


        #region 语言服务



        private object SICPFindResource(object key)
        {
            if (key == null) return null;
            return Current.TryFindResource(key);
        }

        private void ChangeLanguageResource(string languageDictionaryUriPath)
        {
            ResourceDictionary langRd = Application.LoadComponent(new Uri(languageDictionaryUriPath, UriKind.Relative)) as ResourceDictionary;
            if (base.Resources.MergedDictionaries.Count > 0)
            {
                var old = base.Resources.MergedDictionaries.FirstOrDefault(rItem => rItem.Contains("LanuageKey_LanguageResourceKey"));
                if (old != null)
                    base.Resources.MergedDictionaries.Remove(old);
            }
            base.Resources.MergedDictionaries.Add(langRd);

        }

        private IDictionary<string, string> GetLanguageResourceCache()
        {
            IDictionary<string, string> languageResourceCache = new Dictionary<string, string>();
            var languageResource = base.Resources.MergedDictionaries.FirstOrDefault(rItem => rItem.Contains("LanguageKey_LanguageResourceKey"));
            if (languageResource == null) return languageResourceCache;
            foreach (var key in languageResource.Keys)
            {
                var languageKey = key.ToString();
                var languageValue = languageResource[key].ToString();
                if (!languageResourceCache.ContainsKey(languageValue)
                    && languageKey.Contains("LanguageKey_Code_"))
                    languageResourceCache.Add(languageValue, languageKey);
            }
            return languageResourceCache;
        }

        #endregion
    }

}
