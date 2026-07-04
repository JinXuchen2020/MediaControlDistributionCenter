using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaEditViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        public FrameworkElement? SelectedElement { get; set; }

        public List<SchedulerDayViewModel> SchedulerDays { get; set; }

        public IEditorSurface? Surface { get; set; }

        [ObservableProperty]
        private string selectedType = "All";

        [ObservableProperty]
        private double canvasRatio;

        [ObservableProperty]
        private ProgramViewModel currentMedia;

        [ObservableProperty]
        private MediaConfigViewModel mediaConfig;

        [ObservableProperty]
        private MediaPageViewModel selectedPage;

        [ObservableProperty]
        private BaseComponentViewModel selectedComponent;

        [ObservableProperty]
        private ObservableCollection<MediaViewModel> medias;

        [ObservableProperty]
        private bool isPreviewing;

        private readonly IFileService fileService;
        private readonly IProgramService programService;
        private readonly IMediaService mediaService;

        public MediaEditViewModel()
        {
            this.fileService = GetService<IFileService>();
            this.programService = GetService<IProgramService>();
            this.mediaService = GetService<IMediaService>();
        }

        public override async Task LoadData()
        {
            MediaConfig? config = null;
            var surface = Surface;
            if (surface == null) return;

            double ratio = surface.Width / double.Parse(CurrentMedia.Width);

            if (double.Parse(CurrentMedia.Width) > double.Parse(CurrentMedia.Height))
            {
                ratio = surface.Width / double.Parse(CurrentMedia.Width);
                surface.Height = double.Parse(CurrentMedia.Height) / double.Parse(CurrentMedia.Width) * surface.Width;
            }
            else
            {
                ratio = surface.Height / double.Parse(CurrentMedia.Height);
                surface.Width = double.Parse(CurrentMedia.Width) / double.Parse(CurrentMedia.Height) * surface.Height;
            }

            ratio = 1;
            surface.Width = double.Parse(CurrentMedia.Width);
            surface.Height = double.Parse(CurrentMedia.Height);

            surface.Width = CanvasRatio * surface.Width;
            surface.Height = CanvasRatio * surface.Height;

            if (Directory.Exists(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, CurrentMedia.Name)))
            {
                config = fileService.ReadFileContent<MediaConfig>(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, CurrentMedia.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
                if (config != null)
                {
                    config.Program = CurrentMedia.ToModel();
                    config.Ratio = ratio;
                }
            }

            config ??= new MediaConfig
            {
                Ratio = ratio,
                Program = CurrentMedia.ToModel(),
                Pages =
                [
                    new() {
                        Id = 1,
                        Order = 1,
                        Type = "normal",
                        PlayCount = 1,
                        PlayGap = 10,
                        AdPlayMode = "perday",
                        Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Page")}{1}",
                        Schedulers = [new Scheduler { Id = 1, ScheduleDays = [1, 2, 3, 4, 5, 6, 7] }],
                        Components = []
                    }
                ]
            };
            this.MediaConfig = new MediaConfigViewModel(config);
            this.MediaConfig.Program = CurrentMedia;

            SelectedPage = this.MediaConfig.Pages.FirstOrDefault();
            if (SelectedPage != null)
            {
                SelectedPage.IsSelected = true;
            }

            var medias = (await mediaService.GetAll(null)).Data?.ToList() ?? new List<MediaDto>();
            var mediaList = new List<MediaViewModel>();
            foreach (var c in medias.OrderByDescending(t => t.Id))
            {
                var result = new MediaViewModel();
                result.Binding(c);

                await result.GetBitmap();
                mediaList.Add(result);
            }

            this.Medias = new ObservableCollection<MediaViewModel>(mediaList);
        }

        public async Task RefreshMedias()
        {
            var type = SelectedType == "All" ? null : SelectedType;
            var medias = (await mediaService.GetAll(new MediaDto { Type = type })).Data?.ToList() ?? new List<MediaDto>();
            var mediaList = new List<MediaViewModel>();
            foreach (var c in medias.OrderByDescending(t => t.Id))
            {
                var result = new MediaViewModel();
                result.Binding(c);

                await result.GetBitmap();
                mediaList.Add(result);
            }

            this.Medias = new ObservableCollection<MediaViewModel>(mediaList);
        }

        public BaseComponentViewModel? CreateComponent(MediaType type, int id)
        {
            switch (type)
            {
                case MediaType.Video:
                    return VideoComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Image:
                    return ImageComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Text:
                    return TextComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Web:
                    return WebComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Stream:
                    return StreamComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Hdmi:
                    return HdmiComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Rss:
                    return RssComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.Word:
                    return WordComponentViewModel.CreateInstance(CurrentUser.Account, id);
                case MediaType.ColorText:
                    return ColorTextComponentViewModel.CreateInstance(CurrentUser.Account, id);
                default:
                    return null;
            }
        }

        public void PrepareComponentDefaults(BaseComponentViewModel component)
        {
            var maxW = double.Parse(CurrentMedia.Width);
            var maxH = double.Parse(CurrentMedia.Height);
            switch (component.Type)
            {
                case "Text":
                    component.Width = Math.Min(maxW, 300 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 200 / component.Ratio / CanvasRatio);
                    break;
                case "Web":
                    component.Width = Math.Min(maxW, 220 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 220 / component.Ratio / CanvasRatio);
                    break;
                case "Stream":
                    component.Width = Math.Min(maxW, 220 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 220 / component.Ratio / CanvasRatio);
                    break;
                case "Hdmi":
                    component.Width = Math.Min(maxW, 241 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 160 / component.Ratio / CanvasRatio);
                    break;
                case "Rss":
                    component.Width = Math.Min(maxW, 200 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 200 / component.Ratio / CanvasRatio);
                    break;
                case "Word":
                    component.Width = Math.Min(maxW, 229 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 329 / component.Ratio / CanvasRatio);
                    break;
                case "ColorText":
                    component.Width = Math.Min(maxW, 300 / component.Ratio / CanvasRatio);
                    component.Height = Math.Min(maxH, 200 / component.Ratio / CanvasRatio);
                    break;
            }
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var type = SelectedType == "All" ? null : SelectedType;
            var medias = (await mediaService.GetAll(new MediaDto { Type = type, Name = SearchString }, true)).Data?.ToList() ?? new List<MediaDto>();
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
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, DialogHostId);
        }

        [RelayCommand]
        private async Task ShowDialogContent(UserControl dialogContent)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialogContent, Constants.DialogHostId);
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

        [RelayCommand]
        private void Capture()
        {
            if (SelectedPage == null || Surface == null) return;

            var snapshot = Surface.CaptureSnapshot();
            if (snapshot == null) return;

            var fileService = App.ServicesProvider.GetRequiredService<IFileService>();
            var filePath = Path.Combine(Constants.OutPath, CurrentUser.Account, CurrentMedia.Name, SelectedPage.Name);
            var fileName = "thumbnail.png";
            using var memoryStream = new MemoryStream(snapshot);
            filePath = fileService.SaveFileContent(filePath, fileName, memoryStream);

            SelectedPage.ThumbnailFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            SelectedPage.Thumbnail = GetBitmap(SelectedPage.ThumbnailFilePath);
        }

        private BitmapImage? GetBitmap(string? source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(source);
            bitmap.EndInit();

            return bitmap;
        }
    }
}
