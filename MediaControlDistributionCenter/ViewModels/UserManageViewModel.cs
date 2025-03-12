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

        public UserManageViewModel(IUserGroupService userGroupService, IUserService userService) 
        {
            this.userService = userService;
            this.userGroupService = userGroupService;
        }

        public void SetValues(UserViewModel viewModel, long? groupId = null)
        {
            this.CurrentUser = viewModel;
            var groups = userGroupService.GetAll(new UserGroupDto { AgentAccount = viewModel.Role == "agent" ? viewModel.Account : null }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserGroupDto>();
            groups.Insert(0, new UserGroupDto
            {
                Id = -1,
                Name = "全部",
                AgentAccount = viewModel.AgentId,
            });

            this.Groups = new ObservableCollection<UserGroupViewModel>(groups.Select(c =>
            {
                var viewModel = new UserGroupViewModel();
                viewModel.Binding(c, groupId == null ? c.Id == -1 : c.Id == groupId);
                return viewModel;
            }));

            var users = userService.GetAll(new UserDto { AgentAccount = viewModel.Role == "agent" ? viewModel.Account : null, UserGroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
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
            var response = await userService.Save(userViewModel.ToModel());
            if (response.Code == 200)
            {
                userViewModel.Group = Groups.FirstOrDefault(c => c.Id == userViewModel.GroupId)?.Name ?? "未分组";
                CloseDialog();
                if (userViewModel.Id == 0)
                {
                    Users.Add(userViewModel);
                    userViewModel.ShowConfirmDialogCommand.Execute(null);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteUser(UserViewModel viewModel)
        {
            Users.Remove(viewModel);
            var response = await userService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
            }
        }

        [RelayCommand]
        private async Task DeleteUserBatch()
        {
            var selectedIds = Users.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await userService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
            }
        }

        [RelayCommand]
        private async Task SaveGroup(UserGroupViewModel viewModel)
        {
            var response = await userGroupService.Save(viewModel.ToModel());
            if(response.Code == 200)
            {
                if (viewModel.Id == 0)
                {
                    Groups.Add(viewModel);
                }

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
                user.Group = Groups.FirstOrDefault(c => c.Id == viewModel?.Id)?.Name ?? "未分组";

                user.IsSelected = false;
                var response = await userService.Save(user.ToModel());
                if (response.Code == 200)
                {
                }
            }

            CloseDialog();
        }
    }
}
