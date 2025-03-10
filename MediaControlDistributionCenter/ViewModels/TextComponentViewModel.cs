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

    public partial class TextComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Text";

        [ObservableProperty]
        private Color background;

        [ObservableProperty]
        private Color foreground;

        [ObservableProperty]
        private string playMode; //"翻页"

        [ObservableProperty]
        private string direction; //"向右滚动"        

        [ObservableProperty]
        private int effectDuration; //特效时长    -毫秒   文本翻页时才用到

        [ObservableProperty]
        private string componentEffect; //入场特效               文本翻页时才用到     

        [ObservableProperty]
        private int rollingSpeed;  //滚动速度档位        一共1-10个档位

        [ObservableProperty]
        private double textSize; //20,                            //字体大小
        [ObservableProperty]
        private bool isLoopEnabled; // true,                //是否首尾相接
        [ObservableProperty]
        private double letterSpacing; //10",                  //字体间距
        [ObservableProperty]
        private double lineSpacing; //16", 

        private DispatcherTimer _timer;
        private int currentPlayCount = 0;

        public Dictionary<string, Action<RichTextBox>> Effects { get; set; }

        public TextComponentViewModel(TextComponent component, double ratio = 1): base(component, ratio)
        {
            background = (Color)ColorConverter.ConvertFromString(component.Background);
            foreground = (Color)ColorConverter.ConvertFromString(component.TextColor);
            playMode = component.PlayMode;
            direction = component.Direction;
            effectDuration = component.EffectDuration;
            componentEffect = component.ComponentEffect;
            rollingSpeed = component.RollingSpeed;
            textSize = component.TextSize;
            letterSpacing = component.LetterSpacing;
            lineSpacing = component.LineSpacing;
            isLoopEnabled = component.IsLoopEnabled;
            Effects = new Dictionary<string, Action<RichTextBox>>
            {
                { "向右扩展", FadeIn},
                { "向下扩展", FadeIn},
                { "向左扩展", FadeIn},
                { "向上扩展", FadeIn},
                { "中间向外扩展", FadeIn},
                { "左右扩展", FadeIn},
                { "上下扩展", FadeIn},
                { "向右平移", FadeIn},
                { "向下平移", FadeIn},
                { "向左平移", FadeIn},
                { "向上平移", FadeIn},
                { "向右压缩", FadeIn},
                { "向下压缩", FadeIn},
                { "向左压缩", FadeIn},
                { "向上压缩", FadeIn},
                { "上下压缩", FadeIn},
                { "左右压缩", FadeIn},
                { "向下展开卷轴", FadeIn},
                { "向上展开卷轴", FadeIn},
                { "水平百叶窗", FadeIn},
                { "垂直百叶窗", FadeIn},
                { "变焦全屏", FadeIn},
                { "轮子", FadeIn},
                { "上下齿合", FadeIn},
                { "淡入", FadeIn},
                { "向右堆积", FadeIn},
                { "向下堆积", FadeIn},
                { "向左堆积", FadeIn},
                { "向上堆积", FadeIn},
                { "左镭射", FadeIn},
                { "上镭射", FadeIn},
                { "右镭射", FadeIn},
                { "下镭射", FadeIn},
                { "向下展开", FadeIn},
                { "向上展开", FadeIn},
                { "上下展开", FadeIn},
                { "上下合并", FadeIn},
            };
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
                ComponentEffect = ComponentEffect,
                RollingSpeed = RollingSpeed == 0 ? 1 : RollingSpeed,
                TextSize = TextSize,
                LetterSpacing = LetterSpacing,
                LineSpacing = LineSpacing,
                IsLoopEnabled = IsLoopEnabled,
            };
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

            if (PlayMode == "滚动")
            {
                Scrolling(result);
            }

            result.Loaded += (sender, e) =>
            {
                if (sender is RichTextBox richTextBox)
                {
                    if (PlayMode == "翻页" && ComponentEffect != null)
                    {
                        Effects[ComponentEffect](richTextBox);
                    }
                }

                InitializeTimer(result);
            };
            result.Unloaded += (sender, e) =>
            {
                if (_timer != null)
                {
                    _timer.Stop();
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

        private void FadeIn(RichTextBox target)
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
            var canvas = FindCanvasParent(target);
            var mainCanvas = FindCanvasParent(canvas);
            mainCanvas.Children.Remove(canvas);
            if (currentPlayCount < PlayCount)
            {
                DrawingRunningContent();
            }
            else
            {
                _timer.Stop();
            }
        }

    }
}
