using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

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

            var type = content.GetType();
            switch (content)
            {                
                case var o when o is DeviceTimeControlViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage):
                    btnConfirm.Visibility = Visibility.Collapsed;
                    break;
                case var o when o is DeviceViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage) && viewModel.IsConntectd():
                    btnConfirm.Visibility = Visibility.Collapsed;
                    break;
                case var o when o is PageViewModel viewModel && !string.IsNullOrEmpty(viewModel.ErrorMessage):
                    btnConfirm.Click += BtnConfirm_Click; 
                    break;
                case var o when o is PageViewModel viewModel && viewModel.CanDelete.HasValue:
                    btnConfirm.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as PageViewModel)!;
            viewModel.ErrorMessage = null;
        }

        private void btnExecute(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceTimeControlViewModel)!;
            var manageViewModel = App.ServicesProvider.GetRequiredService<DeviceControlViewModel>();
            manageViewModel.ExecuteScheduleControlCommand.Execute(viewModel);
            manageViewModel.CloseDialogCommand.Execute(null);
        }

        private void btnExecuteSendUser(object sender, RoutedEventArgs e)
        {
            var manageViewModel = App.ServicesProvider.GetRequiredService<DeviceManageViewModel>();
            manageViewModel.SendUserToDeviceCommand.Execute(null);
            manageViewModel.CloseDialogCommand.Execute(null);
        }

        private void btnExecuteDelete(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as PageViewModel)!;
            viewModel.CanDelete = true;
            MaterialDesignThemes.Wpf.DialogHost.Close(Constants.ErrorMessageboxId);
        }
    }

    public class ConfirmDialogDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var type = item.GetType();
            var dialogBox = FindDialog(container);
            switch (item)
            {
                case var o when o is UserViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage):
                    return (DataTemplate)dialogBox.FindResource("UserRegisterSuccess");
                case var o when o is ProgramViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage):
                    return (DataTemplate)dialogBox.FindResource("MediaContentSave");
                case var o when (o is PageViewModel viewModel && viewModel.CanDelete.HasValue):
                    return (DataTemplate)dialogBox.FindResource("DeleteExecution");
                case var o when o is MediaDevicesViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage):
                    return (DataTemplate)dialogBox.FindResource("MediaContentPublish");
                case var o when o is DeviceTimeControlViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage):                    
                    return (DataTemplate)dialogBox.FindResource("ScheduleControlExecution");
                case var o when o is DeviceViewModel viewModel && string.IsNullOrEmpty(viewModel.ErrorMessage) && viewModel.IsConntectd():
                    return (DataTemplate)dialogBox.FindResource("ScheduleSendUserExecution");
                case var o when (o is LoginViewModel loginViewModel && string.IsNullOrEmpty(loginViewModel.ErrorMessage) && loginViewModel.IsSync):
                    return (DataTemplate)dialogBox.FindResource("SyncUserResult");
                case var o when (o is PageViewModel viewModel && !string.IsNullOrEmpty(viewModel.ErrorMessage)):
                    return (DataTemplate)dialogBox.FindResource("ManageErrorResult");
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

        private string FindResource(string key)
        {
            return (string)LanguageTool.Instance.FindResource(key);
        }
    }
}
