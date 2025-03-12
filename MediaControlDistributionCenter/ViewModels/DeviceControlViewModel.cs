using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using MediaControlDistributionCenter.Data.Entity;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using MediaControlDistributionCenter.Views.UserManagement;
using MediaControlDistributionCenter.Services;
using System.Windows;
using MediaControlDistributionCenter.Services.DTO.Models;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using MediaControlDistributionCenter.Services.ApiImps;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceControlViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private DeviceViewModel? currentDevice;

        [ObservableProperty]
        private ObservableCollection<DeviceTimeControlViewModel> deviceTimeControls;

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        private string commandType;

        [ObservableProperty]
        private string commandTypeName;

        [ObservableProperty]
        private string commandTypeHint;

        [ObservableProperty]
        private string commandTypeDesciption;

        [ObservableProperty]
        private string commandTypeColumnName;

        [ObservableProperty]
        private string? commandRTValue;

        private readonly IMonitorService monitorService;
        private readonly IDeviceControlService deviceControlService;

        public DeviceControlViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, IMonitorService monitorService, IDeviceControlService deviceControlService) 
        {
            CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;

            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                ShowNavigation = true;
            }

            this.monitorService = monitorService;
            this.deviceControlService = deviceControlService;
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());

            var devices = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
            this.Devices = new ObservableCollection<DeviceViewModel>(devices.Select(c =>
            {
                var viewModel = new DeviceViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, DialogHostId);
        }

        [RelayCommand]
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(DialogHostId);
        }

        [RelayCommand]
        private async Task GetDeviceTimeControls()
        {
            if (CurrentDevice == null)
            {
                return;
            }
            var results = (await deviceControlService.GetAll(new DeviceControlDto { DeviceId = CurrentDevice.DeviceId, ControlType = CommandType, ExecutionType = "SCHEDULED" })).Data?.ToList() ?? new List<DeviceControlDto>();
            DeviceTimeControls = new ObservableCollection<DeviceTimeControlViewModel>(results.Select(c=>
            {
                var viewModel = new DeviceTimeControlViewModel();
                viewModel.Binding(c);
                viewModel.SetGridColumnName();
                return viewModel;
            }));
        }

        [RelayCommand]
        private async Task DeleteBatch()
        {
            var selectedIds = DeviceTimeControls.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await deviceControlService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
            }
        }

        [RelayCommand]
        private async Task SaveTimeControl(DeviceTimeControlViewModel viewModel)
        {
            var response = await deviceControlService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                await GetDeviceTimeControls();
                CloseDialog();
            }
        }
    }
}
