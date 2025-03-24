using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Views;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaEditViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        public FrameworkElement? SelectedElement { get; set; }

        public List<SchedulerDayViewModel> SchedulerDays { get; set; }

        public Canvas Canvas { get; set; }

        [ObservableProperty]
        private ProgramViewModel currentMedia;

        [ObservableProperty]
        private MediaConfigViewModel mediaConfig;

        [ObservableProperty]
        private MediaPageViewModel selectedPage;

        [ObservableProperty]
        private BaseComponentViewModel selectedComponent;

        private readonly IFileService fileService;
        private readonly IProgramService programService;

        public MediaEditViewModel(IFileService fileService)
        {            
            this.fileService = fileService;
            this.programService = GetService<IProgramService>();
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override void LoadData()
        {
            MediaConfig? config = null;
            if (Directory.Exists(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, CurrentMedia.Name)))
            {
                config = fileService.ReadFileContent<MediaConfig>(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, CurrentMedia.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
                if (config != null)
                {
                    config.Width = string.IsNullOrEmpty(CurrentMedia.Width) ? 0 : double.Parse(CurrentMedia.Width);
                    config.Height = string.IsNullOrEmpty(CurrentMedia.Height) ? 0 : double.Parse(CurrentMedia.Height);
                    config.Name = CurrentMedia.Name;
                    config.UserAccount = CurrentMedia.UserId;
                    config.Ratio = Canvas.Width / double.Parse(CurrentMedia.Width);
                }
            }

            config ??= new MediaConfig
            {
                Id = CurrentMedia.Id,
                Name = CurrentMedia.Name,
                Width = string.IsNullOrEmpty(CurrentMedia.Width) ? 0 : double.Parse(CurrentMedia.Width),
                Height = string.IsNullOrEmpty(CurrentMedia.Height) ? 0 : double.Parse(CurrentMedia.Height),
                Ratio = Canvas.Width / double.Parse(CurrentMedia.Width),
                UserAccount = CurrentMedia.UserId,
                Pages = new List<MediaPage>()
            };
            this.MediaConfig = new MediaConfigViewModel(config);

            SelectedPage = this.MediaConfig.Pages.FirstOrDefault();
            if (SelectedPage != null)
            {
                SelectedPage.IsSelected = true;
            }
        }

        public BaseComponentViewModel? CreateComponent(MediaType type, int id)
        {
            switch (type)
            {
                case MediaType.Video:
                    return VideoComponentViewModel.CreateInstance(id);
                case MediaType.Image:
                    return ImageComponentViewModel.CreateInstance(id);
                case MediaType.Text:
                    return TextComponentViewModel.CreateInstance(id);
                case MediaType.Web:
                    return WebComponentViewModel.CreateInstance(id);
                case MediaType.Stream:
                    return StreamComponentViewModel.CreateInstance(id);
                case MediaType.Hdmi:
                    return HdmiComponentViewModel.CreateInstance(id);
                case MediaType.Rss:
                    return RssComponentViewModel.CreateInstance(id);
                default:
                    return null;
            }
        }

        public void DrawingComponent(Canvas canvas, BaseComponentViewModel component)
        {
            switch (component.Type)
            {
                case "Text":
                    var textComponent = (component as TextComponentViewModel)!;
                    textComponent.Width = 300;
                    textComponent.Height = 200;
                    textComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Web":
                    var webComponent = (component as WebComponentViewModel)!;
                    webComponent.Width = 220;
                    webComponent.Height = 220;
                    webComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Stream":
                    var streamComponent = (component as StreamComponentViewModel)!;
                    streamComponent.Width = 220;
                    streamComponent.Height = 220;
                    streamComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Hdmi":
                    var hdmiComponent = (component as HdmiComponentViewModel)!;
                    hdmiComponent.Width = 241;
                    hdmiComponent.Height = 160;
                    hdmiComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Rss":
                    var rssComponent = (component as RssComponentViewModel)!;
                    rssComponent.Width = 200;
                    rssComponent.Height = 200;
                    rssComponent.DrawContentCommand.Execute(canvas);
                    break;
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
