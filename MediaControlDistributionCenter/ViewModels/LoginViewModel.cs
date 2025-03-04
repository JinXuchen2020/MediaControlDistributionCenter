using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private int userId;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userRole;

        public LoginViewModel(int userId, string userName, string userRole)
        {
            this.userId = userId;
            this.userName = userName;
            this.userRole = userRole;
        }
    }
}
