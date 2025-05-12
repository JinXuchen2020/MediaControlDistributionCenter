using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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

        private bool isSynced;
        private readonly IMonitorService monitorService;
        private readonly IDeviceControlService deviceControlService;
        private readonly ITimeSyncConfigService timeSyncConfigService;

        public DeviceControlViewModel() 
        {
            this.monitorService = GetService<IMonitorService>();
            this.deviceControlService = GetService<IDeviceControlService>();
            this.timeSyncConfigService = GetService<ITimeSyncConfigService>();
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());

            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeName));
            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeHint));
            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeDesciption));
            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeColumnName));
            RegisterLanguageProperty(this.GetType(), nameof(RefreshTimeZone));
            RegisterDevicesChangedAction(this.GetType(), nameof(LoadData));
        }
        public override async Task LoadData()
        {
            var devices = (await monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, Enabled = 1 })).Data?.ToList() ?? new List<MonitorDto>();
            this.Devices = new ObservableCollection<DeviceViewModel>(devices.OrderByDescending(c => c.Id).Select(c =>
            {
                var viewModel = OnlineDevices.FirstOrDefault(t => t.SnCode == c.SnCode)?.DeviceViewModel;
                if (viewModel == null)
                {
                    viewModel = new DeviceViewModel();
                    viewModel.Binding(c);
                }

                viewModel.RefreshStatus();
                return viewModel;
            }));

            CurrentDevice = CurrentDevice ?? Devices.FirstOrDefault(c => c.IsConnected);
        }

        public async Task SyncDeviceTimeControls()
        {
            if (ConnectionMode.Mode == "Local" && CurrentDevice != null && !isSynced)
            {
                await CurrentDevice.SyncDeviceControlCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                {
                    ErrorMessage = CurrentDevice.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    CurrentDevice.ErrorMessage = null;
                    return;
                }

                await GetDeviceTimeControls();
                isSynced = true;
            }
        }

        public async Task GetDeviceTimeControls()
        {
            if (CurrentDevice == null)
            {
                DeviceTimeControls = new ObservableCollection<DeviceTimeControlViewModel>();
                return;
            }

            var results = (await deviceControlService.GetAll(new DeviceControlDto { DeviceId = CurrentDevice.DeviceId, ControlType = CommandType, ExecutionType = "SCHEDULED" })).Data?.ToList() ?? new List<DeviceControlDto>();
            DeviceTimeControls = new ObservableCollection<DeviceTimeControlViewModel>(results.Select(c =>
            {
                var viewModel = new DeviceTimeControlViewModel();
                viewModel.Binding(c);
                viewModel.SetGridColumnName();
                return viewModel;
            }));
        }

        public void RefreshTimeZone()
        {
            if(CommandType == "TimeSync")
            {
                switch (LanguageTool.Instance.Language)
                {
                    case Language.Chinese:
                        CommandRTValue = "China Standard Time";
                        break;
                    case Language.English:
                        CommandRTValue = "Pacific Standard Time";
                        break;
                    case Language.Japanese:
                        CommandRTValue = "Tokyo Standard Time";
                        break;
                    case Language.Korean:
                        CommandRTValue = "Korea Standard Time";
                        break;
                }
            }
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

        //[RelayCommand]
        //private async Task DetectConnectedDevice()
        //{
        //    //await DetectCommunication(CurrentUser.Account);
        //    var localDevice = OnlineDevices.FirstOrDefault(c => c.DeviceViewModel != null);
        //    if (CurrentDevice?.SNumber != localDevice?.SnCode)
        //    {
        //        isSynced = false;
        //        CurrentDevice = localDevice?.DeviceViewModel;
        //    }
        //    LoadData();
        //}

        [RelayCommand]
        private async Task ExecuteRealTimeControl()
        {
            if (CurrentDevice != null && CommandRTValue != null)
            {
                var modelList = new List<DeviceControlDto>();
                var viewModel = new DeviceTimeControlViewModel()
                {
                    DeviceId = CurrentDevice.DeviceId,
                    Type = CommandType,
                    Value = CommandType == "Restart" ? 1 : double.Parse(CommandRTValue),
                    ExecuteTime = "00:00;00",
                    ExecuteMethod = "REAL_TIME",
                    Status = 1,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    RepeatMode = "",
                    UserAccount = CurrentUser.Account,
                };

                var model = viewModel.ToModel();
                modelList.Add(model);
                switch (CommandType)
                {
                    case "Brightness":
                        await CurrentDevice.ChangeBrightnessCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        CurrentDevice.Brightness = viewModel.Value;
                        break;
                    case "Volume":
                        await CurrentDevice.ChangeVolumeCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        CurrentDevice.Volume = viewModel.Value;
                        break;
                    case "Restart":
                        await CurrentDevice.RestartCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                }
            }
        }

        [RelayCommand]
        private async Task ExecuteScheduleControl()
        {
            var viewModels = DeviceTimeControls.ToList();
            if (CurrentDevice != null && viewModels.Count > 0)
            {
                var modelList = viewModels.Select(c => c.ToModel());
                switch (viewModels[0].Type)
                {
                    case "Brightness":
                        await CurrentDevice.ChangeBrightnessCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                    case "Volume":
                        await CurrentDevice.ChangeVolumeCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                    case "Restart":
                        await CurrentDevice.RestartCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                    case "Power":
                        await CurrentDevice.ChangePowerCommand.ExecuteAsync(modelList);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                }

                await GetDeviceTimeControls();
            }
        }

        [RelayCommand]
        private async Task ExecuteRealTimeTimeSync()
        {
            if (CurrentDevice != null && CommandRTValue != null && !string.IsNullOrEmpty(CommandTypeColumnName))
            {
                var timeZoneDateTime = CurrentDevice.CurrentTime;
                var timeZone = TimeZoneInfo.Local;

                var model = new TimeSyncConfigDto()
                {
                    DeviceId = CurrentDevice.DeviceId,
                    Timezone = timeZone.Id,
                    CurrentDate = timeZoneDateTime.ToString(),
                    SyncMode = CommandTypeColumnName,
                    UserAccount = CurrentUser.Account
                };
                
                switch (CommandTypeColumnName)
                {
                    case "manual":
                        model.Timezone = CommandRTValue;
                        model.CurrentDate = TimeZoneInfo.ConvertTime(timeZoneDateTime, TimeZoneInfo.FindSystemTimeZoneById(CommandRTValue)).ToString();
                        await CurrentDevice.TimeSyncCommand.ExecuteAsync(model);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                    case "gps":
                        await CurrentDevice.TimeGPSSyncCommand.ExecuteAsync(model);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            CurrentDevice.ErrorMessage = null;
                            return;
                        }
                        break;
                }
            }
        }

        [RelayCommand]
        private async Task ExecuteRealTimeControlSync()
        {
            if (CommandType == "Brightness" && CurrentDevice != null)
            {
                await CurrentDevice.SyncBrightnessCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                {
                    ErrorMessage = CurrentDevice.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    CurrentDevice.ErrorMessage = null;
                    return;
                }

                CommandRTValue = CurrentDevice.Brightness.ToString();
            }

            if (CommandType == "Volume" && CurrentDevice != null)
            {
                await CurrentDevice.SyncVolumeCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                {
                    ErrorMessage = CurrentDevice.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    CurrentDevice.ErrorMessage = null;
                    return;
                }

                CommandRTValue = CurrentDevice.Volume.ToString();
            }
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
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }

            if (viewModel.RepeatMode == "year")
            {
                if (string.IsNullOrEmpty(viewModel.RepeatString))
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Control_Tooltip_126");
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    return;
                }
            }

            if (viewModel.RepeatMode == "week" || viewModel.RepeatMode == "month")
            {
                if (string.IsNullOrEmpty(viewModel.RepeatString))
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Control_Tooltip_127");
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    return;
                }
            }

            var response = await deviceControlService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                await GetDeviceTimeControls();
                CloseDialog();
            }
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var devices = (await monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, Name = SearchString}, true)).Data?.ToList() ?? new List<MonitorDto>();
            this.Devices = new ObservableCollection<DeviceViewModel>(devices.Select(c =>
            {
                var viewModel = OnlineDevices.FirstOrDefault(t => t.SnCode == c.SnCode)?.DeviceViewModel;
                if (viewModel == null)
                {
                    viewModel = new DeviceViewModel();
                    viewModel.Binding(c);
                }
                return viewModel;
            }));

            await Task.CompletedTask;
        }
    }
}
