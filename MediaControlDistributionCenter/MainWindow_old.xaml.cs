using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Views;
using MediaControlDistributionCenter.Views.CustomControls;

using Serilog;

namespace MediaControlDistributionCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow_old : Window
    {
        public MainWindow_old()
        {
            InitializeComponent();
            Log.Information("MainWindow initialized.");
            userManage.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        }
        private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
        {
            //切换语音示例:后续变成 下拉列表切换  如示例：

            var language = typeof(MediaControlDistributionCenter.Language).ToOptionList();

            var str = TipCodeFindRes.GetTipString(503);

            LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
            LanguageTool.Instance.ChangeLanguageResource();
        }


        private void LanguageButton_Click(object sender, RoutedEventArgs e) => LanguagePopup.IsOpen = !LanguagePopup.IsOpen;

        private void ChangeLanguage(object sender, RoutedEventArgs e)
        {
            // 切换到韩语的逻辑
            if (sender is Button button)
            {
                switch (button.Name)
                {
                    case "KoreanButton":
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Korean;
                        break;
                    case "JapaneseButton":
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Japanese;
                        break;
                    case "EnglishButton":
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
                        break;
                    case "ChineseButton":
                        LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.Chinese;
                        break;
                    default:
                        break;
                }

                //切换语音示例:后续变成 下拉列表切换  如示例：

                var language = typeof(MediaControlDistributionCenter.Language).ToOptionList();

                var str = TipCodeFindRes.GetTipString(503);

                //LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
                LanguageTool.Instance.ChangeLanguageResource();
            }
            LanguagePopup.IsOpen = false;
        }

        private void CheckButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckButton checkButton)
            {
                foreach (var sibling in ((Panel)checkButton.Parent).Children)
                {
                    if (sibling is CheckButton siblingCheckButton && siblingCheckButton != checkButton)
                    {
                        siblingCheckButton.IsChecked = false;
                    }
                }


                switch (checkButton.Content)
                {
                    case "用户管理":
                        borderMiddle.Visibility = Visibility.Collapsed;
                        borderMiddleAdd.Visibility = Visibility.Visible;
                        borderMiddleAdd.Child = new UserManage();
                        break;
                    case "设定":
                        borderMiddle.Visibility = Visibility.Collapsed;
                        borderMiddleAdd.Visibility = Visibility.Visible;
                        borderMiddleAdd.Child = new Setting();
                        break;
                    default:
                        break;
                }
            }
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
    }
}