using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MediaControlDistributionCenter.Views.CustomControls
{
    /// <summary>
    /// Breadcrumb.xaml 的交互逻辑
    /// </summary>
    public partial class Breadcrumb : UserControl
    {
        public ICollection<BreadcrumbItem> BreadcrumbItems { get; set; }

        public Breadcrumb()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 这里添加导航逻辑
            var button = sender as Button;
            if (button != null)
            {
                var breadcrumbItem = button.DataContext as BreadcrumbItem;
                if (breadcrumbItem != null)
                {
                    // 执行导航操作，例如
                    // NavigateToPage(breadcrumbItem.Name);
                }
            }
        }
    }

    public class BreadcrumbItem
    {
        public string Name { get; set; }
        public bool IsLastItem { get; set; }
        // 可能还需要其他属性，如命令等
    }
}
