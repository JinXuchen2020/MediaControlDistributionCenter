using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class IconTextBox : Control
    {
        static IconTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconTextBox), new FrameworkPropertyMetadata(typeof(IconTextBox)));
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(IconTextBox), new PropertyMetadata(null));

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(IconTextBox), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register("IsPassword", typeof(bool), typeof(IconTextBox), new PropertyMetadata(false, OnIsPasswordChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(IconTextBox), new PropertyMetadata(5.0));

        private TextBox _textBox;
        private PasswordBox _passwordBox;
        private TextBlock _watermarkText;

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public string? Text
        {
            get
            {
                if (IsPassword)
                {
                    return _passwordBox?.Password;
                }
                else
                {
                    return _textBox?.Text;
                }
            }
            set
            {
                if (IsPassword)
                {
                    if (_passwordBox != null)
                    {
                        _passwordBox.Password = value ?? string.Empty;
                    }
                }
                else
                {
                    if (_textBox != null)
                    {
                        _textBox.Text = value ?? string.Empty;
                    }
                }
                //UpdateWatermarkVisibility();
            }
        }

        private static void OnIsPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as IconTextBox;
            control?.UpdatePasswordState();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("TextBoxControl") as TextBox;
            _passwordBox = GetTemplateChild("PasswordBoxControl") as PasswordBox;
            _watermarkText = GetTemplateChild("WatermarkText") as TextBlock;

            if (_textBox == null || _passwordBox == null || _watermarkText == null)
            {
                throw new InvalidOperationException("Template parts not found.");
            }
            _textBox.GotFocus += TextBox_GotFocus;
            _textBox.LostFocus += TextBox_LostFocus;
            _passwordBox.GotFocus += PasswordBox_GotFocus;
            _passwordBox.LostFocus += PasswordBox_LostFocus;
            UpdatePasswordState();
        }

        private void UpdatePasswordState()
        {
            if (_textBox != null && _passwordBox != null)
            {
                if (IsPassword)
                {
                    _textBox.Visibility = Visibility.Collapsed;
                    _passwordBox.Visibility = Visibility.Visible;
                }
                else
                {
                    _textBox.Visibility = Visibility.Visible;
                    _passwordBox.Visibility = Visibility.Collapsed;
                }
            }

            // 切换水印的显示状态
            if (_watermarkText != null)
            {
                UpdateWatermarkVisibility();
            }
        }

        // 更新水印显示状态
        private void UpdateWatermarkVisibility()
        {
            bool hasContent = false;

            if (IsPassword)
            {
                hasContent = !string.IsNullOrEmpty(_passwordBox.Password);
            }
            else
            {
                hasContent = !string.IsNullOrEmpty(_textBox.Text);
            }

            _watermarkText.Visibility = hasContent ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_watermarkText != null)
            {
                _watermarkText.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateWatermarkVisibility();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_watermarkText != null)
            {
                _watermarkText.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateWatermarkVisibility();
        }
    }
}
