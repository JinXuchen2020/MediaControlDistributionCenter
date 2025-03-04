using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class CircleWithTextControl : Control
    {
        // 注册依赖属性，用于绑定颜色和文本
        public static readonly DependencyProperty CircleColorProperty =
            DependencyProperty.Register("CircleColor", typeof(Brush), typeof(CircleWithTextControl), new PropertyMetadata(Brushes.Blue));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CircleWithTextControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(double), typeof(CircleWithTextControl), new PropertyMetadata(50.0, OnDiameterChanged));

        // CircleColor 属性
        public Brush CircleColor
        {
            get { return (Brush)GetValue(CircleColorProperty); }
            set { SetValue(CircleColorProperty, value); }
        }

        // Text 属性
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Diameter 属性
        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        // 直径变化时重新设置控件大小
        private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CircleWithTextControl)d;
            control.InvalidateMeasure();
        }

        // 重写 OnRender 方法绘制圆形和文本
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // 使用 Diameter 属性来绘制圆形
            double radius = Diameter / 2;

            // 计算圆形的绘制位置，确保其垂直居中
            double centerX = radius;
            double centerY = ActualHeight / 2;

            // 绘制圆形
            drawingContext.DrawEllipse(CircleColor, null, new Point(centerX+10, centerY), radius, radius);

            //PixelsPerDip
            // 绘制文本
            FormattedText formattedText = new(
                          Text,
                          System.Globalization.CultureInfo.CurrentCulture,
                          FlowDirection.LeftToRight,
                          new Typeface("Arial"),
                          12,
                          Brushes.Black,
                          VisualTreeHelper.GetDpi(this).PixelsPerDip); // 使用 PixelsPerDip


            // 计算文本的绘制位置，确保文本垂直居中
            double textX = Diameter + 20;
            double textY = centerY - formattedText.Height / 2; // 确保文本垂直居中

            drawingContext.DrawText(formattedText, new Point(textX, textY));
        }

        // 重写测量方法以适应新的直径
        protected override Size MeasureOverride(Size availableSize)
        {
            // 根据直径计算控件的最终大小
            double width = Diameter + 10 + 100; // 100是文本的宽度
            double height = Math.Max(Diameter, 18); // 高度最小为20，防止出现过小的文本显示

            return new Size(width, height);
        }
    }

}
