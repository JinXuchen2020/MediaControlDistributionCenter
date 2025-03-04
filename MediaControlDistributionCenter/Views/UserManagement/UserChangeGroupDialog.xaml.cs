using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
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
            var selectedGroup = (UserGroupViewModel)cbGroups.SelectedValue;
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var selectedUsers = manageViewModel.Users.Where(c => c.IsSelected);

            foreach (var user in selectedUsers)
            {
                user.GroupId = selectedGroup?.Id;
                user.AgentId = selectedGroup?.AgentId ?? user.AgentId;
                user.Group = manageViewModel.Groups.FirstOrDefault(c => c.Id == selectedGroup?.Id)?.Name ?? "未分组";
               
                user.IsSelected = false;
                SQLite.UpdateTable<User>(user.ToModel());
            }

            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
