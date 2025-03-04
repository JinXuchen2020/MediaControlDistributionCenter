using System.Windows;
using System.Windows.Controls;

using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login_old : Window
    {

        public Login_old()
        {
            InitializeComponent();
            DataContext = this;
            var savedPwd = StorageHelper.LoadFromFile();
            if (!string.IsNullOrWhiteSpace(savedPwd))
            {
                UsernameTextBox.Text = savedPwd.Split("+-*/----").FirstOrDefault();
                PasswordTextBox.Text = savedPwd.Split("+-*/----").LastOrDefault();
                RememberPasswordCheckBox.IsChecked = true;
            }
            else
            {
                UsernameTextBox.Text = string.Empty;
                PasswordTextBox.Text = string.Empty;
                RememberPasswordCheckBox.IsChecked = false;
            }
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //登录逻辑
           var username = UsernameTextBox.Text;
            var password = PasswordTextBox.Text;
            var remember = RememberPasswordCheckBox.IsChecked;
            //var language = LanguageComboBox.SelectedItem;
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("请输入用户名");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("请输入密码");
                return;
            }
            if (remember == true)
            {
                // 记住密码
                StorageHelper.SaveToFile($"{username}+-*/----{password}");
            }
            else
            {
                // 不记住密码
            }
            if (ConnectionDetect.IsConnected())
            {
                // 连接成功,在线登录
            }
            else
            {
                // 连接失败，本地登录
            }

            var pwdHash = EncryptionHelper.GetSha256Hash(password);
            // todo 从服务或者数据库中验证用户名和密码

            var savedPwd = StorageHelper.LoadFromFile();
            var validUser = EncryptionHelper.VerifyPassword("1", savedPwd);


            AppRuntime.Instance.UserName = username;
            Console.WriteLine(pwdHash);
            //MainWindow mainWindow = new();
            //mainWindow.ShowDialog();
            this.Hide();
        }

        private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
        {
            LanguagePopup.IsOpen = !LanguagePopup.IsOpen;
            //切换语音示例:后续变成 下拉列表切换  如示例：

            //var language = typeof(MediaControlDistributionCenter.Language).ToOptionList();

            //var str = TipCodeFindRes.GetTipString(503);

            //LanguageTool.Instance.Language = MediaControlDistributionCenter.Language.English;
            //LanguageTool.Instance.ChangeLanguageResource();
        }
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

                LanguageTool.Instance.Language = LanguageTool.Instance.Language;
                LanguageTool.Instance.ChangeLanguageResource();

            }
            LanguagePopup.IsOpen = false;
        }

    }
}
