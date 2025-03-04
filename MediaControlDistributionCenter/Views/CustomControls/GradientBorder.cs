using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class GradientBorder : Border
    {
        // 定义渐变起始颜色
        public static readonly DependencyProperty GradientStartColorProperty =
            DependencyProperty.Register(nameof(GradientStartColor), typeof(Color), typeof(GradientBorder), new PropertyMetadata(Colors.Transparent, OnGradientChanged));

        // 定义渐变结束颜色
        public static readonly DependencyProperty GradientEndColorProperty =
            DependencyProperty.Register(nameof(GradientEndColor), typeof(Color), typeof(GradientBorder), new PropertyMetadata(Colors.Transparent, OnGradientChanged));

        // 渐变的起始颜色
        public Color GradientStartColor
        {
            get => (Color)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        // 渐变的结束颜色
        public Color GradientEndColor
        {
            get => (Color)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        static GradientBorder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GradientBorder), new FrameworkPropertyMetadata(typeof(GradientBorder)));
        }

        // 渐变色变更时的回调
        private static void OnGradientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GradientBorder border)
            {
                border.UpdateGradient();
            }
        }

        // 更新渐变色
        private void UpdateGradient()
        {
            // 使用 90 度渐变来实现上下方向的渐变
            var gradientBrush = new LinearGradientBrush(GradientStartColor, GradientEndColor, 90);
            this.Background = gradientBrush; // 设置背景为渐变色
        }
    }
}
