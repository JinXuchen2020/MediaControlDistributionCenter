using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class RssComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Rss";

        private int currentPlayCount = 0;
        private DispatcherTimer? _timer;

        private Dictionary<string, string> FieldList = new Dictionary<string, string>()
        {
            {"Title", FindResource("LanguageKey_Code_ProgramEdit_Tooltip_192") },
            {"PublishDate", FindResource("LanguageKey_Code_ProgramEdit_Tooltip_193") },
            {"Body", FindResource("LanguageKey_Code_ProgramEdit_Tooltip_194") }
        };

        [ObservableProperty]
        private int interval;

        [ObservableProperty]
        private ObservableCollection<RssContentViewModel> contents;

        [ObservableProperty]
        private ObservableCollection<FontFamily> fontFamilis;

        [ObservableProperty]
        private string playMode; //"翻页"

        [ObservableProperty]
        private string direction; //"向右滚动"        

        [ObservableProperty]
        private int effectDuration; //特效时长    -毫秒   文本翻页时才用到

        [ObservableProperty]
        private string componentEffect; //入场特效               文本翻页时才用到

        [ObservableProperty]
        private string componentEffectKey;

        [ObservableProperty]
        private int rollingSpeed;

        public RssComponentViewModel(RssComponent component, double ratio = 1) : base(component, ratio)
        {
            playMode = component.PlayMode;
            direction = component.Direction;
            effectDuration = component.EffectDuration;
            componentEffectKey = component.ComponentEffect;
            componentEffect = Effects.FirstOrDefault(c => c.Key == component.ComponentEffect)!.Name;
            rollingSpeed = component.RollingSpeed;
            interval = component.Interval;
            fontFamilis = new ObservableCollection<FontFamily>(LoadFonts());
            contents = new ObservableCollection<RssContentViewModel>(FieldList.Select(c =>
            {
                var content = component.Contents.FirstOrDefault(t => t.FieldName == c.Key);
                if (content != null)
                {
                    return new RssContentViewModel
                    {
                        FieldName = content.FieldName,
                        FieldNameContent = c.Value,
                        FontFamily = new FontFamily(content.FontFamily),
                        Fonts = fontFamilis,
                        FontColor = (Color)ColorConverter.ConvertFromString(content.FontColor) ,
                        FontSize = content.FontSize,
                        IsBold = content.IsBold,
                        IsItalic = content.IsItalic,
                        IsUnderline = content.IsUnderline,
                        IsSelected = true,
                    };
                }
                else
                {
                    return new RssContentViewModel
                    {
                        FieldName = c.Key,
                        FieldNameContent = c.Value,
                        FontFamily = fontFamilis.First(),
                        Fonts = fontFamilis,
                        FontColor = Colors.White,
                        FontSize = 12,
                        IsBold = false,
                        IsItalic = false,
                        IsUnderline = false,
                        IsSelected = false,
                    };
                }
            }));
        }

        public static RssComponentViewModel CreateInstance(int id)
        {
            return new RssComponentViewModel(new RssComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_108")}{id}",
                ZIndex = 1,
                Type = MediaType.Rss,
                Timeline = 5,
                PlayCount = 1,
                PlayDuration = "00:00:05",
                Source = "about:blank",
                Interval = 10,
                PlayMode = "pageTurning",
                ComponentEffect = "FadeIn",
                EffectDuration = 1000,
                Direction = "rollingLeft",
                RollingSpeed = 3,
                Contents = new List<RssContent>()
            });
        }

        public override RssComponent ToModel(double ratio)
        {
            return new RssComponent
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
                PlayCount = PlayCount,
                PlayDuration = PlayDuration,

                PlayMode = PlayMode,
                Direction = Direction,
                EffectDuration = EffectDuration,
                ComponentEffect = ComponentEffectKey,
                RollingSpeed = RollingSpeed == 0 ? 3 : RollingSpeed,
                Interval = Interval,
                Contents = Contents.Where(c => c.IsSelected).Select(c => c.ToModel()).ToList()
            };
        }

        protected override FrameworkElement DrawingContent()
        {
            Image result = new()
            {
                Source = GetBitmap(),                
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
        private BitmapImage GetBitmap()
        {
            var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/site-{Type.ToLower()}.png", UriKind.Absolute);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = uri;
            bitmap.EndInit();

            return bitmap;
        }

        protected override FrameworkElement DrawingRunningContent()
        {
            IsRunningLoaded = false;
            var result = LoadRssFeed().GetAwaiter().GetResult();
            IsRunningLoaded = true;
            return result;
        }

        private void InitializeTimer(Canvas canvas)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(Interval);
            _timer.Tick += (s, e) => Timer_Tick(canvas);
            _timer.Start();
            currentPlayCount++;
        }

        private void Timer_Tick(Canvas target)
        {
            var newContent = LoadRssFeed().GetAwaiter().GetResult();
            if (newContent != null)
            {

                var mainCanvas = FindCanvasParent(target);
                mainCanvas.Children.Remove(target);
                mainCanvas.Children.Add(newContent);
            }
        }

        private void Scrolling(TextBlock target)
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
            if (element is RichTextBox target)
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

        private async Task<Canvas> LoadRssFeed()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string rssContent = await client.GetStringAsync(Source);
                    XDocument rssDoc = XDocument.Parse(rssContent);

                    Canvas canvas = new Canvas()
                    {
                        Width = Width * Ratio,
                        Height = Height * Ratio,
                    };

                    // 解析RSS内容并显示在Canvas中
                    foreach (var item in rssDoc.Descendants("item"))
                    {
                        Border border = new Border
                        {
                            BorderBrush = Brushes.White,
                            BorderThickness = new Thickness(1),
                        };

                        StackPanel panel = new StackPanel()
                        {
                            Width = Width * Ratio,
                            Height = Height * Ratio,
                        };

                        foreach (var content in Contents.Where(c => c.IsSelected))
                        {
                            string? fieldValue = item.Element(content.FieldName)?.Value;
                            if (fieldValue != null)
                            {
                                TextBlock valueBlock = new TextBlock
                                {
                                    Text = fieldValue,
                                    FontSize = content.FontSize,
                                    FontFamily = content.FontFamily,
                                    Foreground = new SolidColorBrush(content.FontColor),
                                    FontWeight = content.IsBold ? FontWeights.Bold : FontWeights.Normal,
                                    FontStyle = content.IsItalic ? FontStyles.Italic : FontStyles.Normal,
                                    TextDecorations = content.IsUnderline ? TextDecorations.Underline : null,
                                    TextWrapping = TextWrapping.WrapWithOverflow,
                                };

                                panel.Children.Add(valueBlock);

                                if (PlayMode == FindResource("LanguageKey_Code_ProgramEdit_Tooltip_128"))
                                {
                                    Scrolling(valueBlock);
                                }

                                if (PlayMode == FindResource("LanguageKey_Code_ProgramEdit_Tooltip_127") && ComponentEffectKey != null)
                                {
                                    Effects.Find(c => c.Key == ComponentEffectKey)?.Action(valueBlock);
                                }
                            }
                        }

                        border.Child = panel;

                        Canvas.SetTop(border, 0);
                        Canvas.SetLeft(border, 0);
                        Canvas.SetZIndex(border, 1);

                        canvas.Children.Add(border);
                    }

                    Canvas.SetLeft(canvas, Left * Ratio);
                    Canvas.SetTop(canvas, Top * Ratio);
                    Canvas.SetZIndex(canvas, ZIndex);
                    return canvas;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading RSS feed: {ex.Message}");
                return null;
            }
        }
    }

    public partial class RssContentViewModel : ObservableObject
    {
        [ObservableProperty]
        private string fieldName;

        [ObservableProperty]
        private string fieldNameContent;

        [ObservableProperty]
        private FontFamily fontFamily;

        [ObservableProperty]
        private IList<FontFamily> fonts;

        [ObservableProperty]
        private int fontSize;

        [ObservableProperty]
        private Color fontColor;

        [ObservableProperty]
        private bool isBold;

        [ObservableProperty]
        private bool isItalic;

        [ObservableProperty]
        private bool isUnderline;

        [ObservableProperty]
        private bool isSelected;

        //public RssContentViewModel(RssContent content)
        //{
        //    fieldName = content.FieldName;
        //    fontFamily = content.FontFamily;
        //    fontSize = content.FontSize;
        //    fontColor = content.FontColor;
        //    isBold = content.IsBold;
        //    isItalic = content.IsItalic;
        //    isUnderline = content.IsUnderline;
        //    isSelected = false;
        //}

        public RssContent ToModel()
        {
            return new RssContent
            {
                FieldName = FieldName,
                FontFamily = FontFamily.ToString(),
                FontSize = FontSize,
                FontColor = FontColor.ToString(),
                IsBold = IsBold,
                IsItalic = IsItalic,
                IsUnderline = IsUnderline,
            };
        }
    }
}
