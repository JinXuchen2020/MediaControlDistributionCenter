using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
