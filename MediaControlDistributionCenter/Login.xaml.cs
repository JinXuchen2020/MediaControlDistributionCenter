using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using SqlSugar;

namespace MediaControlDistributionCenter
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        private LoginViewModel viewModel;

        private DispatcherTimer _timer;

        private readonly IServiceProvider serviceProvider;
        public Login(LoginViewModel viewModel, IServiceProvider serviceProvider)
        {
            this.viewModel = viewModel;
            this.serviceProvider = serviceProvider;
            InitializeComponent();
            DataContext = viewModel;

            switch (LanguageTool.Instance.Language)
            {
                case MediaControlDistributionCenter.Language.Chinese:
                    SysLanguage.Text = FindResource("LanguageKey_Code_ZH").ToString();
                    Log.Debug("zh_CN");
                    break;
                case MediaControlDistributionCenter.Language.English:
                    SysLanguage.Text = FindResource("LanguageKey_Code_EN").ToString();
                    Log.Debug("en_US");
                    break;
                case MediaControlDistributionCenter.Language.Japanese:
                    SysLanguage.Text = FindResource("LanguageKey_Code_JP").ToString();
                    Log.Debug("ja_JP");
                    break;
                case MediaControlDistributionCenter.Language.Korean:
                    SysLanguage.Text = FindResource("LanguageKey_Code_KO").ToString();
                    Log.Debug("ko_KR");
                    break;
            }

            InitializeTimer();

            this.Loaded += Login_Loaded;
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        private void NetworkChange_NetworkAddressChanged(object? sender, EventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                await viewModel.SendBroadcastMessageCommand.ExecuteAsync(null);
            });
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                await viewModel.DetectInternetDevicesCommand.ExecuteAsync(null);
            });
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string loginId = txtLoginId.Text;
            string password = passwordBox.Password;

            Log.Debug($"Login attempt with ID: {loginId}");

            viewModel.LoginCommand.ExecuteAsync(new AccountDto { Account = loginId, Password = password }).GetAwaiter().OnCompleted(() =>
            {
                if (viewModel.IsLogin)
                {
                    Log.Information($"User {loginId} logged in successfully.");

                    App.Current.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
                    App.Current.MainWindow.Show();
                    this.Hide();
                }
            });
        }


        private void LanguageSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem selectedItem)
            {
                string selectedLanguage = selectedItem.Name.ToString()!;
                switch (selectedLanguage)
                {
                    case "zh_CN":
                        //ChangeLanguage("zh-CN");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Chinese;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_ZH").ToString();
                        Log.Debug("zh_CN");
                        break;
                    case "en_US":
                        //ChangeLanguage("en-US");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_EN").ToString();
                        Log.Debug("en_US");
                        break;
                    case "ja_JP":
                        //ChangeLanguage("ja-JP");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Japanese;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_JP").ToString();
                        Log.Debug("ja_JP");
                        break;
                    case "ko_KR":
                        //ChangeLanguage("ko-KR");
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Korean;
                        LanguageTool.Instance.ChangeLanguageResource();
                        SysLanguage.Text = FindResource("LanguageKey_Code_KO").ToString();
                        Log.Debug("ko_KR");
                        break;
                }

                viewModel.TranslateLanguageProperties();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ConnectionModeChanged_Click(object sender, RoutedEventArgs e)
        {
            if(btnLocal.IsChecked == true)
            {
                viewModel.ConnectionMode.Mode = "Local";
                spAddress.Visibility = Visibility.Visible;
                spSlogan.Visibility = Visibility.Visible;
                viewModel.RefreshService();
                Dispatcher.Invoke(async () =>
                {
                    await viewModel.DetectConnectedDevice();
                });
            }
            else
            {
                viewModel.ConnectionMode.Mode = "Remote";
                spAddress.Visibility = Visibility.Collapsed;
                spSlogan.Visibility = Visibility.Collapsed;
                viewModel.RefreshService();
            }
        }

        //private void btnConnect_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Dispatcher.Invoke(async () =>
        //    {
        //        if (!viewModel.IsSyncing)
        //        {
        //            await viewModel.ConnectCommand.ExecuteAsync(null);
        //        }
        //    });
        //}

        private void InitializeTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (viewModel.ConnectionMode.Mode == "Local")
            {
                Dispatcher.Invoke(async () =>
                {
                    await viewModel.DetectConnectedDevice();
                });
            }
        }
    }
}
