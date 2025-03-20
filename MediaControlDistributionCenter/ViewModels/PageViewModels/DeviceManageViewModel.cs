using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
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

        public DeviceViewModel? SelectedDevice { get; set; }

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        [ObservableProperty]
        private ObservableCollection<DeviceGroupViewModel> deviceGroups;

        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private long? selectedGroupId;

        private readonly IMonitorService monitorService;
        private readonly IMonitorGroupService monitorGroupService;
        private readonly IUserService userService;
        private readonly IUserGroupService userGroupService;
        private readonly Communication communication;

        public DeviceManageViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, Communication communication) 
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

            this.monitorService = GetService<IMonitorService>();
            this.monitorGroupService = GetService<IMonitorGroupService>();
            this.userService = GetService<IUserService>();
            this.userGroupService = GetService<IUserGroupService>();
            this.communication = communication;
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override void LoadData(long? groupId = null)
        {
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
            this.Devices = new ObservableCollection<DeviceViewModel>(devices.Select(c =>
            {
                var result = new DeviceViewModel();
                result.Binding(c);
                return result;
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
            viewModel.Status = 1;

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
        private async Task DeleteDevice(DeviceViewModel viewModel)
        {
            var response = await monitorService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task EnableDevice(DeviceViewModel viewModel)
        {
            var response = await monitorService.EnableById(viewModel.Id, viewModel.Enabled == 1);
            if (response.Code == 200)
            {
                LoadData();
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
        private async Task SaveDevice(DeviceViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }
            var response = await monitorService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task ConnectDevice(DeviceViewModel viewModel)
        {
            await viewModel.ConnectCommand.ExecuteAsync(communication);
            if (!string.IsNullOrEmpty(viewModel?.ErrorMessage))
            {
                ErrorMessage = viewModel.ErrorMessage;
                await ShowConfirmDialogCommand.ExecuteAsync(null);
            }
            else
            {
                viewModel?.ShowConfirmDialogCommand.Execute(null);
                if (viewModel.StatusText == FindResource("LanguageKey_Code_Online"))
                {
                    SelectedDevice = viewModel;
                }
            }
        }

        [RelayCommand]
        private async Task SendUserToDevice()
        {
            if (SelectedDevice == null)
            {
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                await ShowConfirmDialogCommand.ExecuteAsync(null);
                return;
            }

            var result = new UsersSync();
            var users = new List<UserSync>();
            var adminUser = userService.GetAll(new UserDto { Role = "admin" }).GetAwaiter().GetResult().Data?.FirstOrDefault();
            if (adminUser != null)
            {
                users.Add(new UserSync(adminUser, null, null));
            }

            UserGroupDto? userGroup = null;
            if (!string.IsNullOrEmpty(CurrentUser.AgentId))
            {
                var agentUser = userService.GetAll(new UserDto { Account = CurrentUser.AgentId }).GetAwaiter().GetResult().Data?.FirstOrDefault();
                if (agentUser != null)
                {
                    users.Add(new UserSync(agentUser, null, null));
                }

                userGroup = (await userGroupService.GetById(CurrentUser.GroupId!.Value)).Data;
            }


            users.Add(new UserSync(CurrentUser.ToModel(), userGroup, new MonitorSync(SelectedDevice!.ToModel(), null)));
            result.Users = users;

            await SelectedDevice.SendUserCommand.ExecuteAsync(result);

        }

        [RelayCommand]
        private void DisconnectDevice()
        {
            if (SelectedDevice != null)
            {
                SelectedDevice.DisconnectCommand.Execute(null);
            }
        }
    }
}
