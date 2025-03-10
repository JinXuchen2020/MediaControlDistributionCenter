using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Views;
using MediaControlDistributionCenter.Views.UserManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string role;

        [ObservableProperty]
        private int? groupId;

        [ObservableProperty]
        private int? agentId;

        [ObservableProperty]
        private string group;

        [ObservableProperty]
        private string account;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string region;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private ObservableCollection<UserGroupViewModel> groups;

        public UserViewModel()
        {
        }

        public UserViewModel(User model)
        {
            id = model.Id;
            role = model.Role;
            groupId = model.GroupId;
            agentId = model.AgentId;
            group = model.Group?.Name ?? "未分组";
            account = model.Account;
            name = model.Name;
            region = model.Region;
            password = model.Password;
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }

        [RelayCommand]
        private void Reset()
        {
            Role = string.Empty;
            GroupId = null;
            Account = string.Empty;
            Name = string.Empty;
            Region = string.Empty;
            Password = string.Empty;
        }

        public User ToModel()
        {
            return new User
            {
                Id = Id,
                Name = Name,
                Account = Account,
                Region = Region,
                Password = Password,
                GroupId = GroupId,
                AgentId = AgentId,
                Role = Role
            };
        }
    }
}
