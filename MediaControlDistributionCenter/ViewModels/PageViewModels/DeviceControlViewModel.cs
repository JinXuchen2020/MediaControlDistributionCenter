using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.Collections.ObjectModel;
using System.Windows;

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
        private readonly ITimeSyncConfigService timeSyncConfigService;

        public DeviceControlViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, IMonitorService monitorService, IDeviceControlService deviceControlService, ITimeSyncConfigService timeSyncConfigService) 
        {
            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                ShowNavigation = true;
                CurrentUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
            }

            this.monitorService = monitorService;
            this.deviceControlService = deviceControlService;
            this.timeSyncConfigService = timeSyncConfigService;
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());

            LoadData();
        }
        public override void LoadData(long? groupId = null)
        {
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
        private async Task ExecuteRealTimeControl()
        {
            if (CurrentDevice != null && CommandRTValue != null)
            {
                switch (CommandType)
                {
                    case "Brightness":
                        await CurrentDevice.ChangeBrightnessCommand.ExecuteAsync(CommandRTValue);
                        break;
                    case "Volume":
                        await CurrentDevice.ChangeVolumeCommand.ExecuteAsync(CommandRTValue);
                        break;
                    case "Restart":
                        CommandRTValue = "1";
                        await CurrentDevice.RestartCommand.ExecuteAsync(CommandRTValue);
                        break;
                }
                var viewModel = new DeviceTimeControlViewModel()
                {
                    DeviceId = CurrentDevice.DeviceId,
                    Type = CommandType,
                    Value = double.Parse(CommandRTValue),
                    ExecuteTime = "00:00;00",
                    ExecuteMethod = "REAL_TIME",
                    Status = 1,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    RepeatMode = "",
                    UserAccount = CurrentUser.Account,
                };

                var response = await deviceControlService.Save(viewModel.ToModel());
                if (response.Code == 200)
                {
                    await GetDeviceTimeControls();
                }
            }
        }

        [RelayCommand]
        private async Task ExecuteRealTimeTimeSync()
        {
            if (CurrentDevice != null && CommandRTValue != null && !string.IsNullOrEmpty(CommandTypeColumnName))
            {
                var timeZoneDateTime = DateTime.Now;
                var timeZone = TimeZoneInfo.Local;
                switch (CommandTypeColumnName)
                {
                    case "手动":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById(CommandRTValue);
                        timeZoneDateTime = TimeZoneInfo.ConvertTime(timeZoneDateTime, timeZone);
                        CurrentDevice.TimeSyncCommand.Execute(timeZoneDateTime);
                        break;
                    case "GPS":
                        CurrentDevice.TimeGPSSyncCommand.Execute(timeZoneDateTime);
                        break;
                }
                var model = new TimeSyncConfigDto()
                {
                    DeviceId = CurrentDevice.DeviceId,
                    Timezone = timeZone.DisplayName,
                    SyncMode = CommandTypeColumnName,
                    UserAccount = CurrentUser.Account
                };

                var response = await timeSyncConfigService.Save(model);
                if (response.Code == 200)
                {
                }
            }
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
                await GetDeviceTimeControls();
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
