using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserSettingViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        public bool IsShelf { get; set; } = true;

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        private ObservableCollection<object> roleList;

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
        }

        public override void LoadData()
        {
            CurrentUser.LoadLogo();
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                if (ConnectionMode.Mode == "Local")
                {
                    var loginViewModel = App.ServicesProvider.GetRequiredService<LoginViewModel>();
                    if (loginViewModel.ConnectedDevice != null && loginViewModel.ConnectedDevice.IsConnected())
                    {
                        await loginViewModel.ConnectedDevice.ShowConfirmDialogCommand.ExecuteAsync(null);
                        if (!string.IsNullOrEmpty(loginViewModel.ConnectedDevice.ErrorMessage))
                        {
                            ErrorMessage = loginViewModel.ConnectedDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            loginViewModel.ConnectedDevice.ErrorMessage = null;
                        }
                        else
                        {
                            if (loginViewModel.ConnectedDevice.IsSendUserCompleted)
                            {
                                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_128"); // "烧录用户信息成功";
                                await ShowConfirmDialogCommand.ExecuteAsync(null);
                                loginViewModel.ConnectedDevice.IsSendUserCompleted = false;
                            }
                        }
                    }
                }
                else
                {
                    if (deviceManageViewModel.SelectedDevice != null && deviceManageViewModel.SelectedDevice.IsConnected())
                    {
                        await deviceManageViewModel.SelectedDevice.ShowConfirmDialogCommand.ExecuteAsync(null);
                        if (!string.IsNullOrEmpty(deviceManageViewModel.SelectedDevice.ErrorMessage))
                        {
                            ErrorMessage = deviceManageViewModel.SelectedDevice.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            deviceManageViewModel.SelectedDevice.ErrorMessage = null;
                        }
                        else
                        {
                            if (deviceManageViewModel.SelectedDevice.IsSendUserCompleted)
                            {
                                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_128"); // "烧录用户信息成功";
                                await ShowConfirmDialogCommand.ExecuteAsync(null);
                                deviceManageViewModel.SelectedDevice.IsSendUserCompleted = false;
                            }
                        }

                    }
                }
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
