using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MediaControlDistributionCenter.ViewModels
{

    public partial class ColorTextComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "ColorText";

        [ObservableProperty]
        private Color background;

        [ObservableProperty]
        public string Background { get; set; }

        [ObservableProperty]
        public string FontFamily { get; set; }

        [ObservableProperty]
        public int FontSize { get; set; } = 12;

        [ObservableProperty]
        public bool IsBold { get; set; }

        [ObservableProperty]
        public bool IsItalic { get; set; }

        [ObservableProperty]
        public bool IsUnderline { get; set; }

        [ObservableProperty]
        public double LetterSpacing { get; set; }

        [ObservableProperty]
        public string ComponentEffect { get; set; }

        [ObservableProperty]
        public string Direction { get; set; }

        [ObservableProperty]
        public int RollingSpeed { get; set; }

        private DispatcherTimer? _timer;
        private int currentPlayCount = 0;

        public ColorTextComponentViewModel(ColorTextComponent component, double ratio = 1): base(component, ratio)
        {
            background = (Color)ColorConverter.ConvertFromString(component.Background);
            foreground = (Color)ColorConverter.ConvertFromString(component.TextColor);
            playMode = component.PlayMode;
            direction = component.Direction;
            effectDuration = component.EffectDuration;
            componentEffectKey = component.ComponentEffect;
            componentEffect = Effects.FirstOrDefault(c => c.Key == component.ComponentEffect)!.Name;
            rollingSpeed = component.RollingSpeed;
            textSize = component.TextSize;
            letterSpacing = component.LetterSpacing;
            lineSpacing = component.LineSpacing;
            isLoopEnabled = component.IsLoopEnabled;
        }

        public override TextComponent ToModel(double ratio)
        {
            return new TextComponent
            {
                Id = Id,
                Name = Name,
                ZIndex = ZIndex,
                Type = (MediaType)Enum.Parse(typeof(MediaType), Type),
                Left = Left / ratio,
                Top = Top / ratio,
                Width = Width / ratio,
                Height = Height / ratio,
                Source = Source,
                Timeline = Timeline,
                Background = Background.ToString(),
                TextColor = Foreground.ToString(),
                PlayMode = PlayMode,
                Direction = Direction,
                PlayCount = PlayCount,
                PlayDuration = PlayDuration,
                EffectDuration = EffectDuration,
                ComponentEffect = ComponentEffectKey,
                RollingSpeed = RollingSpeed == 0 ? 1 : RollingSpeed,
                TextSize = TextSize,
                LetterSpacing = LetterSpacing,
                LineSpacing = LineSpacing,
                IsLoopEnabled = IsLoopEnabled,
            };
        }

        public static TextComponentViewModel CreateInstance(int id)
        {
            return new TextComponentViewModel(new TextComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_105")}{id}",
                ZIndex = 1,
                Type = MediaType.Text,
                Source = $"{FindResource("LanguageKey_Code_HelloWorld")}",
                PlayCount = 1,
                PlayDuration = "00:00:05",
                PlayMode = "pageTurning",
                ComponentEffect = "FadeIn",
                EffectDuration = 1000,
                Direction = "rollingLeft",
                Timeline = 5,
                Background = "black",
                TextColor = "white",
                TextSize = 16,
                IsLoopEnabled = true,
                LetterSpacing = 2,
                LineSpacing = 2,
                RollingSpeed = 2,
            });
        }

        protected override FrameworkElement DrawingContent()
        {
            RichTextBox result = new()
            {
                Width = Width,
                Height = Height,
                AllowDrop = true,
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Black),
                DataContext = this,
            };

            Paragraph paragraph = new Paragraph();
            Run run = new Run(Source);

            CreateBinding(paragraph, Paragraph.LineHeightProperty, nameof(LineSpacing));

            CreateBinding(run, Run.TextProperty, nameof(Source));
            CreateBinding(run, Run.BackgroundProperty, nameof(Background), new ColorToBrushConverter());
            CreateBinding(run, Run.ForegroundProperty, nameof(Foreground), new ColorToBrushConverter());

            paragraph.Inlines.Add(run);
            result.Document.Blocks.Clear();
            result.Document.Blocks.Add(paragraph);

            CreateBinding(result, FrameworkElement.WidthProperty, nameof(Width));
            CreateBinding(result, FrameworkElement.HeightProperty, nameof(Height));
            CreateBinding(result, TextBlock.FontSizeProperty, nameof(TextSize));

            Canvas.SetLeft(result, Left);
            Canvas.SetTop(result, Top);
            Canvas.SetZIndex(result, ZIndex);

            // 添加鼠标事件处理
            result.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            result.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            result.MouseMove += Element_MouseMove;
            result.MouseWheel += Element_MouseWheel;
            result.Loaded += RichTextBox_Loaded;
            result.MouseDoubleClick += Result_MouseDoubleClick;
            result.LostFocus += Result_LostFocus;

            FrameworkElement = result;

            return result;
        }

        private void Result_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is RichTextBox richTextBox)
            {
                richTextBox.IsReadOnly = true;
                richTextBox.Focusable = false;
            }
        }

        private void Result_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is RichTextBox richTextBox)
            {
                richTextBox.IsReadOnly = false;
                richTextBox.Focusable = true;
                richTextBox.Focus();
            }
        }

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is RichTextBox richTextBox)
            {
                richTextBox.IsReadOnly = true;
                richTextBox.Focusable = false;
            }
        }

        protected override FrameworkElement DrawingRunningContent()
        {
            RichTextBox result = new()
            {
                Width = Width * Ratio,
                Height = Height * Ratio,
                IsReadOnly = true,
                Focusable = false,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden, // 隐藏垂直滚动条
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, // 隐藏水平滚动条
                Background = Brushes.Transparent, // 设置背景透明
                BorderThickness = new Thickness(0), // 去掉边框
                FontSize = TextSize != 0 ? TextSize : 12
            };

            Paragraph paragraph = new Paragraph();
            Run run = new Run(Source);
            run.Foreground = new SolidColorBrush(Foreground);
            run.Background = new SolidColorBrush(Background);
            paragraph.LineHeight = LineSpacing;
            paragraph.Inlines.Add(run);
            result.Document.Blocks.Clear();
            result.Document.Blocks.Add(paragraph);

            // 创建一个Canvas来放置RichTextBox
            Canvas canvas = new Canvas
            {
                Width = Width * Ratio,
                Height = Height * Ratio,
            };

            // 将RichTextBox添加到Canvas中
            canvas.Children.Add(result);

            if (PlayMode == FindResource("LanguageKey_Code_ProgramEdit_Tooltip_128"))
            {
                Scrolling(result);
            }

            IsRunningLoaded = false;
            result.Loaded += (sender, e) =>
            {
                IsRunningLoaded = true;
                if (sender is RichTextBox richTextBox)
                {
                    if (PlayMode == FindResource("LanguageKey_Code_ProgramEdit_Tooltip_127") && ComponentEffectKey != null)
                    {
                        Effects.Find(c => c.Key == ComponentEffectKey)?.Action(richTextBox);
                    }

                    InitializeTimer(richTextBox);
                }                
            };
            result.Unloaded += (sender, e) =>
            {
                currentPlayCount = 0;
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
            };

            Canvas.SetLeft(canvas, Left * Ratio);
            Canvas.SetTop(canvas, Top * Ratio);
            Canvas.SetZIndex(canvas, ZIndex);

            return canvas;
        }

        private void Scrolling(RichTextBox target)
        {
            // 创建一个TranslateTransform来控制滚动
            TranslateTransform translateTransform = new TranslateTransform();
            target.RenderTransform = translateTransform;

            // 创建一个Storyboard来控制滚动动画
            Storyboard storyboard = new Storyboard();

            // 创建DoubleAnimation来控制横向滚动
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                From = 300,
                To = -500, // 根据文本长度调整To的值
                Duration = new Duration(TimeSpan.FromSeconds(1)), // 滚动时间
                RepeatBehavior = RepeatBehavior.Forever
            };

            // 将DoubleAnimation添加到Storyboard中
            Storyboard.SetTarget(doubleAnimation, translateTransform);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("X"));

            storyboard.Children.Add(doubleAnimation);

            // 开始滚动动画
            storyboard.Begin();
        }

        public double GetFormattedTextWidth(TextPointer start, TextPointer end, System.Windows.Media.FontFamily fontFamily, double fontSize)
        {
            var textRange = new TextRange(start, end);
            var formattedText = new FormattedText(
                textRange.Text,
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black, 1.25
            );

            return formattedText.WidthIncludingTrailingWhitespace;
        }

        protected override void FadeIn(FrameworkElement element)
        {
            if(element is RichTextBox target)
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation
                {
                    From = 0.0, // 初始不透明度为0（完全透明）
                    To = 1.0,   // 最终不透明度为1（完全不透明）
                    Duration = new Duration(TimeSpan.FromMilliseconds(EffectDuration)) // 持续时间为2秒
                };

                // 将动画应用到Image的Opacity属性
                Storyboard.SetTarget(doubleAnimation, target);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(RichTextBox.OpacityProperty));

                // 将动画添加到Storyboard中
                storyboard.Children.Add(doubleAnimation);

                // 开始Storyboard
                storyboard.Begin();
            }            
        }
        private void InitializeTimer(RichTextBox target)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(Timeline);
            _timer.Tick += (s, e) => Timer_Tick(target);
            _timer.Start();
            currentPlayCount++;
        }

        private void Timer_Tick(RichTextBox target)
        {
            if (currentPlayCount < PlayCount)
            {
                if (PlayMode == FindResource("LanguageKey_Code_ProgramEdit_Tooltip_127") && ComponentEffectKey != null)
                {
                    Effects.Find(c => c.Key == ComponentEffectKey)?.Action(target);
                }

                currentPlayCount++;
            }
            else
            {
                var canvas = FindCanvasParent(target);
                canvas.Children.Remove(target);
                var mainCanvas = FindCanvasParent(canvas);
                mainCanvas.Children.Remove(canvas);
            }
        }
    }
}
