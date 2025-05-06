using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserManageViewModel : PageViewModel
    {
        [ObservableProperty]
        private ObservableCollection<UserViewModel> users;

        [ObservableProperty]
        private ObservableCollection<UserGroupViewModel> groups;

        public UserViewModel CurrentUser { get; set; }

        public UserViewModel? SelectedUser { get; set; }

        public UserGroupViewModel? SelectedGroup { get; set; }

        private readonly IUserGroupService userGroupService;
        private readonly IUserService userService;

        public UserManageViewModel(LoginViewModel loginViewModel) 
        {
            this.CurrentUser = loginViewModel.CurrentUser;
            this.userService = GetService<IUserService>();
            this.userGroupService = GetService<IUserGroupService>();
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override async Task LoadData()
        {
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var groups = (await userGroupService.GetAll(new UserGroupDto { AgentAccount = CurrentUser.Account })).Data?.ToList() ?? new List<UserGroupDto>();
            groups.Insert(0, new UserGroupDto
            {
                Id = -1,
                Name = FindResource("LanguageKey_Code_All"),
                AgentAccount = CurrentUser.Account,
            });

            this.Groups = new ObservableCollection<UserGroupViewModel>(groups.Select(c =>
            {
                var viewModel = new UserGroupViewModel();
                viewModel.Binding(c, c.Id == (groupId ?? -1) ? true : false);
                return viewModel;
            }));

            var query = new UserDto
            {
                AgentAccount = CurrentUser.Role == "agent" ? CurrentUser.Account : null
            };

            if (CurrentUser.Role == "agent")
            {
                query.AgentUserGroupId = groupId;
            }
            else
            {
                query.AdminUserGroupId = groupId;
            }

            var users = (await userService.GetAll(query)).Data?.ToList() ?? new List<UserDto>();
            this.Users = new ObservableCollection<UserViewModel>(users.OrderByDescending(c => c.Id).Select(c =>
            {
                var viewModel = new UserViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, Constants.DialogHostId);
        }

        [RelayCommand]
        private async Task ShowDialogContent(UserControl dialogContent)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialogContent, Constants.DialogHostId);
        }

        [RelayCommand]
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(Constants.DialogHostId);
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
                await LoadData();
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
                if (viewModel.Role == RoleType.Agent.ToString().ToLower())
                {
                    var groups = (await userGroupService.GetAll(new UserGroupDto { AgentAccount = viewModel.Account })).Data?.ToList() ?? new List<UserGroupDto>();
                    if (groups.Count > 0)
                    {
                        await userGroupService.DeleteBatch(groups.Select(c => c.Id).ToList());
                    }

                    var agentUsers = (await userService.GetAll(new UserDto { AgentAccount = viewModel.Account })).Data?.ToList() ?? new List<UserDto>();
                    foreach (var item in agentUsers)
                    {
                        item.AgentAccount = null;
                        await userService.Save(item);
                    }
                }

                if (viewModel.Role == RoleType.User.ToString().ToLower())
                {
                    var deviceManageViewModel = App.ServicesProvider.GetRequiredService<DeviceManageViewModel>();
                    deviceManageViewModel.CurrentUser = viewModel;
                    await deviceManageViewModel.LoadData();
                    foreach (var device in deviceManageViewModel.Devices)
                    {
                        await deviceManageViewModel.DeleteDeviceCommand.ExecuteAsync(device);                       
                    }

                    foreach (var deviceGroup in deviceManageViewModel.DeviceGroups)
                    {
                        await deviceManageViewModel.DeleteGroupCommand.ExecuteAsync(deviceGroup);
                    }

                    var mediaManageViewModel = App.ServicesProvider.GetRequiredService<MediaManageViewModel>();
                    mediaManageViewModel.CurrentUser = viewModel;
                    await mediaManageViewModel.LoadData();
                    foreach (var media in mediaManageViewModel.Medias)
                    {
                        await mediaManageViewModel.DeleteMediaCommand.ExecuteAsync(media);
                    }

                    foreach (var programGroup in mediaManageViewModel.MediaGroups)
                    {
                        await mediaManageViewModel.DeleteGroupCommand.ExecuteAsync(programGroup);
                    }
                }
                
                var fileService = GetService<IFileService>();
                fileService.DeleteResourcePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, viewModel.Account, viewModel.Name));
            }

            await LoadData();
        }

        [RelayCommand]
        private async Task DeleteUserBatch()
        {
            var selectedIds = Users.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await userService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
                foreach (var item in Users.Where(c => c.IsSelected && c.Role == "agent"))
                {
                    var groups = (await userGroupService.GetAll(new UserGroupDto { AgentAccount = item.Account })).Data?.ToList() ?? new List<UserGroupDto>();
                    if (groups.Count > 0)
                    {
                        await userGroupService.DeleteBatch(groups.Select(c => c.Id).ToList());
                    }

                    var agentUsers = (await userService.GetAll(new UserDto { AgentAccount = item.Account })).Data?.ToList() ?? new List<UserDto>();
                    foreach (var user in agentUsers)
                    {
                        user.AgentAccount = null;
                        await userService.Save(user);
                    }
                }

                await LoadData();
            }
        }

        [RelayCommand]
        private async Task SaveGroup(UserGroupViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }
            var response = await userGroupService.Save(viewModel.ToModel());
            if(response.Code == 200)
            {
                await LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task ChangeGroup(UserGroupViewModel viewModel)
        {
            var selectedUsers = Users.Where(c => c.IsSelected);

            foreach (var user in selectedUsers)
            {
                if (CurrentUser.Role == "agent")
                {
                    user.AgentUserGroupId = viewModel.Id;                    
                }
                else
                {
                    user.AdminUserGroupId = viewModel.Id;
                }

                var response = await userService.Save(user.ToModel());
                if (response.Code == 200)
                {
                }
            }

            await LoadData();
            CloseDialog();
        }

        [RelayCommand]
        private async Task DeleteGroup(UserGroupViewModel viewModel)
        {
            var response = await userGroupService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                if (CurrentUser.Role == RoleType.Agent.ToString().ToLower())
                {
                    var agentUsers = (await userService.GetAll(new UserDto { AgentUserGroupId = viewModel.Id })).Data?.ToList() ?? new List<UserDto>();
                    foreach (var item in agentUsers)
                    {
                        item.AgentUserGroupId = null;
                        await userService.Save(item);
                    }
                }
                if (CurrentUser.Role == RoleType.Admin.ToString().ToLower())
                {
                    var agentUsers = (await userService.GetAll(new UserDto { AdminUserGroupId = viewModel.Id })).Data?.ToList() ?? new List<UserDto>();
                    foreach (var item in agentUsers)
                    {
                        item.AdminUserGroupId = null;
                        await userService.Save(item);
                    }
                }
            }

            await LoadData();
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var query = new UserDto
            {
                AgentAccount = CurrentUser.Role == "agent" ? CurrentUser.Account : null,
                Account = SearchString
            };

            if (CurrentUser.Role == "agent")
            {
                query.AgentUserGroupId = groupId;
            }
            else
            {
                query.AdminUserGroupId = groupId;
            }
            var results = (await userService.GetAll(query, true)).Data?.ToList() ?? new List<UserDto>();
            this.Users = new ObservableCollection<UserViewModel>(results.OrderByDescending(c => c.Id).Select(c =>
            {
                var viewModel = new UserViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));

            await Task.CompletedTask;
        }
    }
}
