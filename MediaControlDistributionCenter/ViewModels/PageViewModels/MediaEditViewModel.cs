using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
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
using System.Windows.Media;
using Azure;

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
        private string selectedType = "All";

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

        private readonly IFileService fileService;
        private readonly IProgramService programService;
        private readonly IMediaService mediaService;

        public MediaEditViewModel(IFileService fileService)
        {            
            this.fileService = fileService;
            this.programService = GetService<IProgramService>();
            this.mediaService = GetService<IMediaService>();
        }

        public override void LoadData()
        {
            MediaConfig? config = null;
            if (Directory.Exists(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, CurrentMedia.Name)))
            {
                config = fileService.ReadFileContent<MediaConfig>(System.IO.Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, CurrentMedia.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
                if (config != null)
                {
                    config.Program = CurrentMedia.ToModel();
                    config.Ratio = Canvas.Width / double.Parse(CurrentMedia.Width);
                }
            }

            config ??= new MediaConfig
            {
                Ratio = Canvas.Width / double.Parse(CurrentMedia.Width),
                Program = CurrentMedia.ToModel(),
                Pages = new List<MediaPage>()
            };
            this.MediaConfig = new MediaConfigViewModel(config);
            this.MediaConfig.Program = CurrentMedia;

            SelectedPage = this.MediaConfig.Pages.FirstOrDefault();
            if (SelectedPage != null)
            {
                SelectedPage.IsSelected = true;
            }

            var medias = mediaService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            Medias = new ObservableCollection<MediaViewModel>(medias.Select(c =>
            {
                var result = new MediaViewModel();
                result.Binding(c);
                return result;
            }));
        }

        public void RefreshMedias()
        {
            var type = SelectedType == "All" ? null : SelectedType;
            var medias = mediaService.GetAll(new MediaDto { Type = type }).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
            this.Medias = new ObservableCollection<MediaViewModel>(medias.OrderByDescending(c => c.Id).Select(c =>
            {
                var result = new MediaViewModel();
                result.Binding(c);
                return result;
            }));
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
                case "Word":
                    var wordComponent = (component as WordComponentViewModel)!;
                    wordComponent.Width = 229;
                    wordComponent.Height = 329;
                    wordComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "ColorText":
                    var colorTextComponent = (component as ColorTextComponentViewModel)!;
                    colorTextComponent.Width = 300;
                    colorTextComponent.Height = 200;
                    colorTextComponent.DrawContentCommand.Execute(canvas);
                    break;
            }
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var type = SelectedType == "All" ? null : SelectedType;
            var medias = mediaService.GetAll(new MediaDto { Type = type, Name = SearchString }, true).GetAwaiter().GetResult().Data?.ToList() ?? new List<MediaDto>();
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
        private void Capture(Canvas canvas)
        {
            if(SelectedPage != null)
            {
                SelectedPage.ThumbnailFilePath = canvas.Dispatcher.Invoke<string>(() =>
                {
                    // 创建一个RenderTargetBitmap
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                        (int)canvas.ActualWidth,
                        (int)canvas.ActualHeight,
                        96, 96,
                        PixelFormats.Pbgra32);
                    //canvas.Measure(new Size(canvas.ActualWidth, canvas.ActualHeight));
                    //canvas.Arrange(new Rect(new Size(canvas.ActualWidth, canvas.ActualHeight)));

                    // 将MediaElement绘制到RenderTargetBitmap
                    renderTargetBitmap.Render(canvas);
                    PngBitmapEncoder png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    using var memoryStream = new MemoryStream();
                    png.Save(memoryStream);
                    var fileService = App.ServicesProvider.GetRequiredService<IFileService>();
                    var filePath = Path.Combine(Constants.OutPath, CurrentUser.Account, CurrentMedia.Name, SelectedPage.Name);
                    var fileName = "thumbnail.png";
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    filePath = fileService.SaveFileContent(filePath, fileName, memoryStream);

                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
                });

                SelectedPage.Thumbnail = GetBitmap(SelectedPage.ThumbnailFilePath);
            }
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
