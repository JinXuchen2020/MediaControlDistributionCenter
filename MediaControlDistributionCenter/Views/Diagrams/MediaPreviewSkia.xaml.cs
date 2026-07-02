using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using Serilog;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    public partial class MediaPreviewSkia : Window
    {
        private DispatcherTimer _timer;
        private DispatcherTimer _adtimer;
        private double _ratio;
        private double _oldRatio;
        private int currentPlayCount;
        private int adPlayCount;
        private int adPlayGap;
        private MediaPageViewModel CurrentPage;
        private MediaPageViewModel? AdPage;
        private readonly MediaEditViewModel _viewModel;
        private readonly SkiaCanvasController _controller;
        private readonly IServiceProvider? _serviceProvider;
        private bool _isRunning;

        public MediaPreviewSkia(MediaEditViewModel viewModel) : this(viewModel, null)
        {
        }

        public MediaPreviewSkia(MediaEditViewModel viewModel, IServiceProvider? serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            _viewModel = viewModel;
            CurrentPage = viewModel.MediaConfig.Pages.First(c => !c.IsDeleted && c.Type == "normal");
            AdPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => !c.IsDeleted && c.Type == "ad");
            DataContext = viewModel;

            _oldRatio = viewModel.MediaConfig.Ratio;
            _controller = new SkiaCanvasController(serviceProvider);

            InitializeCanvasSize();

            InitializeTimer();
            _ = InitializeAdTimerAsync();

            this.Unloaded += MediaPreviewSkia_Unloaded;
            this.Closed += MediaPreviewSkia_Closed;
            _viewModel.IsPreviewing = true;
            _isRunning = true;

            this.PreviewKeyDown += OnPreviewKeyDown;

            CompositionTarget.Rendering += OnRendering;
        }

        private void InitializeCanvasSize()
        {
            SkCanvas.Width = Width - 40;
            SkCanvas.Height = Height - 100;

            if (double.TryParse(_viewModel.CurrentMedia.Width, out var mediaW) &&
                double.TryParse(_viewModel.CurrentMedia.Height, out var mediaH))
            {
                if (mediaW > mediaH)
                {
                    _ratio = (float)(SkCanvas.Width / mediaW);
                    SkCanvas.Height = mediaH / mediaW * SkCanvas.Width;
                }
                else
                {
                    _ratio = (float)(SkCanvas.Height / mediaH);
                    SkCanvas.Width = mediaW / mediaH * SkCanvas.Height;
                }
                Height = SkCanvas.Height + 62 + 30;
                Width = SkCanvas.Width + 40;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (!_isRunning) return;

            _controller.UpdateDeltaTime();
            _controller.FpsCounter.Update(_controller.LastDeltaSeconds);

            if (_controller.FpsCounter.IsVisible || _controller.RenderEngine.HasActiveAnimations)
            {
                SkCanvas.InvalidateVisual();
            }
        }

        private void SkCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isRunning) return;

            var canvas = e.Surface.Canvas;

            canvas.Clear(new SKColor(0xFF, 0xFF, 0xFF));
            _controller.RenderEngine.RenderFrame(canvas, _controller.LastDeltaSeconds);

            _controller.FpsCounter.Draw(canvas, (float)SkCanvas.ActualWidth);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                _controller.ToggleFps();
                e.Handled = true;
            }
        }

        private void MediaPreviewSkia_Unloaded(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            CompositionTarget.Rendering -= OnRendering;
            this.PreviewKeyDown -= OnPreviewKeyDown;
            DisposeCanvasComponents();
            _viewModel.IsPreviewing = false;
        }

        private void MediaPreviewSkia_Closed(object? sender, EventArgs e)
        {
            _isRunning = false;
            CompositionTarget.Rendering -= OnRendering;
            this.PreviewKeyDown -= OnPreviewKeyDown;
            DisposeCanvasComponents();
            _viewModel.IsPreviewing = false;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InitializeTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            var pageTimeline = CurrentPage.Components.Count == 0
                ? 5
                : CurrentPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
            _timer.Interval = TimeSpan.FromSeconds(pageTimeline);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            currentPlayCount++;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (currentPlayCount < CurrentPage.PlayCount)
            {
                LoadCanvasComponents(CurrentPage);
                InitializeTimer();
            }
            else
            {
                currentPlayCount = 0;
                var nextPage = _viewModel.MediaConfig.Pages
                    .FirstOrDefault(c => c.Order > CurrentPage.Order && !c.IsDeleted && c.Type == "normal");
                if (nextPage == null)
                {
                    nextPage = _viewModel.MediaConfig.Pages
                        .First(c => !c.IsDeleted && c.Type == "normal");
                }
                CurrentPage = nextPage;
                LoadCanvasComponents(CurrentPage);
                InitializeTimer();
            }
        }

        private async Task InitializeAdTimerAsync()
        {
            try
            {
                if (_adtimer != null)
                {
                    _adtimer.Stop();
                }

                if (AdPage != null)
                {
                    var pageTimeline = AdPage.Components.Count == 0
                        ? 5
                        : AdPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
                    int delayTime = 0;
                    if (AdPage.AdPlayMode == "perday")
                        delayTime = 24 * 60 / AdPage.PlayGap;
                    if (AdPage.AdPlayMode == "perhour")
                        delayTime = 60 / AdPage.PlayGap;

                    delayTime = delayTime * 60 - (int)pageTimeline * AdPage.PlayCount;
                    if (delayTime < 0) delayTime = 0;
                    await Task.Delay(delayTime * 1000).ConfigureAwait(false);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        _adtimer = new DispatcherTimer();
                        _adtimer.Interval = TimeSpan.FromSeconds(pageTimeline);
                        _adtimer.Tick += AdTimer_Tick;
                        _adtimer.Start();
                        adPlayGap++;
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Async ad timer initialization failed");
            }
        }

        private void AdTimer_Tick(object? sender, EventArgs e)
        {
            if (AdPage == null) return;

            if (adPlayCount < AdPage.PlayCount)
            {
                LoadCanvasComponents(AdPage);
                adPlayCount++;
            }
            else
            {
                adPlayCount = 0;
                if (adPlayGap < AdPage.PlayGap)
                    _ = InitializeAdTimerAsync();

                var nextPage = _viewModel.MediaConfig.Pages
                    .OrderByDescending(c => c.Order)
                    .FirstOrDefault(c => c.Order < AdPage.Order && !c.IsDeleted && c.Type == "normal");
                if (nextPage == null)
                {
                    nextPage = _viewModel.MediaConfig.Pages
                        .First(c => !c.IsDeleted && c.Type == "normal");
                }
                CurrentPage = nextPage;
                LoadCanvasComponents(CurrentPage);
                InitializeTimer();
            }
        }

        private void LoadCanvasComponents(MediaPageViewModel mediaPage)
        {
            _controller.RenderEngine.Clear();
            _controller.AnimationEngine.StopAll();

            float effectiveRatio = (float)(_oldRatio * _ratio);

            foreach (var component in mediaPage.Components.Where(c => !c.IsDeleted))
            {
                if (component == null) continue;

                try
                {
                    float savedRatio = (float)component.Ratio;
                    component.Ratio = effectiveRatio;

                    if (_controller.Registry.CanCreate(component.Type))
                    {
                        var renderable = _controller.Registry.Create(component);

                        component.Ratio = savedRatio;

                        _controller.RenderEngine.AddRenderable(renderable);

                        if (component.Effects.Any())
                        {
                            var effectKey = component.Effects
                                .FirstOrDefault(e => e.Key != "Empty")?.Key;
                            if (effectKey == "FadeIn")
                            {
                                var fade = new FadeInAnimation(
                                    (float)(component.EffectDuration > 0
                                        ? component.EffectDuration / 1000.0
                                        : 0.5));
                                _controller.RenderEngine.PlayAnimation(renderable, fade);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to create renderable for component type: {Type}", component.Type);
                }
            }
        }

        private void DisposeCanvasComponents()
        {
            _controller.Dispose();
            _timer?.Stop();
            _timer = null;
            _adtimer?.Stop();
            _adtimer = null;
        }

        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
