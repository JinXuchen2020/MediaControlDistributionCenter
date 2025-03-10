using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.ViewModels;
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

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// UserSaveResultDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ResultConfirmDialog : UserControl
    {
        public ResultConfirmDialog(ObservableObject content)
        {
            InitializeComponent();
            DataContext = content;
        }
    }

    public class ConfirmDialogDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var type = item.GetType();
            var dialogBox = FindDialog(container);
            switch (type)
            {
                case var o when o == typeof(UserViewModel):
                    return (DataTemplate)dialogBox.FindResource("UserRegisterSuccess");
                case var o when o == typeof(MediaViewModel):
                    return (DataTemplate)dialogBox.FindResource("MediaContentSave");
                case var o when o == typeof(MediaDevicesViewModel):
                    return (DataTemplate)dialogBox.FindResource("MediaContentPublish");
                default:
                    return null;
            }
        }

        private ResultConfirmDialog FindDialog(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                if (parentObject is ResultConfirmDialog)
                {
                    return parentObject as ResultConfirmDialog;
                }

                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null; // 如果没有找到Canvas，则返回null
        }
    }
}
