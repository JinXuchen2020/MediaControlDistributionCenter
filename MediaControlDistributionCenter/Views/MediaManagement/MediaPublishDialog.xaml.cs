
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// UserChangeGroupDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPublishDialog : UserControl
    {
        private readonly MediaDevicesViewModel manageViewModel;
        private readonly DashboardViewModel dashboardViewModel;

        public MediaPublishDialog(DashboardViewModel dashboardViewModel, MediaDevicesViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            manageViewModel = viewModel;
            this.dashboardViewModel = dashboardViewModel;

            this.Loaded += MediaPublishDialog_Loaded;
        }

        private void MediaPublishDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                await manageViewModel.LoadData();
            });
            //Dispatcher.Invoke(async () =>
            //{
            //    await manageViewModel.DetectConnectedDeviceCommand.ExecuteAsync(null);
            //});
            //if (manageViewModel.ConnectionMode.Mode == "Local")
            //{
            //    var communication = App.ServicesProvider.GetRequiredService<Communication>();
            //    foreach (var device in manageViewModel.Devices)
            //    {
            //        device.ConnectCommand.Execute(communication);
            //    }
            //}
        }

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                await manageViewModel.PublishCommand.ExecuteAsync(null);

                var screenCount = manageViewModel.Devices.Where(c => c.IsSelected).Count();
                manageViewModel.CurrentMedia.ScreensCount = screenCount;
                await manageViewModel.SaveCommand.ExecuteAsync(null);

                if (manageViewModel.PublishDevices.Count > 0)
                {
                    MaterialDesignThemes.Wpf.DialogHost.Close(Constants.DialogHostId);

                    foreach (var device in manageViewModel.PublishDevices)
                    {
                        var model = new PlaybackRecordDto
                        {
                            IsTimerPlay = manageViewModel.IsTimerPlay,
                            NextPlayTime = manageViewModel.NextPlayTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            MediaName = manageViewModel.CurrentMedia.Name,
                            MediaType = manageViewModel.CurrentMedia.Type,
                            MonitorSnCode = device.SNumber,
                            PlaySuccess = true
                        };
                        await device.SendPlayTimeCommand.ExecuteAsync(model);
                        if (!string.IsNullOrEmpty(device.ErrorMessage))
                        {
                            manageViewModel.ErrorMessage = device.ErrorMessage;
                            await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);
                            device.ErrorMessage = null;
                            continue;
                        }

                        Log.Information($"下发命令:{CommunicationCmd.CmdPlayTime}到设备:{device.SNumber}成功");
                    }

                    await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                    if (dashboardViewModel.CurrentUser.Role == "user")
                    {
                        var content = App.ServicesProvider.GetRequiredService<MediaManage>();
                        (App.Current.MainWindow as MainWindow).GoContent(content, 2);
                    }
                    else
                    {
                        var content = App.ServicesProvider.GetRequiredService<UserControllers>();
                        (App.Current.MainWindow as MainWindow).GoContent(content, 2);
                    }
                }
            });
        }

        private void SelectDevicesAll_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox.IsChecked.GetValueOrDefault())
            {
                foreach (var item in manageViewModel.Devices)
                {
                    if (item.IsConnected)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            else
            {
                foreach (var item in manageViewModel.Devices)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void btnPublishCancel_Click(object sender, RoutedEventArgs e)
        {
            if(manageViewModel.IsPublishing)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_214");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            MaterialDesignThemes.Wpf.DialogHost.Close(Constants.DialogHostId);
        }
    }
}
