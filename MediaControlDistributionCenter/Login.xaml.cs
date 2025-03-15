using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private readonly IServiceProvider serviceProvider;
        public Login(LoginViewModel viewModel, IServiceProvider serviceProvider)
        {
            this.viewModel = viewModel;
            this.serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string loginId = txtLoginId.Text;
            string password = passwordBox.Password;

            Log.Debug($"Login attempt with ID: {loginId}");

            if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Login ID and password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            viewModel.LoginCommand.ExecuteAsync(new AccountDto { Account = loginId, Password = password }).GetAwaiter().OnCompleted(() =>
            {
                if (viewModel.IsLogin)
                {
                    Log.Information($"User {loginId} logged in successfully.");

                    App.Current.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
                    App.Current.MainWindow.Show();
                    this.Hide();
                }
                else
                {
                    Log.Warning($"Failed login attempt for user {loginId}.");
                    MessageBox.Show("Invalid login ID or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });            

            //var resultResponse = authService.Login(new AccountDto { Account = loginId, Password = password}).GetAwaiter().GetResult();
            //if (resultResponse.Code == 200)
            //{
            //    Log.Information($"User {loginId} logged in successfully.");
            //    //MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            //    var loginUser = (UserDto)JsonConvert.DeserializeObject(resultResponse.Data!)!;

            //    var userViewModel = new UserViewModel(loginUser);

            //    App.Current.MainWindow = new MainWindow(userViewModel);
            //    App.Current.MainWindow.Show();
            //    this.Hide();
            //}
            //else
            //{
            //    Log.Warning($"Failed login attempt for user {loginId}.");
            //    MessageBox.Show("Invalid login ID or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        //private User? ValidateCredentials(string loginId, string password)
        //{
        //    // Replace with actual validation logic, e.g., checking against a database 
        //    //if (!SQLite.CheckTableExists("Users"))
        //    //{
        //    //    SQLite.CreateTable<User>();
        //    //    SQLite.InserTable<User>(new User() { Account = "admin", Password = "123456", Name = "管理员", Role = "admin" });
        //    //}
        //    var result = userService.GetUser(string loginId, string password) SQLite.QueryTable<User>().Where(d => d.Account == loginId && password == "123456").First();
        //    return result;
        //}


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
                viewModel.RefreshService();
            }
            else
            {
                viewModel.ConnectionMode.Mode = "Remote";
                viewModel.RefreshService();
            }
        }
    }
}
