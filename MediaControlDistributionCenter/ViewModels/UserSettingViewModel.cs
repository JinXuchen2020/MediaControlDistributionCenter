using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserSettingViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        private readonly IUserService userService;

        public UserSettingViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, IUserService userService)
        {
            this.userService = userService;
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());

            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                CurrentUser = dashboardViewModel.CurrentUser;
                LoginUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                LoginUser = dashboardViewModel.CurrentUser;
                CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
            }
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                CurrentUser.ShowConfirmDialogCommand.Execute(null);
            }
        }
    }
}
