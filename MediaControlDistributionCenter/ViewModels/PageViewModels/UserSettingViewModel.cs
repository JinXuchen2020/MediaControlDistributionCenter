using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserSettingViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        public bool IsShelf { get; set; } = true;

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        private ObservableCollection<object> roleList;

        [ObservableProperty]
        private ObservableCollection<InternetDevice> devices;

        [ObservableProperty]
        private string? pageType;

        [ObservableProperty]
        private string? detectStatus;

        [ObservableProperty]
        private string? oldPassword;

        [ObservableProperty]
        private string? newPassword;

        [ObservableProperty]
        private string? newPasswordConfirm;

        private readonly IUserService userService;
        private readonly DeviceManageViewModel deviceManageViewModel;

        public UserSettingViewModel(DeviceManageViewModel deviceManageViewModel)
        {
            this.userService = GetService<IUserService>();
            this.deviceManageViewModel = deviceManageViewModel;
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
            roleList = new ObservableCollection<object>(new List<RoleModel>
            {
                new RoleModel
                {
                    Role = RoleType.Admin.ToString().ToLower(),
                    RoleText = FindResource("LanguageKey_Code_Role_Admin")
                },
                new RoleModel
                {
                    Role = RoleType.Agent.ToString().ToLower(),
                    RoleText = FindResource("LanguageKey_Code_Role_Agent")
                },
                new RoleModel
                {
                    Role = RoleType.User.ToString().ToLower(),
                    RoleText = FindResource("LanguageKey_Code_Role_User")
                }
            });

            RegisterLanguageProperty(this.GetType(), nameof(DetectStatus));
            RegisterDevicesChangedAction(this.GetType(), nameof(LoadData));
        }

        public override void LoadData()
        {
            CurrentUser.LoadLogo();
            Devices = new ObservableCollection<InternetDevice>([.. OnlineDevices]);
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                foreach (var viewModel in OnlineDevices.Where(c => c.DeviceViewModel != null).Select(c => c.DeviceViewModel!))
                {
                    if (viewModel.IsConnected)
                    {
                        await viewModel.VerifySnCodeCommand.ExecuteAsync(null);
                        if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                        {
                            ErrorMessage = viewModel.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            viewModel.ErrorMessage = null;
                            viewModel.DisconnectCommand.Execute(null);
                            continue;
                        }

                        await viewModel.SendUserCommand.ExecuteAsync(null);
                        if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                        {
                            ErrorMessage = viewModel.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            viewModel.ErrorMessage = null;
                            continue;
                        }

                        if (viewModel.IsSendUserCompleted)
                        {
                            ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_129");
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            viewModel.IsSendUserCompleted = false;
                        }
                    }
                }                    

                ErrorMessage = FindResource("LanguageKey_Code_Setting_Tooltip_104");
                await ShowConfirmDialogCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task ResetUser()
        {
            var response = await userService.GetById(CurrentUser.Id);
            if (response.Code == 200)
            {
                CurrentUser.Binding(response.Data!);
            }
        }

        [RelayCommand]
        private async Task DisconnectInternetDevice(InternetDevice device)
        {
            if (device.DeviceViewModel != null)
            {
                device.DeviceViewModel.DisconnectCommand.Execute(null);
                device.DeviceViewModel = null;
                device.Status = 0;
                device.StatusText = GetStatus(0);
            }

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            CurrentUser.Password = NewPassword;

            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                App.Current.Shutdown();
            }
        }

        [RelayCommand]
        private void CancelChangePassword()
        {
            OldPassword = null;
            NewPassword = null;
            NewPasswordConfirm = null;
        }
    }
}
