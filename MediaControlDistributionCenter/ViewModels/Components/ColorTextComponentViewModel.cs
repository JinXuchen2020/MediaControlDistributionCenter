using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Models;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MediaControlDistributionCenter.ViewModels
{

    public partial class ColorTextComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "ColorText";

        public List<ComponentEffect> ColorTextEffects => new List<ComponentEffect>
        {
            new ComponentEffect()
            {
                Key = "ColorText",
                Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_110"),
            },
        };

        [ObservableProperty]
        private Color background;

        [ObservableProperty]
        public FontFamily fontFamily;

        [ObservableProperty]
        public int fontSize;

        [ObservableProperty]
        public bool isBold;

        [ObservableProperty]
        private FontWeight fontWeight;

        [ObservableProperty]
        public bool isItalic;

        [ObservableProperty]
        private FontStyle fontStyle;

        [ObservableProperty]
        public bool isUnderline;

        [ObservableProperty]
        private TextDecorationCollection textDecoration;

        [ObservableProperty]
        public double letterSpacing;

        [ObservableProperty]
        public string componentEffect;

        [ObservableProperty]
        public string direction;

        [ObservableProperty]
        public int rollingSpeed;

        [ObservableProperty]
        private bool isLoopEnabled;

        [ObservableProperty]
        private ObservableCollection<FontFamily> fontFamilis;

        private DispatcherTimer? _timer;

        private FrameworkElement RunningElement;

        public ColorTextComponentViewModel(ColorTextComponent component, double ratio = 1): base(component, ratio)
        {
            direction = component.Direction;
            componentEffect = component.ComponentEffect;
            rollingSpeed = component.RollingSpeed;
            isLoopEnabled = component.IsLoopEnabled;
            fontFamilis = new ObservableCollection<FontFamily>(LoadFonts());
            fontFamily = string.IsNullOrEmpty(component.FontFamily) ? fontFamilis.First() : new FontFamily(component.FontFamily);
            fontSize = component.FontSize;
            isBold = component.IsBold;
            isItalic = component.IsItalic;
            isUnderline = component.IsUnderline;
            letterSpacing = component.LetterSpacing;
            background = (Color)ColorConverter.ConvertFromString(component.Background);
            fontWeight = component.IsBold ? FontWeights.Bold : FontWeights.Normal;
            fontStyle = component.IsItalic ? FontStyles.Italic : FontStyles.Normal;
            textDecoration = component.IsUnderline ? TextDecorations.Underline : null;
        }

        public override ColorTextComponent ToModel(double ratio)
        {
            return new ColorTextComponent
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
                Direction = Direction,
                PlayCount = PlayCount,
                PlayDuration = PlayDuration,
                ComponentEffect = ComponentEffect,
                RollingSpeed = RollingSpeed == 0 ? 3 : RollingSpeed,
                LetterSpacing = LetterSpacing,
                IsLoopEnabled = IsLoopEnabled,
                FontFamily = FontFamily.ToString(),
                FontSize = FontSize,
                IsBold = IsBold,
                IsItalic = IsItalic,
                IsUnderline = IsUnderline,
            };
        }

        public static ColorTextComponentViewModel CreateInstance(int id)
        {
            return new ColorTextComponentViewModel(new ColorTextComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_110")}{id}",
                ZIndex = 1,
                Type = MediaType.ColorText,
                Source = $"{FindResource("LanguageKey_Code_HelloWorld")}",
                PlayCount = 1,
                PlayDuration = "00:00:05",
                ComponentEffect = "ColorText",
                Direction = "rollingLeft",
                Timeline = 5,
                Background = "black",
                IsLoopEnabled = true,
                LetterSpacing = 2,
                RollingSpeed = 3,
                FontSize = 12,
                IsBold = false,
                IsItalic = false,
                IsUnderline = false,
                FontFamily = "",                 
            });
        }

        protected override FrameworkElement DrawingContent()
        {
            TextBlock result = new()
            {
                Background = new SolidColorBrush(Colors.Black),
                Text = Source,
                Foreground = new System.Windows.Media.LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection()
                    {
                        new GradientStop(Colors.Red, 0),
                        new GradientStop(Colors.Orange, 0.5),
                        new GradientStop(Colors.Yellow, 1)
                    }
                },
                Effect = new DropShadowEffect()
                {
                    Color = Colors.Purple,
                    Direction = 320,
                    ShadowDepth = 10,
                    BlurRadius = 10,
                },
                TextWrapping = TextWrapping.Wrap,
            };

            Border border = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = Width,
                Height = Height,
                DataContext = this,
                Child = result
            };

            CreateBinding(result, TextBlock.TextProperty, nameof(Source));
            CreateBinding(result, TextBlock.BackgroundProperty, nameof(Background), new ColorToBrushConverter());
            CreateBinding(result, TextBlock.FontSizeProperty, nameof(FontSize));
            CreateBinding(result, TextBlock.FontFamilyProperty, nameof(FontFamily));
            CreateBinding(result, TextBlock.FontStyleProperty, nameof(FontStyle));
            CreateBinding(result, TextBlock.FontWeightProperty, nameof(FontWeight));
            CreateBinding(result, TextBlock.TextDecorationsProperty, nameof(TextDecoration));

            CreateBinding(border, FrameworkElement.WidthProperty, nameof(Width));
            CreateBinding(border, FrameworkElement.HeightProperty, nameof(Height));

            Canvas.SetLeft(border, Left);
            Canvas.SetTop(border, Top);
            Canvas.SetZIndex(border, ZIndex);

            // 添加鼠标事件处理
            border.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            border.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            border.MouseMove += Element_MouseMove;
            border.MouseWheel += Element_MouseWheel;

            FrameworkElement = border;

            return border;
        }

        protected override FrameworkElement DrawingRunningContent()
        {
            TextBlock result = new()
            {
                Background = new SolidColorBrush(Colors.Black),
                Text = Source,
                Foreground = new System.Windows.Media.LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection()
                    {
                        new GradientStop(Colors.Red, 0),
                        new GradientStop(Colors.Orange, 0.5),
                        new GradientStop(Colors.Yellow, 1)
                    }
                },
                Effect = new DropShadowEffect()
                {
                    Color = Colors.Purple,
                    Direction = 320,
                    ShadowDepth = 10,
                    BlurRadius = 10,
                },
                FontSize = FontSize,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                TextDecorations = TextDecoration,
                FontFamily = FontFamily,
            };

            Border border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = Width * Ratio,
                Height = Height * Ratio,
                Child = result
            };

            IsRunningLoaded = false;
            result.Loaded += (sender, e) =>
            {
                IsRunningLoaded = true;
                if (sender is TextBlock target)
                {
                    RunningElement = target;

                    InitializeTimer(target);
                }
            };
            result.Unloaded += (sender, e) =>
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
            };

            Canvas.SetLeft(border, Left * Ratio);
            Canvas.SetTop(border, Top * Ratio);
            Canvas.SetZIndex(border, ZIndex);

            return border;
        }

        public override void EffectExecution()
        {
            Scrolling(RunningElement);
        }

        private void Scrolling(FrameworkElement target)
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

        private void InitializeTimer(TextBlock target)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(Timeline);
            _timer.Tick += (s, e) => Timer_Tick(target);
            _timer.Start();
        }

        private void Timer_Tick(TextBlock target)
        {
            var mainCanvas = FindCanvasParent(target);
            mainCanvas.Children.Remove(target);
        }
    }
}
