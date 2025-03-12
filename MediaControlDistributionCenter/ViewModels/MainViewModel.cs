using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MainViewModel : PageViewModel
    {
        [ObservableProperty]
        private UserViewModel currentUser;

        public MainViewModel(LoginViewModel loginViewModel)
        {
            currentUser = loginViewModel.CurrentUser;
        }
    }
}
