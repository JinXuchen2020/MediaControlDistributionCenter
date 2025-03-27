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

        private readonly IMonitorService monitorService;
        private readonly IDeviceControlService deviceControlService;
        private readonly ITimeSyncConfigService timeSyncConfigService;
        private readonly Communication communication;

        public DeviceControlViewModel(Communication communication) 
        {
            this.monitorService = GetService<IMonitorService>();
            this.deviceControlService = GetService<IDeviceControlService>();
            this.timeSyncConfigService = GetService<ITimeSyncConfigService>();
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
            this.communication = communication;

            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeName));
            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeHint));
            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeDesciption));
            RegisterLanguageProperty(this.GetType(), nameof(CommandTypeColumnName));
        }
        public override void LoadData()
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
        private async Task ConnectDevice()
        {
            if (CurrentDevice != null)
            {                
                await CurrentDevice.ConnectCommand.ExecuteAsync(communication);
                if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                {
                    ErrorMessage = CurrentDevice.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                }
                else
                {
                    if (CurrentDevice.StatusText == FindResource("LanguageKey_Code_Online"))
                    {
                        await CurrentDevice.VerifyUserCommand.ExecuteAsync(CurrentUser);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }

                        if (ConnectionMode.Mode == "Local" && (DeviceTimeControls == null || DeviceTimeControls.Count == 0))
                        {
                            await CurrentDevice.SyncDeviceControlCommand.ExecuteAsync(deviceControlService);
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                    }
                }
            }
        }

        [RelayCommand]
        private void DisconnectDevice()
        {
            if (CurrentDevice != null)
            {
                CurrentDevice.DisconnectCommand.Execute(null);
            }
        }

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

                modelList.Add(viewModel.ToModel());
                var modelString = JsonConvert.SerializeObject(modelList);
                switch (CommandType)
                {
                    case "Brightness":
                        await CurrentDevice.ChangeBrightnessCommand.ExecuteAsync(modelString);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        CurrentDevice.Brightness = viewModel.Value;
                        break;
                    case "Volume":
                        await CurrentDevice.ChangeVolumeCommand.ExecuteAsync(modelString);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        CurrentDevice.Volume = viewModel.Value;
                        break;
                    case "Restart":
                        await CurrentDevice.RestartCommand.ExecuteAsync(modelString);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        break;
                }

                var response = await deviceControlService.Save(viewModel.ToModel());
                if (response.Code == 200)
                {
                    await GetDeviceTimeControls();
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
                var modelString = JsonConvert.SerializeObject(modelList);
                switch (viewModels[0].Type)
                {
                    case "Brightness":                        
                        await CurrentDevice.ChangeBrightnessCommand.ExecuteAsync(modelString);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        break;
                    case "Volume":
                        await CurrentDevice.ChangeVolumeCommand.ExecuteAsync(modelString);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        break;
                    case "Restart":
                        await CurrentDevice.RestartCommand.ExecuteAsync(modelString);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        break;
                }

                foreach (var deviceControl in modelList)
                {
                    await deviceControlService.Save(deviceControl);
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
                    case "manual":
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById(CommandRTValue);
                        timeZoneDateTime = TimeZoneInfo.ConvertTime(timeZoneDateTime, timeZone);
                        await CurrentDevice.TimeSyncCommand.ExecuteAsync(timeZoneDateTime);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
                        break;
                    case "gps":
                        await CurrentDevice.TimeGPSSyncCommand.ExecuteAsync(timeZoneDateTime);
                        if (!string.IsNullOrEmpty(CurrentDevice.ErrorMessage))
                        {
                            ErrorMessage = CurrentDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            return;
                        }
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
            var devices = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, Name = SearchString}).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
            this.Devices = new ObservableCollection<DeviceViewModel>(devices.Select(c =>
            {
                var viewModel = new DeviceViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));

            await Task.CompletedTask;
        }
    }
}
