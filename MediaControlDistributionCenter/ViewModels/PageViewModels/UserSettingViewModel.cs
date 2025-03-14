using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using System.Collections.ObjectModel;

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

        public override void LoadData(long? groupId = null)
        {
            return;
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
