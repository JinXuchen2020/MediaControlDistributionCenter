using MediaControlDistributionCenter.Helpers;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : UserControl
    {
        public string Result { get; private set; }
        public InputDialog()
        {
            InitializeComponent();

            this.Unloaded += InputDialog_Unloaded;
        }

        private void InputDialog_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Result = this.tbResult.Text;
            MaterialDesignThemes.Wpf.DialogHost.Close(Constants.ErrorMessageboxId);
        }
    }
}
