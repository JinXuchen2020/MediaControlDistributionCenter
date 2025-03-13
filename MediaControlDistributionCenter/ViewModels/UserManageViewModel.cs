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
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Services.ApiImps;
using MaterialDesignThemes.Wpf;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserManageViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";

        [ObservableProperty]
        private ObservableCollection<UserViewModel> users;

        [ObservableProperty]
        private ObservableCollection<UserGroupViewModel> groups;

        public UserViewModel CurrentUser { get; set; }

        public UserViewModel? SelectedUser { get; set; }

        private readonly IUserGroupService userGroupService;
        private readonly IUserService userService;

        public UserManageViewModel(LoginViewModel loginViewModel, IUserGroupService userGroupService, IUserService userService) 
        {
            this.CurrentUser = loginViewModel.CurrentUser;
            this.userService = userService;
            this.userGroupService = userGroupService;
        }

        public void LoadData(long? groupId = null)
        {
            var groups = userGroupService.GetAll(new UserGroupDto { AgentAccount = CurrentUser.Role == "agent" ? CurrentUser.Account : null }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserGroupDto>();
            groups.Insert(0, new UserGroupDto
            {
                Id = -1,
                Name = "全部",
                AgentAccount = CurrentUser.AgentId,
            });

            this.Groups = new ObservableCollection<UserGroupViewModel>(groups.Select(c =>
            {
                var viewModel = new UserGroupViewModel();
                viewModel.Binding(c, groupId == null ? c.Id == -1 : c.Id == groupId);
                return viewModel;
            }));

            var users = userService.GetAll(new UserDto { AgentAccount = CurrentUser.Role == "agent" ? CurrentUser.Account : null, UserGroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
            this.Users = new ObservableCollection<UserViewModel>(users.Select(c =>
            {
                var viewModel = new UserViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));
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
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(DialogHostId);
        }

        [RelayCommand]
        private async Task SaveUser(UserViewModel userViewModel)
        {
            userViewModel.SubmitCommand.Execute(null);
            if (userViewModel.HasErrors)
            {
                return;
            }
            var response = await userService.Save(userViewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
                if (userViewModel.Id == 0)
                {
                    userViewModel.ShowConfirmDialogCommand.Execute(null);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteUser(UserViewModel viewModel)
        { 
            var response = await userService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteUserBatch()
        {
            var selectedIds = Users.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await userService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task SaveGroup(UserGroupViewModel viewModel)
        {
            var response = await userGroupService.Save(viewModel.ToModel());
            if(response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task ChangeGroup(UserGroupViewModel viewModel)
        {
            var selectedUsers = Users.Where(c => c.IsSelected);

            foreach (var user in selectedUsers)
            {
                user.GroupId = viewModel.Id;
                user.AgentId = viewModel.AgentId ?? user.AgentId;

                user.IsSelected = false;
                var response = await userService.Save(user.ToModel());
                if (response.Code == 200)
                {
                }
            }

            LoadData();
            CloseDialog();
        }
    }
}
