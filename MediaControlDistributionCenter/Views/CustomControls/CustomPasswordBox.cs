using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class CustomPasswordBox : Control
    {
        static CustomPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomPasswordBox), new FrameworkPropertyMetadata(typeof(CustomPasswordBox)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CustomPasswordBox), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CustomPasswordBox), new PropertyMetadata(null));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty AlternateIconProperty =
            DependencyProperty.Register("AlternateIcon", typeof(ImageSource), typeof(CustomPasswordBox), new PropertyMetadata(null));

        public ImageSource AlternateIcon
        {
            get { return (ImageSource)GetValue(AlternateIconProperty); }
            set { SetValue(AlternateIconProperty, value); }
        }
        public string Password
        {
            get { return _passwordBox?.Password; }
            set
            {
                if (_passwordBox != null && _textBox != null)
                {
                    _passwordBox.Password = value;
                    _textBox.Text = value;
                }
            }
        }

        private PasswordBox _passwordBox;
        private TextBox _textBox;
        private Image _iconControl;
        private bool _isPasswordVisible;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _iconControl = GetTemplateChild("PART_Icon") as Image;

            if (_iconControl != null)
            {
                _iconControl.MouseDown += OnIconClick;
            }

            if (_passwordBox != null && _textBox != null)
            {
                _passwordBox.PasswordChanged += (s, e) => SyncPasswordToTextBox();
                _textBox.TextChanged += (s, e) => SyncTextBoxToPassword();
            }
        }

        private void SyncPasswordToTextBox()
        {
            if (_textBox.Text != _passwordBox.Password)
            {
                _textBox.Text = _passwordBox.Password;
            }
        }

        private void SyncTextBoxToPassword()
        {
            if (_passwordBox.Password != _textBox.Text)
            {
                _passwordBox.Password = _textBox.Text;
            }
        }

        private void OnIconClick(object sender, MouseButtonEventArgs e)
        {
            if (_passwordBox == null || _textBox == null) return;

            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                _passwordBox.Visibility = Visibility.Collapsed;
                _textBox.Visibility = Visibility.Visible;
                _iconControl.Source = AlternateIcon;
            }
            else
            {
                _passwordBox.Visibility = Visibility.Visible;
                _textBox.Visibility = Visibility.Collapsed;
                _iconControl.Source = Icon;
            }
        }
    }
}
