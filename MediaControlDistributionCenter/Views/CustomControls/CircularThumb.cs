using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Data;
using System.Runtime.CompilerServices;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class CircularThumb : Thumb
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CircularThumb), new PropertyMetadata(new CornerRadius(16.0)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public CircularThumb()
        {
            // 创建ControlTemplate
            var template = new ControlTemplate(typeof(Thumb));

            // 创建椭圆作为模板的可视化树
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.NameProperty, "PART_Border");
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background)) { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding(nameof(BorderBrush)) { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding(nameof(BorderThickness)) { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.CornerRadiusProperty, new Binding(nameof(CornerRadius)) { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            // 将椭圆添加到模板的可视化树
            template.VisualTree = borderFactory;

            // 应用模板
            this.Template = template;

            // 确保模板应用
            this.ApplyTemplate();
        }
    }
}
