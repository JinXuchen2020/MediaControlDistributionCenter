using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Helpers.Broadcast;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MainViewModel : PageViewModel
    {
        [ObservableProperty]
        private UserViewModel currentUser;

        [ObservableProperty]
        private string connectionString;

        [ObservableProperty]
        private string deviceConnString;

        [ObservableProperty]
        private bool isConnected = false;

        [ObservableProperty]
        private double? storagePercentage;

        [ObservableProperty]
        private double? usedStoragePercentage;

        public MainViewModel(LoginViewModel loginViewModel, Communication communication)
        {
            currentUser = loginViewModel.CurrentUser;
            connectionString = ConnectionMode.Mode == "Local" ? FindResource("LanguageKey_Code_Device_Tooltip_103") : FindResource("LanguageKey_Code_Device_Tooltip_104");
            RegisterLanguageProperty(this.GetType(), nameof(ConnectionString));
            RegisterLanguageProperty(this.GetType(), nameof(DeviceConnString));
            RegisterDevicesChangedAction(this.GetType(), nameof(LoadData));
        }

        public override async Task LoadData()
        {
            IsConnected = OnlineDevices.FirstOrDefault(c => c.Status == 1) != null;
            UsedStoragePercentage = OnlineDevices.FirstOrDefault(c => c.Status == 1)?.DeviceViewModel?.UsedStoragePercentage;
            StoragePercentage = OnlineDevices.FirstOrDefault(c => c.Status == 1)?.DeviceViewModel?.StoragePercentage;
            DeviceConnString = IsConnected ? FindResource("LanguageKey_Code_Device_Tooltip_105") : FindResource("LanguageKey_Code_Device_Tooltip_106");
            await base.LoadData();
        }
    }
}
