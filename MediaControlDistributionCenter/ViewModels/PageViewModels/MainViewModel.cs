using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Helpers.Broadcast;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MainViewModel : PageViewModel
    {
        [ObservableProperty]
        private UserViewModel currentUser;

        [ObservableProperty]
        private DeviceViewModel? connectedDevice;

        [ObservableProperty]
        private string connectionString;

        [ObservableProperty]
        private string deviceConnString;

        public MainViewModel(LoginViewModel loginViewModel, Communication communication)
        {
            currentUser = loginViewModel.CurrentUser;
            connectedDevice = loginViewModel.ConnectedDevice;
            connectionString = ConnectionMode.Mode == "Local" ? FindResource("LanguageKey_Code_Device_Tooltip_103") : FindResource("LanguageKey_Code_Device_Tooltip_104");
            deviceConnString = communication.netClient.IsConnected ? FindResource("LanguageKey_Code_Device_Tooltip_105") : FindResource("LanguageKey_Code_Device_Tooltip_106");
            RegisterLanguageProperty(this.GetType(), nameof(ConnectionString));
            RegisterLanguageProperty(this.GetType(), nameof(DeviceConnString));
        }

        public override void LoadData()
        {
            return;
        }
    }
}
