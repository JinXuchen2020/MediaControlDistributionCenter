using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceManageViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";

        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        public DeviceGroupViewModel? SelectedGroup { get; set; }

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        [ObservableProperty]
        private ObservableCollection<DeviceGroupViewModel> deviceGroups;

        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private long? selectedGroupId;

        [ObservableProperty]
        private string selectDisabled = "1";

        [ObservableProperty]
        private bool isSearching;

        private readonly IMonitorService monitorService;
        private readonly IMonitorGroupService monitorGroupService;
        private readonly IUserService userService;
        private readonly IUserGroupService userGroupService;
        private readonly IPlaybackRecordService playbackRecordService;
        private readonly IProgramService programService;

        public DeviceManageViewModel() 
        {
            this.monitorService = GetService<IMonitorService>();
            this.monitorGroupService = GetService<IMonitorGroupService>();
            this.userService = GetService<IUserService>();
            this.userGroupService = GetService<IUserGroupService>();
            this.programService = GetService<IProgramService>();
            this.playbackRecordService = GetService<IPlaybackRecordService>();
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
            RegisterDevicesChangedAction(this.GetType(), nameof(LoadData));
        }

        public override void LoadData()
        {
            if (CurrentUser == null)
            {
                return;
            }
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var groups = monitorGroupService.GetAll(new MonitorGroupDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorGroupDto>();
            groups.Insert(0, new MonitorGroupDto
            {
                Id = -1,
                Name = FindResource("LanguageKey_Code_All"),
                UserAccount = CurrentUser.Account,
            });
            this.DeviceGroups = new ObservableCollection<DeviceGroupViewModel>(groups.Select(c =>
            {
                var result = new DeviceGroupViewModel();
                result.Binding(c, c.Id == (groupId ?? -1) ? true : false);
                return result;
            }));

            var devices = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
            this.Devices = new ObservableCollection<DeviceViewModel>(devices.Where(c => c.Enabled == int.Parse(SelectDisabled)).OrderByDescending(c => c.Id).Select(c =>
            {
                var viewModel = OnlineDevices.FirstOrDefault(t => t.SnCode == c.SnCode)?.DeviceViewModel;
                if (viewModel == null)
                {
                    viewModel = new DeviceViewModel();
                    viewModel.Binding(c);
                }

                viewModel.GetPrograms();
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

        public DeviceViewModel CreateDevice()
        {
            var viewModel = new DeviceViewModel();
            viewModel.UserId = CurrentUser.Account;
            viewModel.OwnerName = userService.GetAll(new UserDto { Account = CurrentUser.Account }).GetAwaiter().GetResult().Data!.First().Company;
            viewModel.DeviceId = "";
            viewModel.Status = 0;
            viewModel.Enabled = 1;
            viewModel.StartDate = DateTime.Now;
            viewModel.EndDate = DateTime.Now;

            return viewModel;
        }

        [RelayCommand]
        private async Task CreateGroup(DeviceGroupViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }
            var response = await monitorGroupService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task ChangeGroup()
        {
            var selectedItems = Devices.Where(c => c.IsSelected);

            foreach (var item in selectedItems)
            {
                item.GroupId = SelectedGroupId;

                item.IsSelected = false;
                var response = await monitorService.Save(item.ToModel());
                if (response.Code == 200)
                {
                    SelectedGroupId = null;
                    LoadData();
                    CloseDialog();
                }
            }
        }

        [RelayCommand]
        private async Task DeleteGroup(DeviceGroupViewModel viewModel)
        {
            var response = await monitorGroupService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                var agentUsers = monitorService.GetAll(new MonitorDto { GroupId = viewModel.Id }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                foreach (var item in agentUsers)
                {
                    item.GroupId = null;
                    await monitorService.Save(item);
                }
            }

            LoadData();
        }

        [RelayCommand]
        private async Task DeleteDevice(DeviceViewModel viewModel)
        {
            var response = await monitorService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                var deviceControlService = GetService<IDeviceControlService>();
                var deviceControls = (await deviceControlService.GetAll(new DeviceControlDto { DeviceId = viewModel.DeviceId })).Data?.ToList() ?? new List<DeviceControlDto>();
                await deviceControlService.DeleteBatch(deviceControls.Select(c => c.Id).ToList());

                var playRecords = (await playbackRecordService.GetAll(new PlaybackRecordDto { MonitorSnCode = viewModel.SNumber })).Data?.ToList() ?? new List<PlaybackRecordDto>();
                await playbackRecordService.DeleteBatch(playRecords.Select(c => c.Id).ToList());
                LoadData();
            }
        }

        [RelayCommand]
        private async Task EnableDevice(DeviceViewModel viewModel)
        {
            if (ConnectionMode.Mode == "Remote" && viewModel.IsConnected)
            {
                await viewModel.VerifySnCodeCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                {
                    ErrorMessage = viewModel.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.ErrorMessage = null;
                    viewModel.DisconnectCommand.Execute(null);
                    return;
                }

                await viewModel.EnableMonitorCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                {
                    ErrorMessage = viewModel.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.ErrorMessage = null;
                    return;
                }

                if (viewModel.Enabled == 1)
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_130");
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                }

                if (viewModel.Enabled == 0)
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_131");
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                }

                var response = await monitorService.EnableById(viewModel.Id, viewModel.Enabled == 1);
                if (response.Code == 200)
                {
                    LoadData();
                }
            }
        }

        [RelayCommand]
        private async Task ActivateDevice(DeviceViewModel viewModel)
        {
            if (ConnectionMode.Mode == "Remote" && viewModel.IsConnected)
            {
                await viewModel.VerifySnCodeCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                {
                    ErrorMessage = viewModel.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.ErrorMessage = null;
                    viewModel.DisconnectCommand.Execute(null);
                    return;
                }

                viewModel.Status = 1;
                await viewModel.SendUserCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                {
                    ErrorMessage = viewModel.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.ErrorMessage = null;
                    return;
                }

                if (viewModel.IsSendUserCompleted)
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_128"); 
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.IsSendUserCompleted = false;
                }

                var response = await monitorService.Save(viewModel.ToModel());
                if (response.Code == 200)
                {
                    LoadData();
                }
            }
        }

        [RelayCommand]
        private async Task SaveDevice(DeviceViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }

            if (viewModel.Id != 0 && viewModel.IsConnected)
            {
                await viewModel.VerifySnCodeCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                {
                    ErrorMessage = viewModel.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.ErrorMessage = null;
                    viewModel.DisconnectCommand.Execute(null);
                    return;
                }

                await viewModel.SendUserCommand.ExecuteAsync(null);
                if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                {
                    ErrorMessage = viewModel.ErrorMessage;
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.ErrorMessage = null;
                    return;
                }

                if (viewModel.IsSendUserCompleted)
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_129"); 
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    viewModel.IsSendUserCompleted = false;
                }
            }

            var response = await monitorService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task DetectConnectedDevice()
        {
            await DetectCommunication(CurrentUser.Account);
            LoadData();
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var nameDevices = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, Name = SearchString, GroupId = groupId }, true).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
            var snDevices = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, SnCode = SearchString, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();

            nameDevices.AddRange(snDevices);
            this.Devices = new ObservableCollection<DeviceViewModel>(nameDevices.Select(c =>
            {
                var viewModel = OnlineDevices.FirstOrDefault(t => t.SnCode == c.SnCode)?.DeviceViewModel;
                if (viewModel == null)
                {
                    viewModel = new DeviceViewModel();
                    viewModel.Binding(c);
                }

                viewModel.GetPrograms();
                return viewModel;
            }));

            await Task.CompletedTask;
        }
    }
}
