using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MediaControlDistributionCenter.Views.CustomControls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MediaControlDistributionCenter.Views.CustomControls;assembly=MediaControlDistributionCenter.Views.CustomControls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:RoundRectButton/>
    ///
    /// </summary>
    public class RoundRectButton : Button
    {
        static RoundRectButton()
        {
            // 注册自定义控件的默认样式
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundRectButton), new FrameworkPropertyMetadata(typeof(RoundRectButton)));
        }

        // 自定义属性：图标路径
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(RoundRectButton), new PropertyMetadata(null));

        public ImageSource IconSource
        {
            get => (ImageSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }
        // 自定义属性：是否显示图标
        public static readonly DependencyProperty IconVisibilityProperty =
            DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(RoundRectButton), new PropertyMetadata(Visibility.Visible));

        public Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }
        // 自定义属性：边框颜色
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(SolidColorBrush), typeof(RoundRectButton), new PropertyMetadata(Brushes.Black));

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }


        // 自定义属性：边框宽度
        public static readonly DependencyProperty BorderWidthProperty =
            DependencyProperty.Register("BorderWidth", typeof(double), typeof(RoundRectButton), new PropertyMetadata(2.0));

        public double BorderWidth
        {
            get => (double)GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        // 自定义属性：圆角角度
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(RoundRectButton), new PropertyMetadata(15.0));

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        // Normal, Hover, and Pressed color properties
        public static readonly DependencyProperty NormalColorProperty =
            DependencyProperty.Register("NormalColor", typeof(SolidColorBrush), typeof(RoundRectButton), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public SolidColorBrush NormalColor
        {
            get => (SolidColorBrush)GetValue(NormalColorProperty);
            set => SetValue(NormalColorProperty, value);
        }

        public static readonly DependencyProperty HoverColorProperty =
            DependencyProperty.Register("HoverColor", typeof(SolidColorBrush), typeof(RoundRectButton), new PropertyMetadata(new SolidColorBrush(Colors.LightSkyBlue)));

        public SolidColorBrush HoverColor
        {
            get => (SolidColorBrush)GetValue(HoverColorProperty);
            set => SetValue(HoverColorProperty, value);
        }

        public static readonly DependencyProperty PressedColorProperty =
            DependencyProperty.Register("PressedColor", typeof(SolidColorBrush), typeof(RoundRectButton), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public SolidColorBrush PressedColor
        {
            get => (SolidColorBrush)GetValue(PressedColorProperty);
            set => SetValue(PressedColorProperty, value);
        }

        public RoundRectButton()
        {
            Debug.WriteLine($"BorderColor: {BorderColor}");
            // 按钮点击事件
            //this.Click += (sender, e) => MessageBox.Show("圆角矩形按钮被点击了!");
        }
    }
}
