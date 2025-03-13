using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DashboardViewModel : PageViewModel
    {
        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private ObservableCollection<UserViewModel> users;

        [ObservableProperty]
        private ObservableCollection<MediaViewModel> medias;

        public UserViewModel CurrentUser { get; set; }

        public UserViewModel? SelectedUser { get; set; }

        public MediaViewModel? SelectedMedia { get; set; }

        public DashboardViewModel(LoginViewModel loginViewModel, IMonitorService monitorService, IProgramService programService, IUserService userService)
        {
            CurrentUser = loginViewModel.CurrentUser;

            switch (CurrentUser.Role)
            {
                case "admin":
                    var deviceResponse = monitorService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    this.devices = new ObservableCollection<DeviceViewModel>(deviceResponse.Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    var userResponse = userService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
                    this.users = new ObservableCollection<UserViewModel>(userResponse.Select(c =>
                    {
                        var viewModel = new UserViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    this.medias = new ObservableCollection<MediaViewModel>();
                    break;
                case "agent":
                    deviceResponse = monitorService.GetAgentAll(CurrentUser.Account, null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    this.devices = new ObservableCollection<DeviceViewModel>(deviceResponse.Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    userResponse = userService.GetAll(new UserDto { AgentAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
                    this.users = new ObservableCollection<UserViewModel>(userResponse.Select(c =>
                    {
                        var viewModel = new UserViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    this.medias = new ObservableCollection<MediaViewModel>();
                    break;
                case "user":
                    deviceResponse = monitorService.GetAll(new MonitorDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
                    this.devices = new ObservableCollection<DeviceViewModel>(deviceResponse.Select(c =>
                    {
                        var viewModel = new DeviceViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));

                    this.users = new ObservableCollection<UserViewModel>();

                    var mediaResponse = programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
                    this.medias = new ObservableCollection<MediaViewModel>(mediaResponse.Select(c =>
                    {
                        var viewModel = new MediaViewModel();
                        viewModel.Binding(c);
                        return viewModel;
                    }));
                    break;
            }
        }
    }
}
