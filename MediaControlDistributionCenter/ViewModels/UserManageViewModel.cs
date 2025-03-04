using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.UserManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MediaControlDistributionCenter.Data.Entity;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserManageViewModel : ObservableObject
    {
        private const string DialogHostId = "RootDialogHostId";

        [ObservableProperty]
        private ObservableCollection<UserViewModel> users;

        [ObservableProperty]
        private ObservableCollection<UserGroupViewModel> groups;

        public UserViewModel CurrentUser { get; set; }

        public UserManageViewModel(UserViewModel currentUser, IEnumerable<UserViewModel> users, IEnumerable<UserGroupViewModel> groups) 
        {
            this.users = new ObservableCollection<UserViewModel>(users);
            this.groups = new ObservableCollection<UserGroupViewModel>(groups);
            this.CurrentUser = currentUser;
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, DialogHostId);
        }

        [RelayCommand]
        private async Task ShowDialogContent(UserControl dialogContent)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialogContent, DialogHostId);
        }

        [RelayCommand]
        private async Task CloseDialog(bool isUserDialog)
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(DialogHostId);
            if (isUserDialog)
            {
                var dialogBox = new UserSaveResultDialog(CurrentUser);
                await MaterialDesignThemes.Wpf.DialogHost.Show(dialogBox, DialogHostId);
            }
        }
    }
}
