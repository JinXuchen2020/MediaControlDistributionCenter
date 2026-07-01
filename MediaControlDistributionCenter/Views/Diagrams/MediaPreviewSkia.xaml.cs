using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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
        private readonly SkiaRenderEngine _renderEngine;
        private readonly AnimationEngine _animationEngine;
        private readonly RenderableRegistry _registry;
        private DateTime _lastFrameTime;
        private bool _isRunning;
        private readonly FpsCounter _fpsCounter = new();

        public MediaPreviewSkia(MediaEditViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            CurrentPage = viewModel.MediaConfig.Pages.First(c => !c.IsDeleted && c.Type == "normal");
            AdPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => !c.IsDeleted && c.Type == "ad");
            DataContext = viewModel;

            _oldRatio = viewModel.MediaConfig.Ratio;
            _animationEngine = new AnimationEngine();
            _renderEngine = new SkiaRenderEngine(_animationEngine);
            _registry = new RenderableRegistry();

            InitializeFactories();
            InitializeCanvasSize();

            InitializeTimer();
            InitializeAdTimer();

            this.Unloaded += MediaPreviewSkia_Unloaded;
            _viewModel.IsPreviewing = true;
            _isRunning = true;
            _lastFrameTime = DateTime.UtcNow;

            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F11)
                {
                    _fpsCounter.IsVisible = !_fpsCounter.IsVisible;
                    if (_fpsCounter.IsVisible) _fpsCounter.Reset();
                    e.Handled = true;
                }
            };

            CompositionTarget.Rendering += OnRendering;
        }

        private void InitializeFactories()
        {
            _registry.Register(new ImageComponentFactory());
            _registry.Register(new ColorTextComponentFactory());
            _registry.Register(new TextComponentFactory());
            _registry.Register(new RssComponentFactory());
            _registry.Register(new WordComponentFactory());
            _registry.Register(new VideoComponentFactory());
            _registry.Register(new WebComponentFactory());
            _registry.Register(new StreamComponentFactory());
            _registry.Register(new HdmiComponentFactory());
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

            var now = DateTime.UtcNow;
            float deltaSeconds = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;

            if (deltaSeconds > 0.1f)
                deltaSeconds = 0.016f;

            _fpsCounter.Update(deltaSeconds);
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isRunning) return;

            var canvas = e.Surface.Canvas;
            float deltaSeconds = (float)(DateTime.UtcNow - _lastFrameTime).TotalSeconds;
            if (deltaSeconds > 0.1f) deltaSeconds = 0.016f;

            canvas.Clear(new SKColor(0xFF, 0xFF, 0xFF));
            _renderEngine.RenderFrame(canvas, deltaSeconds);

            _fpsCounter.Draw(canvas, (float)SkCanvas.ActualWidth);
        }

        private void MediaPreviewSkia_Unloaded(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            CompositionTarget.Rendering -= OnRendering;
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

        private async void InitializeAdTimer()
        {
            if (_adtimer != null)
            {
                _adtimer.Stop();
            }

            if (AdPage != null)
            {
                var pageTimeline = AdPage.Components.Count == 0
                    ? 5
                    : CurrentPage.Components.Select(c => c.Timeline * c.PlayCount).Max();
                int delayTime = 0;
                if (AdPage.AdPlayMode == "perday")
                    delayTime = 24 * 60 / AdPage.PlayGap;
                if (AdPage.AdPlayMode == "perhour")
                    delayTime = 60 / AdPage.PlayGap;

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
                    InitializeAdTimer();

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
            _renderEngine.Clear();
            _animationEngine.StopAll();

            foreach (var component in mediaPage.Components.Where(c => !c.IsDeleted))
            {
                if (component == null) continue;
                component.Ratio = _oldRatio * _ratio;

                try
                {
                    if (_registry.CanCreate(component.Type))
                    {
                        var renderable = _registry.Create(component);
                        _renderEngine.AddRenderable(renderable);

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
                                _renderEngine.PlayAnimation(renderable, fade);
                            }
                        }
                    }
                }
                catch { }

                component.Ratio = _oldRatio;
            }

            this.Dispatcher.Invoke(async () =>
            {
                while (mediaPage.Components.Where(c => !c.IsDeleted)
                    .Any(c => !c.IsRunningLoaded))
                {
                    await Task.Delay(1000);
                }

                foreach (var component in mediaPage.Components.Where(c => !c.IsDeleted))
                {
                    component.EffectExecution();
                }
            });
        }

        private void DisposeCanvasComponents()
        {
            _renderEngine.Clear();
            _animationEngine.StopAll();
            _timer?.Stop();
            _adtimer?.Stop();
        }

        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
