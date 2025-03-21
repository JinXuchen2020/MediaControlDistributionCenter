

using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
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

        public UserSettingsContent(UserSettingViewModel userSettingViewModel, DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel)
        {
            InitializeComponent();

            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                userSettingViewModel.CurrentUser = dashboardViewModel.CurrentUser;
                userSettingViewModel.CurrentUser.Groups = userManageViewModel.Groups;
                userSettingViewModel.LoginUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                userSettingViewModel.LoginUser = dashboardViewModel.CurrentUser;
                userSettingViewModel.CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
                userSettingViewModel.CurrentUser.Groups = userManageViewModel.Groups;
            }
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
            if (manageViewModel.OldPassword != manageViewModel.CurrentUser.Password)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Setting_Tooltip_100");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (manageViewModel.NewPassword == null || manageViewModel.NewPasswordConfirm == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Setting_Tooltip_101");
                 manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (manageViewModel.NewPassword != manageViewModel.NewPasswordConfirm)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Setting_Tooltip_102");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    await manageViewModel.ChangePasswordCommand.ExecuteAsync(null);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnCancelPassword_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.CancelChangePasswordCommand.Execute(null);
        }

        private void btnReset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.ResetUserCommand.Execute(null);
        }

        private void btnUpload_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ((sender as Border).DataContext as UserViewModel)!;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;

                // 显示缩略图
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                viewModel.LogoThumbnail = bitmap;
                viewModel.Logo = filePath;
                viewModel.LogoFileName = System.IO.Path.GetFileName(filePath);
                viewModel.IsUpload = true;
            }
        }
    }
}
