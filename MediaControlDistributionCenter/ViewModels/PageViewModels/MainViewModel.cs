using CommunityToolkit.Mvvm.ComponentModel;

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

        public override void LoadData(long? groupId = null)
        {
            return;
        }
    }
}
