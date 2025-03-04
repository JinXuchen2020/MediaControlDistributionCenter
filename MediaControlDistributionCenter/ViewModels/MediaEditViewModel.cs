using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        private bool isEditingName;

        [ObservableProperty]
        private MediaConfigViewModel mediaConfig;

        [ObservableProperty]
        private MediaPageViewModel selectedPage;

        [ObservableProperty]
        private BaseComponentViewModel selectedComponent;

        public MediaEditViewModel(MediaViewModel currentMedia, UserViewModel currentUser, MediaConfigViewModel mediaConfig)
        {
            this.currentMedia = currentMedia;
            CurrentUser = currentUser;
            this.mediaConfig = mediaConfig;

            SelectedPage = this.mediaConfig.Pages.FirstOrDefault();
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
        private async Task ShowDialogContent(UserControl dialogContent)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialogContent, DialogHostId);
        }

        [RelayCommand]
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(DialogHostId);
        }
    }
}
