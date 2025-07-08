using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    /// <summary>
    /// MediaPreview.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPreview : Window
    {
        private DispatcherTimer _timer;

        private DispatcherTimer _adtimer;

        private double _ratio;

        private double _oldRatio;

        private int currentPlayCount = 0;

        private int adPlayCount = 0;

        private int adPlayGap = 0;

        private MediaPageViewModel CurrentPage;

        private MediaPageViewModel? AdPage;

        private readonly MediaEditViewModel manageViewModel;

        public MediaPreview(MediaEditViewModel viewModel)
        {
            InitializeComponent();
            CurrentPage = viewModel.MediaConfig.Pages.First(c => !c.IsDeleted && c.Type == "normal");
            AdPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => !c.IsDeleted && c.Type == "ad");
            DataContext = viewModel;

            InitializeTimer();
            InitializeAdTimer();

            _oldRatio = viewModel.MediaConfig.Ratio;

            MainCanvas.Width = Width;
            MainCanvas.Height = Height;
            Height = MainCanvas.Height + 62 + 20;
            Width = MainCanvas.Width + 20;

            if (double.Parse(viewModel.CurrentMedia.Width) > double.Parse(viewModel.CurrentMedia.Height))
            {
                _ratio = MainCanvas.Width / double.Parse(viewModel.CurrentMedia.Width);
                MainCanvas.Height = double.Parse(viewModel.CurrentMedia.Height) / double.Parse(viewModel.CurrentMedia.Width) * MainCanvas.Width;
                Height = MainCanvas.Height + 62 + 20;
            }
            else
            {
                _ratio = MainCanvas.Height / double.Parse(viewModel.CurrentMedia.Height);
                MainCanvas.Width = double.Parse(viewModel.CurrentMedia.Width) / double.Parse(viewModel.CurrentMedia.Height) * MainCanvas.Height;
                Width = MainCanvas.Width + 20;
            }

            this.SizeChanged += MediaPreview_SizeChanged;
            //this.Loaded += MediaPreview_Loaded;
            this.Unloaded += MediaPreview_Unloaded;

            this.manageViewModel = viewModel;
            this.manageViewModel.IsPreviewing = true;
        }

        private void MediaPreview_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeCanvasComponents();
            this.manageViewModel.IsPreviewing = false;
        }

        private void MediaPreview_Loaded(object sender, RoutedEventArgs e)
        {            
            LoadCanvasComponents(CurrentPage);
        }

        private void MediaPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        //    MainCanvas.Width = (sender as Window).Width;
        //    MainCanvas.Height = (sender as Window).Height;
        //    _ratio = double.Parse(manageViewModel.CurrentMedia.Width) > double.Parse(manageViewModel.CurrentMedia.Height) ? MainCanvas.Width / double.Parse(manageViewModel.CurrentMedia.Width) : MainCanvas.Height / double.Parse(manageViewModel.CurrentMedia.Height);

            LoadCanvasComponents(CurrentPage);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InitializeTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            var pageTimeline = CurrentPage.Components.Count == 0 ? 5 : CurrentPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
            _timer.Interval = TimeSpan.FromSeconds(pageTimeline);
            _timer.Tick += Timer_Tick; ;
            _timer.Start();
            currentPlayCount++;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var viewModel = (MediaEditViewModel)DataContext;
            if (currentPlayCount < CurrentPage.PlayCount)
            {
                LoadCanvasComponents(CurrentPage);
                InitializeTimer();
            }
            else
            {
                currentPlayCount = 0;
                var nextPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order > CurrentPage.Order && !c.IsDeleted && c.Type == "normal");
                if (nextPage == null)
                {
                    nextPage = viewModel.MediaConfig.Pages.First(c => !c.IsDeleted && c.Type == "normal");
                }

                CurrentPage = nextPage;
                LoadCanvasComponents(CurrentPage);
                InitializeTimer();
            }
        }

        private async void InitializeAdTimer()
        {
            if (_adtimer != null)
            {
                _adtimer.Stop();
            }

            if (AdPage != null)
            {
                var pageTimeline = AdPage.Components.Count == 0 ? 5 : CurrentPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
                int delayTime = 0;
                if (AdPage.AdPlayMode == "perday")
                {
                    delayTime = 24 * 60 / AdPage.PlayGap;
                }

                if (AdPage.AdPlayMode == "perhour")
                {
                    delayTime = 60 / AdPage.PlayGap;
                }

                delayTime = delayTime * 60 - (int)pageTimeline * AdPage.PlayCount;
                await Task.Delay(delayTime * 1000);
                _adtimer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(pageTimeline);
                _timer.Tick += AdTimer_Tick;
                _timer.Start();
                adPlayGap++;
            }
        }

        private void AdTimer_Tick(object? sender, EventArgs e)
        {
            if(AdPage == null)
            {
                return;
            }
            var viewModel = (MediaEditViewModel)DataContext;
            if (adPlayCount < AdPage.PlayCount)
            {
                LoadCanvasComponents(AdPage);
                adPlayCount++;
            }
            else
            {
                adPlayCount = 0;
                if(adPlayGap < AdPage.PlayGap)
                {
                    InitializeAdTimer();
                }

                var nextPage = viewModel.MediaConfig.Pages.OrderByDescending(c=>c.Order).FirstOrDefault(c => c.Order < AdPage.Order && !c.IsDeleted && c.Type == "normal");
                if (nextPage == null)
                {
                    nextPage = viewModel.MediaConfig.Pages.First(c => !c.IsDeleted && c.Type == "normal");
                }

                CurrentPage = nextPage;
                LoadCanvasComponents(CurrentPage);
                InitializeTimer();
            }
        }

        private void LoadCanvasComponents(MediaPageViewModel mediaPage)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            MainCanvas.Visibility = Visibility.Collapsed;
            MainCanvas.Children.Clear();
            var effectComponents = new List<BaseComponentViewModel>();
            foreach (var component in mediaPage.Components.Where(c => !c.IsDeleted))
            {
                if (component == null) continue;
                component.Ratio = _oldRatio * _ratio;
                switch (component.Type)
                {
                    case "Image":
                        var imageComponent = component as ImageComponentViewModel;
                        imageComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        effectComponents.Add(imageComponent);
                        break;
                    case "Video":
                        var videoComponent = component as VideoComponentViewModel;
                        videoComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        break;
                    case "Text":
                        var textComponent = component as TextComponentViewModel;
                        textComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        effectComponents.Add(textComponent);
                        break;
                    case "Web":
                        var webComponent = component as WebComponentViewModel;
                        webComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        break;
                    case "Stream":
                        var streamComponent = component as StreamComponentViewModel;
                        streamComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        break;
                    case "Rss":
                        var rssComponent = component as RssComponentViewModel;
                        rssComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        effectComponents.Add(rssComponent);
                        break;
                    case "Hdmi":
                        var hdmiComponent = component as HdmiComponentViewModel;
                        hdmiComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        break;
                    case "Word":
                        var wordComponent = component as WordComponentViewModel;
                        wordComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        break;
                    case "ColorText":
                        var colorTextComponent = component as ColorTextComponentViewModel;
                        colorTextComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                        break;
                }

                component.Ratio = _oldRatio;
            }

            this.Dispatcher.Invoke(async () =>
            {
                while (mediaPage.Components.Where(c => !c.IsDeleted).Any(c => !c.IsRunningLoaded))
                {
                    await Task.Delay(1000);
                }

                LoadingOverlay.Visibility = Visibility.Collapsed;
                MainCanvas.Visibility = Visibility.Visible;

                foreach (var component in mediaPage.Components.Where(c => !c.IsDeleted))
                {
                    component.EffectExecution();                    
                }
            });
        }

        private void DisposeCanvasComponents()
        {
            MainCanvas.Children.Clear();
            _timer?.Stop();
            _adtimer?.Stop();
        }

        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
