using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dm.filter;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using MediaControlDistributionCenter.Views.UserManagement;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]
        private string role;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string agentId;

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

        //private readonly IAuthService authService;
        //private readonly IUserService userService;
        //private readonly IUserGroupService userGroupService;


        //public UserViewModel(IUserService userService, IUserGroupService userGroupService)
        //{
        //    this.authService = authService;
        //    this.userService = userService;
        //    this.userGroupService = userGroupService;
        //}

        //public UserViewModel(IUserService userService, IUserGroupService userGroupService)
        //{
        //    this.authService = authService;
        //    this.userService = userService;
        //    this.userGroupService = userGroupService;
        //}

        //public UserViewModel(UserDto model)
        //{
        //    id = model.Id;
        //    role = model.Role;
        //    groupId = model.UserGroupId;
        //    agentId = model.AgentAccount;
        //    group = model.UserGroupName ?? "未分组";
        //    account = model.Account;
        //    name = model.Company;
        //    region = model.Region;
        //    password = model.Password;
        //}

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

        public UserDto ToModel()
        {
            return new UserDto
            {
                Id = Id,
                Company = Name,
                Account = Account,
                Region = Region,
                Password = Password,
                UserGroupId = GroupId,
                AgentAccount = AgentId,
                Role = Role
            };
        }

        public void Binding(UserDto model)
        {
            Id = model.Id;
            Role = model.Role;
            GroupId = model.UserGroupId;
            AgentId = model.AgentAccount;
            Group = model.UserGroupName ?? "未分组";
            Account = model.Account;
            Name = model.Company;
            Region = model.Region;
            Password = model.Password;
        }
    }
}
