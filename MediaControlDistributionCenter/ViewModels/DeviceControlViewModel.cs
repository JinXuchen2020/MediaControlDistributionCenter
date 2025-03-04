using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using MediaControlDistributionCenter.Data.Entity;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using MediaControlDistributionCenter.Views.UserManagement;
using MediaControlDistributionCenter.Services;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceControlViewModel : ObservableObject
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        [ObservableProperty]
        public ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        public DeviceViewModel? currentDevice;

        [ObservableProperty]
        public ObservableCollection<DeviceTimeControlViewModel> deviceTimeControls;

        [ObservableProperty]
        public ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        public string commandType;

        [ObservableProperty]
        public string commandTypeName;

        [ObservableProperty]
        public string commandTypeHint;

        [ObservableProperty]
        public string commandTypeDesciption;

        [ObservableProperty]
        public string commandTypeColumnName;

        [ObservableProperty]
        public string? commandRTValue;

        public DeviceControlViewModel(UserViewModel currentUser, IEnumerable<DeviceViewModel> devices) 
        {
            CurrentUser = currentUser;
            this.devices = new ObservableCollection<DeviceViewModel>(devices);
            commandType = "Brightness";

            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, DialogHostId);
        }

        [RelayCommand]
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(DialogHostId);
        }

        [RelayCommand]
        private async Task GetDeviceTimeControls(IDeviceService service)
        {
            if (CurrentDevice == null)
            {
                return;
            }
            var results = await service.GetDeviceTimeControls(CurrentDevice.Id, CommandType);
            DeviceTimeControls = new ObservableCollection<DeviceTimeControlViewModel>(results);
            foreach (var item in DeviceTimeControls)
            {
                item.SetGridColumnName();
                
            }
        }
    }
}
