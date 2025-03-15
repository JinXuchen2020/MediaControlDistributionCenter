using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceManageViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";

        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        [ObservableProperty]
        private ObservableCollection<DeviceGroupViewModel> deviceGroups;

        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private long? selectedGroupId;

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        private readonly IMonitorService monitorService;
        private readonly IMonitorGroupService monitorGroupService;
        private readonly IUserService userService;

        public DeviceManageViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel) 
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
        }

        public override void LoadData(long? groupId = null)
        {
            var groups = monitorGroupService.GetAll(new MonitorGroupDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorGroupDto>();
            groups.Insert(0, new MonitorGroupDto
            {
                Id = -1,
                Name = "全部",
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
            //viewModel.SubmitCommand.Execute(null);
            //if (!viewModel.HasErrors)
            //{
            var response = await monitorService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }
    }
}
