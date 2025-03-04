

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
        public UserSettingsContent(UserViewModel userViewModel)
        {
            InitializeComponent();
            var viewModel = new UserSettingViewModel(userViewModel, (App.Current.MainWindow.DataContext as UserViewModel)!);
            DataContext = viewModel;
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as UserSettingViewModel)!;

            var userDetailViewModel = manageViewModel.UserDetail;
            if(userDetailViewModel.Id == 0)
            {
                userDetailViewModel.Id = SQLite.InserTable(manageViewModel.UserDetail.ToModel());
            }
            else
            {
                SQLite.UpdateTable(manageViewModel.UserDetail.ToModel());
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as UserSettingViewModel)!;

            var userDetailViewModel = manageViewModel.UserDetail;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;

                // 显示缩略图
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                userDetailViewModel.LogoThumbnail = bitmap;
                userDetailViewModel.Logo = filePath;
                userDetailViewModel.IsUpload = true;
            }
        }
    }
}
