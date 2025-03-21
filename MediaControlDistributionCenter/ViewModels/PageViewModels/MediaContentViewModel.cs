using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels.PageViewModels
{
    public partial class MediaContentViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        [ObservableProperty]
        private ObservableCollection<MediaDto> medias;

        private readonly IMediaService mediaService;
        private readonly IFileService fileService;

        public MediaContentViewModel(LoginViewModel loginViewModel, IFileService fileService)
        {
            CurrentUser = loginViewModel.CurrentUser;

            this.mediaService = GetService<IMediaService>();
            this.fileService = fileService;
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override void LoadData()
        {
            var medias = mediaService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            this.Medias = new ObservableCollection<MediaDto>(medias);
        }

        public void GetData(string type)
        {
            var medias = mediaService.GetAll(new MediaDto { Type = type}).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            this.Medias = new ObservableCollection<MediaDto>(medias);
        }
    }
}
