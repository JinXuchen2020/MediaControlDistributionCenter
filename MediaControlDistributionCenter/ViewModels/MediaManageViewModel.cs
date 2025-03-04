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
    public partial class MediaManageViewModel : ObservableObject
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        [ObservableProperty]
        private ObservableCollection<MediaGroupViewModel> mediaGroups;

        [ObservableProperty]
        private ObservableCollection<MediaViewModel> medias;

        [ObservableProperty]
        private int selectedGroupId;

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        public MediaManageViewModel(UserViewModel currentUser, IEnumerable<MediaGroupViewModel> mediaGroups, IEnumerable<MediaViewModel> medias) 
        {
            CurrentUser = currentUser;
            this.mediaGroups = new ObservableCollection<MediaGroupViewModel>(mediaGroups);
            this.medias = new ObservableCollection<MediaViewModel>(medias);
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
