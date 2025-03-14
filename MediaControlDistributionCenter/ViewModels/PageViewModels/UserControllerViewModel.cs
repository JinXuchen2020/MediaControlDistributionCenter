using CommunityToolkit.Mvvm.ComponentModel;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserControllerViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        [ObservableProperty]
        private string currentTabName;

        [ObservableProperty]
        private string currentPageName;

        public UserControllerViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel)
        {
            CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
        }

        public void SetValues(string tabName, string pageName)
        {
            CurrentTabName = tabName;
            CurrentPageName = pageName;
        }

        public override void LoadData(long? groupId = null)
        {
            return;
        }
    }
}
