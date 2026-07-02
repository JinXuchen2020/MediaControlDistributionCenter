using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using Serilog;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
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
        private int _currentPlayCount;
        private int _adPlayCount;
        private int _adPlayGap;
        private MediaPageViewModel _currentPage;
        private MediaPageViewModel? _adPage;
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
            _currentPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => !c.IsDeleted && c.Type == "normal")
                ?? throw new InvalidOperationException("No normal page found in media config");
            _adPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => !c.IsDeleted && c.Type == "ad");
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

            if (_controller.FpsCounter.IsVisible || _controller.RenderEngine.HasActiveAnimations)
            {
                _controller.UpdateDeltaTime();
                _controller.FpsCounter.Update(_controller.LastDeltaSeconds);
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
            var pageTimeline = _currentPage.Components.Count == 0
                ? 5
                : _currentPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
            _timer.Interval = TimeSpan.FromSeconds(pageTimeline);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            _currentPlayCount++;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_currentPlayCount < _currentPage.PlayCount)
            {
                LoadCanvasComponents(_currentPage);
                InitializeTimer();
            }
            else
            {
                _currentPlayCount = 0;
                var nextPage = _viewModel.MediaConfig.Pages
                    .FirstOrDefault(c => c.Order > _currentPage.Order && !c.IsDeleted && c.Type == "normal");
                nextPage ??= _viewModel.MediaConfig.Pages
                    .FirstOrDefault(c => !c.IsDeleted && c.Type == "normal");
                if (nextPage == null) return;
                _currentPage = nextPage;
                LoadCanvasComponents(_currentPage);
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

                if (_adPage != null)
                {
                    var pageTimeline = _adPage.Components.Count == 0
                        ? 5
                        : _adPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
                    int delayTime = 0;
                    if (_adPage.AdPlayMode == "perday" && _adPage.PlayGap > 0)
                        delayTime = 24 * 60 / _adPage.PlayGap;
                    if (_adPage.AdPlayMode == "perhour" && _adPage.PlayGap > 0)
                        delayTime = 60 / _adPage.PlayGap;

                    delayTime = delayTime * 60 - (int)pageTimeline * _adPage.PlayCount;
                    if (delayTime < 0) delayTime = 0;
                    await Task.Delay(delayTime * 1000).ConfigureAwait(false);
                    if (!_isRunning) return;
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        if (!_isRunning) return;
                        _adtimer = new DispatcherTimer();
                        _adtimer.Interval = TimeSpan.FromSeconds(pageTimeline);
                        _adtimer.Tick += AdTimer_Tick;
                        _adtimer.Start();
                        _adPlayGap++;
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
            if (_adPage == null) return;

            if (_adPlayCount < _adPage.PlayCount)
            {
                LoadCanvasComponents(_adPage);
                _adPlayCount++;
            }
            else
            {
                _adPlayCount = 0;
                if (_adPlayGap < _adPage.PlayGap)
                    _ = InitializeAdTimerAsync();
                else
                    _adPlayGap = 0;

                var nextPage = _viewModel.MediaConfig.Pages
                    .OrderByDescending(c => c.Order)
                    .FirstOrDefault(c => c.Order < _adPage.Order && !c.IsDeleted && c.Type == "normal");
                nextPage ??= _viewModel.MediaConfig.Pages
                    .FirstOrDefault(c => !c.IsDeleted && c.Type == "normal");
                if (nextPage == null) return;
                _currentPage = nextPage;
                LoadCanvasComponents(_currentPage);
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

            SkCanvas.InvalidateVisual();
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
