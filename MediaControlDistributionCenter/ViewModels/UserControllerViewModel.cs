using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserControllerViewModel : ObservableObject
    {
        public UserViewModel CurrentUser { get; set; }

        [ObservableProperty]
        private string currentTabName;

        [ObservableProperty]
        private string currentPageName;

        public void SetValues(UserViewModel userViewModel, string tabName, string pageName)
        {
            CurrentUser = userViewModel;
            CurrentTabName = tabName;
            CurrentPageName = pageName;
        }
    }
}
