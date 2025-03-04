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
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceManageViewModel : ObservableObject
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        [ObservableProperty]
        public ObservableCollection<DeviceGroupViewModel> deviceGroups;

        [ObservableProperty]
        public ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        public int? selectedGroupId;

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        public DeviceManageViewModel(UserViewModel currentUser, IEnumerable<DeviceGroupViewModel> deviceGroups, IEnumerable<DeviceViewModel> devices) 
        {
            CurrentUser = currentUser;
            this.deviceGroups = new ObservableCollection<DeviceGroupViewModel>(deviceGroups);
            this.devices = new ObservableCollection<DeviceViewModel>(devices);
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
    }
}
