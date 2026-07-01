using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.FTP;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.LocalImps;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFwTypeLib;
using Serilog;
using Syncfusion.Licensing;
using System.IO;
using System.Windows;

namespace MediaControlDistributionCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServicesProvider { get; private set; }

        private static Mutex _mutex = null;

        //public static MainWindow MainWindow;
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;

            _mutex = new Mutex(true, "Media Control", out createdNew);

            if (!createdNew)
            {
                // 如果应用程序已经在运行，则退出
                MessageBox.Show("Application is running!");
                Current.Shutdown();
                return;
            }
            // 1. 创建并加载配置文件
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // 设置配置文件的基础路径
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // 加载 appsettings.json 配置文件
                .Build();

            var services = new ServiceCollection();

            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXxdcXRRRGBYVUR2XkRWYUA=");

            var connectionMode = new ConnectionMode();
            configuration.Bind("ConnectionMode", connectionMode);
            connectionMode.Mode = "Remote";
            services.AddSingleton(connectionMode);

            var ftpConnection = new FtpConnection();
            configuration.Bind("FtpConnection", ftpConnection);
            services.AddSingleton(ftpConnection);

            var skiaCanvasConfig = new SkiaCanvasConfig();
            configuration.Bind("SkiaCanvas", skiaCanvasConfig);
            services.AddSingleton(skiaCanvasConfig);

            services.AddSingleton<FtpServer>();
            services.AddSingleton<FtpClient>();
            services.AddSingleton<Communication>();
            services.AddKeyedSingleton<IDetectService, DetectServiceLocal>("Local");
            services.AddKeyedSingleton<IDetectService, DetectService>("Remote");
            services.AddKeyedSingleton<IDeviceInteractService, DeviceInteractServiceLocal>("Local");
            services.AddKeyedSingleton<IDeviceInteractService, DeviceInteractService>("Remote");
            services.AddSingleton<IConnectService, ConnectService>();

            services.AddLocalServices();
            services.AddRemoteServices();
            //if (connectionMode.Mode == "Local")
            //{
            //    services.AddLocalServices();
            //}
            //else
            //{
            //    services.AddRemoteServices();
            //}

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

            // 3. 启动主窗口
            var mainWindow = ServicesProvider.GetRequiredService<Login>();
            mainWindow.Show();

            // 4. 启动时记录日志
            Log.Information("Application started.");
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            INetFwMgr netFwMgr = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));
            INetFwAuthorizedApplication app = (INetFwAuthorizedApplication)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication"));
            app.Name = nameof(MediaControlDistributionCenter);
            app.ProcessImageFileName = Environment.ProcessPath;
            app.Enabled = true;
            netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
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
            UpdateLanguageResourceCache(langRd);

        }

        private IDictionary<string, string> GetLanguageResourceCache()
        {
            IDictionary<string, string> languageResourceCache = new Dictionary<string, string>();
            var languageResource = base.Resources.MergedDictionaries.FirstOrDefault(rItem => rItem.Contains("LanguageKey_LanguageResourceKey"));
            if (languageResource == null) return languageResourceCache;
            foreach (string key in languageResource.Keys)
            {
                var languageKey = key.ToString();
                var languageValue = languageResource[key].ToString();
                if (!string.IsNullOrEmpty(languageValue) && !languageResourceCache.ContainsKey(languageValue)
                    && languageKey.Contains("LanguageKey_Code_"))
                    languageResourceCache.Add(languageValue, languageKey);
            }
            return languageResourceCache;
        }

        private void UpdateLanguageResourceCache(ResourceDictionary? langRd)
        {
            IDictionary<string, string> languageResourceCache = LanguageTool.Instance.LanguageResourceCache;
            var languageResource = langRd;
            if (languageResource == null) return;
            foreach (string key in languageResource.Keys)
            {
                var languageKey = key.ToString();
                var languageValue = languageResource[key].ToString();
                if (!string.IsNullOrEmpty(languageValue) && !languageResourceCache.ContainsKey(languageValue)
                   && languageKey.Contains("LanguageKey_Code_"))
                    languageResourceCache.Add(languageValue, languageKey);
            }
        }

        #endregion
    }

}
