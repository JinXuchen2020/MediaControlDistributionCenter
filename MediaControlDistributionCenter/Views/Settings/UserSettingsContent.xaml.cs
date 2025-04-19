

using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenCvSharp.Dnn;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Serilog;
using System.Windows;

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

            if(userSettingViewModel.IsShelf)
            {
                userSettingViewModel.ShowNavigation = true;
                userSettingViewModel.CurrentUser = dashboardViewModel.CurrentUser;
                userSettingViewModel.CurrentUser.Groups = userManageViewModel.Groups;
                userSettingViewModel.IsShelf = dashboardViewModel.CurrentUser.Role != RoleType.Admin.ToString().ToLower();
            }
            else
            {
                userSettingViewModel.ShowNavigation = false;
                userSettingViewModel.CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
                userSettingViewModel.CurrentUser.Groups = userManageViewModel.Groups;
                userSettingViewModel.IsShelf = userSettingViewModel.IsShelf || dashboardViewModel.CurrentUser.Role == RoleType.Agent.ToString().ToLower();
            }

            manageViewModel = userSettingViewModel;
            manageViewModel.PageType = manageViewModel.CurrentUser.Role == RoleType.Admin.ToString().ToLower() ? "internet" : "user";
            manageViewModel.LoadData();
            DataContext = userSettingViewModel;
            this.Unloaded += UserSettingsContent_Unloaded;
        }

        private void UserSettingsContent_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            BitmapImage? bitmap = manageViewModel.CurrentUser.LogoThumbnail;
            if (bitmap != null)
            {
                bitmap.UriSource = new Uri("about:blank");
                bitmap.DecodePixelHeight = 0;
                bitmap.DecodePixelWidth = 0;
                manageViewModel.CurrentUser.LogoThumbnail = null;
                manageViewModel.CurrentUser.Logo = null;
            }

            manageViewModel.StopDetectCommand.Execute(null);
            manageViewModel.DetectStatus = null;
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.SaveUserCommand.Execute(null);
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
            if (manageViewModel.IsShelf)
            {
                return;
            }
            var viewModel = manageViewModel.CurrentUser;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(filePath);
                this.Dispatcher.Invoke(async () =>
                {
                    var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                    await ftpClient.UploadFileToFtpServer(filePath, $"{viewModel.Account}{extension}");

                    // 显示缩略图
                    BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                    viewModel.LogoThumbnail = bitmap;
                    viewModel.Logo = filePath;
                    viewModel.LogoFileName = $"{manageViewModel.CurrentUser.Account}{extension}";
                    viewModel.IsUpload = true;
                });
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = ((sender as Border).Tag as string)!;
            manageViewModel.PageType = tag;
        }

        private void btnDetect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            manageViewModel.DetectInternetDevicesCommand.Execute(null);
        }

        private void btnConnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Dispatcher.Invoke(async() =>
            {
                var device = (sender as Button).DataContext as InternetDevice;
                var communication = App.ServicesProvider.GetRequiredService<Communication>();
                if (!string.IsNullOrEmpty(device.IpAddress) && device.IpAddress != communication.IpAddr)
                {
                    communication.Connect(device.IpAddress, "5001");
                    int count = 5;
                    while (communication.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
                    {
                        await Task.Delay(500);
                        count--;
                    }

                    if (communication.netClient.State != Helpers.SocketClient.SocketState.Connected)
                    {
                        manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Device_Tooltip_100");// MessageBox.Show("无法连接机顶盒!");
                        await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        Log.Debug($"Device with IP {device.IpAddress} is connected!");
                        device.Status = 1;
                        device.StatusText = manageViewModel.GetStatus(1);
                    }
                }
            });
        }

        private void btnStopDetect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            manageViewModel.StopDetectCommand.Execute(null);
        }
    }
}
