using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private ObservableCollection<UserViewModel> users;

        [ObservableProperty]
        private ObservableCollection<MediaViewModel> medias;

        public UserViewModel CurrentUser { get; set; }

        public DashboardViewModel(UserViewModel currentUser, IEnumerable<DeviceViewModel> devices, IEnumerable<UserViewModel> users, IEnumerable<MediaViewModel> medias)
        {
            this.devices = new ObservableCollection<DeviceViewModel>(devices);
            this.users = new ObservableCollection<UserViewModel>(users);
            this.medias = new ObservableCollection<MediaViewModel>(medias);
            this.CurrentUser = currentUser;
        }
    }
}
