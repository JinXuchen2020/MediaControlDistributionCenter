using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MediaControlDistributionCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mainViewModel;

        private readonly IServiceProvider serviceProvider;

        public MainWindow(MainViewModel mainViewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            this.mainViewModel = mainViewModel;
            Log.Information("MainWindow initialized.");
            //userManage.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            DataContext = mainViewModel;

            MainContentControl.Content = serviceProvider.GetRequiredService<Dashboard>();
            //menus = new List<MenuItem>()
            //{
            //    new MenuItem
            //    {
            //        Text = "LanguageKey_Code_Dashboard",
            //        Action = TopMenu_MouseDown
            //    }
            //}

        }
        private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
        {
            //切换语音示例:后续变成 下拉列表切换  如示例：

            var language = typeof(Language).ToOptionList();

            var str = TipCodeFindRes.GetTipString(503);

            LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
            LanguageTool.Instance.ChangeLanguageResource();
        }


        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            //LanguagePopup.IsOpen = !LanguagePopup.IsOpen;
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
            }
        }

        private void CheckButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (sender is CheckButton checkButton)
            //{
            //    foreach (var sibling in ((Panel)checkButton.Parent).Children)
            //    {
            //        if (sibling is CheckButton siblingCheckButton && siblingCheckButton != checkButton)
            //        {
            //            siblingCheckButton.IsChecked = false;
            //        }
            //    }


            //    switch (checkButton.Content)
            //    {
            //        case "用户管理":
            //            borderMiddle.Visibility = Visibility.Collapsed;
            //            borderMiddleAdd.Visibility = Visibility.Visible;
            //            borderMiddleAdd.Child = new UserManage();
            //            break;
            //        case "设定":
            //            borderMiddle.Visibility = Visibility.Collapsed;
            //            borderMiddleAdd.Visibility = Visibility.Visible;
            //            borderMiddleAdd.Child = new Setting();
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            // 关闭主界面显示登录界面
        }

        private void CheckButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckButton checkButton)
            {
                if (checkButton.IsChecked == false)
                {
                    return;
                }
            }
        }

        private void UserManagementTab_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void TopMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;
            var eleName = text.Name;

            switch (eleName)
            {
                case "Dashboard":
                    GoCotent(serviceProvider.GetRequiredService<Dashboard>(), 1);
                    break;
                case "UserManagement":
                    GoCotent(serviceProvider.GetRequiredService<UserManage>(), 2);
                    break;
                case "MediaManagement":
                    GoCotent(serviceProvider.GetRequiredService<MediaManage>(), 2);
                    break;
                case "DeviceManagement":
                    GoCotent(serviceProvider.GetRequiredService<DeviceManage>(), 3);
                    break;
                case "MediaStore":
                    GoCotent(serviceProvider.GetRequiredService<MediaContent>(), 3);
                    break;
                case "DeviceControl":
                    GoCotent(serviceProvider.GetRequiredService<DeviceControlContent>(), 4);
                    break;
                case "Settings":
                    GoCotent(serviceProvider.GetRequiredService<UserSettingsContent>(), 5);
                    break;
            }
        }

        public void GoCotent(UserControl contet,int menuIndex)
        {
            MainContentControl.Content = contet;
            var userRole = mainViewModel.CurrentUser.Role;
            if(userRole == "user")
            {
                switch (menuIndex)
                {
                    case 1:
                        Dashboard.Opacity = 1;
                        MediaManagement.Opacity = 0.7;
                        DeviceManagement.Opacity = 0.7;
                        DeviceControl.Opacity = 0.7;
                        Settings.Opacity = 0.7;
                        break;
                    case 2:
                        Dashboard.Opacity = 0.7;
                        MediaManagement.Opacity = 1;
                        DeviceManagement.Opacity = 0.7;
                        DeviceControl.Opacity = 0.7;
                        Settings.Opacity = 0.7;
                        break;
                    case 3:
                        Dashboard.Opacity = 0.7;
                        MediaManagement.Opacity = 0.7;
                        DeviceManagement.Opacity = 1;
                        DeviceControl.Opacity = 0.7;
                        Settings.Opacity = 0.7;
                        break;
                    case 4:
                        Dashboard.Opacity = 0.7;
                        MediaManagement.Opacity = 0.7;
                        DeviceManagement.Opacity = 0.7;
                        DeviceControl.Opacity = 1;
                        Settings.Opacity = 0.7;
                        break;
                    case 5:
                        Dashboard.Opacity = 0.7;
                        MediaManagement.Opacity = 0.7;
                        DeviceManagement.Opacity = 0.7;
                        DeviceControl.Opacity = 0.7;
                        Settings.Opacity = 1;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (menuIndex)
                {
                    case 1:
                        Dashboard.Opacity = 1;
                        UserManagement.Opacity = 0.7;
                        MediaStore.Opacity = 0.7;
                        break;
                    case 2:
                        Dashboard.Opacity = 0.7;
                        UserManagement.Opacity = 1;
                        MediaStore.Opacity = 0.7;
                        break;
                    case 3:
                        Dashboard.Opacity = 0.7;
                        UserManagement.Opacity = 0.7;
                        MediaStore.Opacity = 1;
                        break;
                    default:
                        break;
                }
            }
            
        }
    }
}
