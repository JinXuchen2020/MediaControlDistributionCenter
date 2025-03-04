using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class CheckButton : CheckBox
    {
        static CheckButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckButton), new FrameworkPropertyMetadata(typeof(CheckButton)));
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CheckButton), new PropertyMetadata(new CornerRadius(0)));

        public SolidColorBrush CheckedBackground
        {
            get { return (SolidColorBrush)GetValue(CheckedBackgroundProperty); }
            set { SetValue(CheckedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register("CheckedBackground", typeof(SolidColorBrush), typeof(CheckButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public SolidColorBrush CheckedForeground
        {
            get { return (SolidColorBrush)GetValue(CheckedForegroundProperty); }
            set { SetValue(CheckedForegroundProperty, value); }
        }

        public static readonly DependencyProperty CheckedForegroundProperty =
            DependencyProperty.Register("CheckedForeground", typeof(SolidColorBrush), typeof(CheckButton), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
    }
}
