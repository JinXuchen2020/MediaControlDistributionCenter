

using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// 界面 的交互逻辑
    /// </summary>
    public partial class UserSettingsContent : UserControl
    {
        private readonly UserSettingViewModel manageViewModel;

        public UserSettingsContent(UserSettingViewModel userSettingViewModel)
        {
            InitializeComponent();
            manageViewModel = userSettingViewModel;
            DataContext = userSettingViewModel;
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.SaveUserCommand.Execute(null);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;

                // 显示缩略图
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                manageViewModel.CurrentUser.LogoThumbnail = bitmap;
                manageViewModel.CurrentUser.Logo = filePath;
                manageViewModel.CurrentUser.IsUpload = true;
            }
        }

        private void btnChangePassword_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.ChangePasswordCommand.Execute(null);
        }

        private void btnCancelPassword_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.CancelChangePasswordCommand.Execute(null);
        }
    }
}
