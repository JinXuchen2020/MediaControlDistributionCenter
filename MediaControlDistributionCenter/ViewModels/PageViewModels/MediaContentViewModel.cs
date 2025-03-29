using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaContentViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        public MediaGroupViewModel SelectedGroup { get; set; }

        [ObservableProperty]
        private ObservableCollection<MediaViewModel> medias;

        [ObservableProperty]
        private ObservableCollection<MediaGroupViewModel> mediaGroups;

        [ObservableProperty]
        private string selectedType = "All";

        [ObservableProperty]
        private int allCount;

        [ObservableProperty]
        private int videoCount;

        [ObservableProperty]
        private int imageCount;

        [ObservableProperty]
        private long? selectedGroupId;

        private readonly IMediaService mediaService;
        private readonly IMediaGroupService mediaGroupService;
        private readonly IFileService fileService;

        public MediaContentViewModel(LoginViewModel loginViewModel, IFileService fileService)
        {
            CurrentUser = loginViewModel.CurrentUser;

            this.mediaService = GetService<IMediaService>();
            this.mediaGroupService = GetService<IMediaGroupService>();
            this.fileService = fileService;
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override void LoadData()
        {
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var groups = mediaGroupService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaGroupDto> ();
            groups.Insert(0, new MediaGroupDto
            {
                Id = -1,
                Name = FindResource("LanguageKey_Code_All"),
            });
            this.MediaGroups = new ObservableCollection<MediaGroupViewModel>(groups.Select(c =>
            {
                var result = new MediaGroupViewModel();
                result.Binding(c, c.Id == (groupId ?? -1) ? true : false);
                return result;
            }));

            var type = SelectedType == "All" ? null : SelectedType;
            var medias = mediaService.GetAll(new MediaDto { Type = type, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            this.Medias = new ObservableCollection<MediaViewModel>(medias.OrderByDescending(c => c.Id).Select(c =>
            {
                var result = new MediaViewModel();
                result.Binding(c);
                return result;
            }));

            var allMedias = mediaService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            AllCount = allMedias.Count;
            VideoCount = allMedias.Where(c => c.Type == "Video").Count();
            ImageCount = allMedias.Where(c => c.Type == "Image").Count();
        }

        public MediaViewModel CreateMedia()
        {
            return new MediaViewModel
            {
                Type = "Video"
            };
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var type = SelectedType == "All" ? null : SelectedType;
            var medias = mediaService.GetAll(new MediaDto { Type = type, GroupId = groupId, Name = SearchString }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            this.Medias = new ObservableCollection<MediaViewModel>(medias.OrderByDescending(c => c.Id).Select(c =>
            {
                var viewModel = new MediaViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, Constants.DialogHostId);
        }

        [RelayCommand]
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(Constants.DialogHostId);
        }

        [RelayCommand]
        private async Task CreateGroup(MediaGroupViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }
            var response = await mediaGroupService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task DeleteGroup(MediaGroupViewModel viewModel)
        {
            var response = await mediaGroupService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                var agentUsers = mediaService.GetAll(new MediaDto { GroupId = viewModel.Id }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
                foreach (var item in agentUsers)
                {
                    item.GroupId = null;
                    await mediaService.Save(item);
                }
            }

            LoadData();
        }

        [RelayCommand]
        private async Task ChangeGroup()
        {
            var selectedItems = Medias.Where(c => c.IsSelected);

            foreach (var item in selectedItems)
            {
                item.GroupId = SelectedGroupId;
                var response = await mediaService.Save(item.ToModel());
                if (response.Code == 200)
                {
                }
            }

            SelectedGroupId = null;
            LoadData();
            CloseDialog();
        }

        [RelayCommand]
        private async Task SaveMedia(MediaViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (viewModel.HasErrors)
            {
                return;
            }
            var response = await mediaService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task DeleteMedias()
        {
            var selectedIds = Medias.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await mediaService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteMedia(MediaViewModel viewModel)
        {
            var response = await mediaService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                LoadData();
            }
        }
    }
}
