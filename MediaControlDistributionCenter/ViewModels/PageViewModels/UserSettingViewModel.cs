using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserSettingViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        private string? oldPassword;

        [ObservableProperty]

        private string? newPassword;

        [ObservableProperty]
        private string? newPasswordConfirm;

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

        [RelayCommand]
        private async Task ChangePassword()
        {
            if (OldPassword != CurrentUser.Password)
            {
                MessageBox.Show("原密码错误！");
                return;
            }

            if (NewPassword == null || NewPasswordConfirm == null)
            {
                MessageBox.Show("请填写新密码！");
                return;
            }

            if (NewPassword != NewPasswordConfirm)
            {
                MessageBox.Show("二次输入密码不一致！");
                return;
            }

            CurrentUser.Password = NewPassword;

            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                MessageBox.Show("密码修改成功！");
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
