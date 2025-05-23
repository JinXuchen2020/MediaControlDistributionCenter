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

        private readonly IFileService fileService;
        private readonly IProgramService programService;
        private readonly IMediaService mediaService;

        public MediaEditViewModel(IFileService fileService)
        {            
            this.fileService = fileService;
            this.programService = GetService<IProgramService>();
            this.mediaService = GetService<IMediaService>();
        }

        public override async Task LoadData()
        {
            MediaConfig? config = null;
            double ratio = Canvas.Width / double.Parse(CurrentMedia.Width);

            if (double.Parse(CurrentMedia.Width) > double.Parse(CurrentMedia.Height))
            {
                ratio = Canvas.Width / double.Parse(CurrentMedia.Width);
                Canvas.Height = double.Parse(CurrentMedia.Height) / double.Parse(CurrentMedia.Width) * Canvas.Width;
            }
            else
            {
                ratio = Canvas.Height / double.Parse(CurrentMedia.Height);
                Canvas.Width = double.Parse(CurrentMedia.Width) / double.Parse(CurrentMedia.Height) * Canvas.Height;
            }

            ratio = 1;
            Canvas.Width = double.Parse(CurrentMedia.Width);
            Canvas.Height = double.Parse(CurrentMedia.Height);

            Canvas.Width = CanvasRatio * Canvas.Width;
            Canvas.Height = CanvasRatio * Canvas.Height;

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

        public void DrawingComponent(Canvas canvas, BaseComponentViewModel component)
        {
            switch (component.Type)
            {
                case "Text":
                    var textComponent = (component as TextComponentViewModel)!;
                    textComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 300 / component.Ratio / CanvasRatio);
                    textComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 200 / component.Ratio / CanvasRatio);
                    textComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Web":
                    var webComponent = (component as WebComponentViewModel)!;
                    webComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 220 / component.Ratio / CanvasRatio);
                    webComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 220 / component.Ratio / CanvasRatio);
                    webComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Stream":
                    var streamComponent = (component as StreamComponentViewModel)!;
                    streamComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 220 / component.Ratio / CanvasRatio);
                    streamComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 220 / component.Ratio / CanvasRatio);
                    streamComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Hdmi":
                    var hdmiComponent = (component as HdmiComponentViewModel)!;
                    hdmiComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 241 / component.Ratio / CanvasRatio);
                    hdmiComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 160 / component.Ratio / CanvasRatio);
                    hdmiComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Rss":
                    var rssComponent = (component as RssComponentViewModel)!;
                    rssComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 200 / component.Ratio / CanvasRatio);
                    rssComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 200 / component.Ratio / CanvasRatio);
                    rssComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "Word":
                    var wordComponent = (component as WordComponentViewModel)!;
                    wordComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 229 / component.Ratio / CanvasRatio);
                    wordComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 329 / component.Ratio / CanvasRatio);
                    wordComponent.DrawContentCommand.Execute(canvas);
                    break;
                case "ColorText":
                    var colorTextComponent = (component as ColorTextComponentViewModel)!;
                    colorTextComponent.Width = Math.Min(double.Parse(CurrentMedia.Width), 300 / component.Ratio / CanvasRatio);
                    colorTextComponent.Height = Math.Min(double.Parse(CurrentMedia.Height), 200 / component.Ratio / CanvasRatio);
                    colorTextComponent.DrawContentCommand.Execute(canvas);
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
        private void Capture(Canvas canvas)
        {
            if(SelectedPage != null)
            {
                SelectedPage.ThumbnailFilePath = canvas.Dispatcher.Invoke<string>(() =>
                {
                    // 创建一个RenderTargetBitmap
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                        (int)canvas.Width,
                        (int)canvas.Height,
                        96, 96,
                        PixelFormats.Pbgra32);
                    //canvas.Measure(new Size(canvas.Width, canvas.Height));
                    canvas.Arrange(new Rect(new Size(canvas.Width, canvas.Height)));

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
