using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using SqlSugar;
using System.Collections.ObjectModel;

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

        private readonly IMonitorService monitorService;
        private readonly IProgramService programService;
        private readonly IUserService userService;

        public UserViewModel CurrentUser { get; set; }

        public UserViewModel? SelectedUser { get; set; }

        public ProgramViewModel? SelectedMedia { get; set; }

        public DashboardViewModel(LoginViewModel loginViewModel)
        {
            CurrentUser = loginViewModel.CurrentUser;
            this.monitorService = GetService<IMonitorService>();
            this.programService = GetService<IProgramService>();
            this.userService = GetService<IUserService>();
        }

        public override void LoadData()
        {
            switch (CurrentUser.Role)
            {
                case "admin":
                    var deviceResponse = monitorService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    Devices = new ObservableCollection<DeviceViewModel>(deviceResponse.OrderByDescending(c=>c.Id).Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    var userResponse = userService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
                    Users = new ObservableCollection<UserViewModel>(userResponse.OrderByDescending(c => c.Id).Take(15).Select(c =>
                    {
                        var viewModel = new UserViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    Medias = new ObservableCollection<ProgramViewModel>();
                    break;
                case "agent":
                    deviceResponse = monitorService.GetAgentAll(CurrentUser.Account, null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    Devices = new ObservableCollection<DeviceViewModel>(deviceResponse.OrderByDescending(c => c.Id).Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    userResponse = userService.GetAll(new UserDto { AgentAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
                    Users = new ObservableCollection<UserViewModel>(userResponse.OrderByDescending(c => c.Id).Take(15).Select(c =>
                    {
                        var viewModel = new UserViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    Medias = new ObservableCollection<ProgramViewModel>();
                    break;
                case "user":
                    deviceResponse = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    Devices = new ObservableCollection<DeviceViewModel>(deviceResponse.OrderByDescending(c => c.Id).Take(6).Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

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
    }
}
