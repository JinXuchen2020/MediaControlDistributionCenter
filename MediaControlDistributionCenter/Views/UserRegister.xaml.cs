using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MediaControlDistributionCenter.Utilities;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// UserRegister.xaml 的交互逻辑
    /// </summary>
    public partial class UserRegister : UserControl
    {
        public UserRegister()
        {
            InitializeComponent();
        }

        private void AutoGeneratePasswordRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (pwdInput is not null)
            {
                pwdInput.Password = PasswordGenerator.GeneratePassword(8);
            }
        }

        private void CustomPasswordRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (pwdInput is not null)
            {
                pwdInput.Password = string.Empty;
            }
        }

        private void SaveUserButton_Clicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
