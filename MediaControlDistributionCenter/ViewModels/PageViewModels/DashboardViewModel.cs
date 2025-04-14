using BitMiracle.LibTiff.Classic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using SqlSugar;
using System.Collections.ObjectModel;
using System.Linq;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DashboardViewModel : PageViewModel
    {
        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private ObservableCollection<UserViewModel> users;

        [ObservableProperty]
        private ObservableCollection<ProgramViewModel> medias;

        [ObservableProperty]
        private int totalDeviceCount;

        [ObservableProperty]
        private int connectedDeviceCount;

        [ObservableProperty]
        private int disconnectedDeviceCount;

        private readonly IMonitorService monitorService;
        private readonly IProgramService programService;
        private readonly IUserService userService;
        private readonly Communication communication;

        public UserViewModel CurrentUser { get; set; }

        public UserViewModel? SelectedUser { get; set; }

        public ProgramViewModel? SelectedMedia { get; set; }

        public DashboardViewModel(LoginViewModel loginViewModel, Communication communication)
        {
            CurrentUser = loginViewModel.CurrentUser;
            this.monitorService = GetService<IMonitorService>();
            this.programService = GetService<IProgramService>();
            this.userService = GetService<IUserService>();
            this.communication = communication;
        }

        public override void LoadData()
        {
            switch (CurrentUser.Role)
            {
                case "admin":
                    var deviceResponse = monitorService.GetAll(new MonitorDto { Enabled = 1}).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    var deviceViewModels = deviceResponse.Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        if (ConnectedDevice != null && viewModel.SNumber == ConnectedDevice.SNumber)
                        {
                            viewModel.ConnectCommand.Execute(communication);
                        }

                        viewModel.GetPrograms();
                        return viewModel;
                    });
                    Devices = new ObservableCollection<DeviceViewModel>(deviceViewModels.OrderByDescending(c => c.IsConnected).Take(5));

                    TotalDeviceCount = deviceResponse.Count();
                    ConnectedDeviceCount = deviceViewModels.Where(c => c.IsConnected).Count();
                    DisconnectedDeviceCount = TotalDeviceCount - ConnectedDeviceCount;

                    var userResponse = userService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
                    Users = new ObservableCollection<UserViewModel>(userResponse.OrderByDescending(c => c.Id).Take(10).Select(c =>
                    {
                        var viewModel = new UserViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    Medias = new ObservableCollection<ProgramViewModel>();
                    break;
                case "agent":
                    deviceResponse = monitorService.GetAgentAll(CurrentUser.Account, new MonitorDto { Enabled = 1 }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    deviceViewModels = deviceResponse.Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        if (ConnectedDevice != null && viewModel.SNumber == ConnectedDevice.SNumber)
                        {
                            viewModel.ConnectCommand.Execute(communication);
                        }

                        viewModel.GetPrograms();
                        return viewModel;
                    });
                    Devices = new ObservableCollection<DeviceViewModel>(deviceViewModels.OrderByDescending(c => c.IsConnected).Take(5));

                    TotalDeviceCount = deviceResponse.Count();
                    ConnectedDeviceCount = deviceViewModels.Where(c => c.IsConnected).Count();
                    DisconnectedDeviceCount = TotalDeviceCount - ConnectedDeviceCount;

                    userResponse = userService.GetAll(new UserDto { AgentAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
                    Users = new ObservableCollection<UserViewModel>(userResponse.OrderByDescending(c => c.Id).Take(10).Select(c =>
                    {
                        var viewModel = new UserViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    Medias = new ObservableCollection<ProgramViewModel>();
                    break;
                case "user":
                    deviceResponse = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account, Enabled = 1 }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    deviceViewModels = deviceResponse.Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        if (ConnectedDevice != null && viewModel.SNumber == ConnectedDevice.SNumber)
                        {
                            viewModel.ConnectCommand.Execute(communication);
                        }

                        viewModel.GetPrograms();
                        return viewModel;
                    });
                    Devices = new ObservableCollection<DeviceViewModel>(deviceViewModels.OrderByDescending(c => c.IsConnected).Take(4));

                    TotalDeviceCount = deviceResponse.Count();
                    ConnectedDeviceCount = deviceViewModels.Where(c => c.IsConnected).Count();
                    DisconnectedDeviceCount = TotalDeviceCount - ConnectedDeviceCount;

                    Users = new ObservableCollection<UserViewModel>();

                    var mediaResponse = programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
                    Medias = new ObservableCollection<ProgramViewModel>(mediaResponse.OrderByDescending(c => c.Id).Take(3).Select(c =>
                    {
                        var viewModel = new ProgramViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));
                    break;
            }
        }

        [RelayCommand]
        private async Task DetectConnectedDevice()
        {
            await DetectCommunication(CurrentUser.Account);
            LoadData();
        }
    }
}
