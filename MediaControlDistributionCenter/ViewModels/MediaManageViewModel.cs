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
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Data;

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

        private readonly IProgramGroupService programGroupService;
        private readonly IProgramService programService;

        public MediaManageViewModel(IProgramService programService, IProgramGroupService programGroupService) 
        {
            this.programService = programService;
            this.programGroupService = programGroupService;
        }

        public void SetValues(UserViewModel userViewModel, long? groupId)
        {
            CurrentUser = userViewModel;

            var groups = programGroupService.GetAll(new ProgramGroupDto { UserAccount = userViewModel.Account}).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramGroupDto>();
            groups.Insert(0, new ProgramGroupDto
            {
                Id = -1,
                Name = "全部",
                UserAccount = userViewModel.Account,
            });
            this.MediaGroups = new ObservableCollection<MediaGroupViewModel>(groups.Select(c=>
            {
                var result = new MediaGroupViewModel();
                result.Binding(c, c.Id == -1 ? true : false);
                return result;
            }));

            var medias = programService.GetAll(new ProgramDto { UserAccount = userViewModel.Account, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
            this.Medias = new ObservableCollection<MediaViewModel>(medias.Select(c =>
            {
                var result = new MediaViewModel();
                result.Binding(c);
                return result;
            }));
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
        private async Task CreateGroup(MediaGroupViewModel groupViewModel)
        {
            var response = await programGroupService.Save(groupViewModel.ToModel());
            if (response.Code == 200)
            {
                var groupModel = await programGroupService.GetAll(new ProgramGroupDto { Name = groupViewModel.Name, UserAccount = groupViewModel.UserId });
                groupViewModel.Id = groupModel.Data!.First().Id;

                MediaGroups.Add(groupViewModel);
            }
        }

        [RelayCommand]
        private async Task ChangeMediaStatus(MediaViewModel viewModel)
        {
            if (viewModel.Status == 1)
            {
                viewModel.Status = 0;
                viewModel.RackingBtnContent = "上架";
            }
            else
            {
                viewModel.Status = 1;
                viewModel.RackingBtnContent = "下架";
            }

            var response = await programService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
            }
        }

        [RelayCommand]
        private async Task DeleteMedia(MediaViewModel viewModel)
        {
            Medias.Remove(viewModel);
            var response = await programService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
            }
        }
    }
}
