
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
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

namespace MediaControlDistributionCenter.Views.UserManagement
{
    /// <summary>
    /// UserChangeGroupDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UserChangeGroupDialog : UserControl
    {
        public UserChangeGroupDialog(UserManageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var selectedGroup = cbGroups.SelectedValue as UserGroupViewModel;
            if (selectedGroup == null || selectedGroup.Id == -1) 
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Users_Tooltip_110");
                //var dialog = new ResultConfirmDialog(manageViewModel);
                //MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageboxId);
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ChangeGroupCommand.Execute(selectedGroup);
        }
    }
}
