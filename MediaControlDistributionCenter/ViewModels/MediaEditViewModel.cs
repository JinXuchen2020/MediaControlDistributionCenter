using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaEditViewModel : ObservableObject
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        public FrameworkElement? SelectedElement { get; set; }

        public List<SchedulerDayViewModel> SchedulerDays { get; set; }

        [ObservableProperty]
        private MediaViewModel currentMedia;

        [ObservableProperty]
        private MediaConfigViewModel mediaConfig;

        [ObservableProperty]
        private MediaPageViewModel selectedPage;

        [ObservableProperty]
        private BaseComponentViewModel selectedComponent;

        private readonly IFileService fileService;
        private readonly IProgramService programService;

        public MediaEditViewModel(MediaManageViewModel mediaManageViewModel, IFileService fileService, IProgramService programService,)
        {
            CurrentMedia = mediaManageViewModel.SelectedMedia;
            CurrentUser = mediaManageViewModel.CurrentUser;
            ShowNavigation = mediaManageViewModel.ShowNavigation;
            this.fileService = fileService;
            this.programService = programService;
        }

        public void SetValues(Canvas canvas)
        {
            MediaConfig? config = null;
            if (Directory.Exists(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentMedia.Name)))
            {
                config = fileService.ReadFileContent<MediaConfig>(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentMedia.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
                if (config != null)
                {
                    config.Width = string.IsNullOrEmpty(CurrentMedia.Width) ? 0 : double.Parse(CurrentMedia.Width);
                    config.Height = string.IsNullOrEmpty(CurrentMedia.Height) ? 0 : double.Parse(CurrentMedia.Height);
                    config.Name = CurrentMedia.Name;
                    config.Ratio = canvas.Width / double.Parse(CurrentMedia.Width);
                }
            }

            config ??= new MediaConfig
            {
                Id = CurrentMedia.Id,
                Name = CurrentMedia.Name,
                Width = string.IsNullOrEmpty(CurrentMedia.Width) ? 0 : double.Parse(CurrentMedia.Width),
                Height = string.IsNullOrEmpty(CurrentMedia.Height) ? 0 : double.Parse(CurrentMedia.Height),
                Ratio = canvas.Width / double.Parse(CurrentMedia.Width),
                Pages = new List<MediaPage>()
            };
            this.MediaConfig = new MediaConfigViewModel(config);

            SelectedPage = this.MediaConfig.Pages.FirstOrDefault();
            if (SelectedPage != null)
            {
                SelectedPage.IsSelected = true;
            }
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
        private void Dispose()
        {
            MediaConfig.DisposeCommand.Execute(null);
        }

        [RelayCommand]
        private async Task Save()
        {
            var response = await programService.Save(CurrentMedia.ToModel());
            if (response.Code == 200)
            {
            }
        }
    }
}
