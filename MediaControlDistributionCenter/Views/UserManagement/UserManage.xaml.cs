using Dm.filter;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// UserManage.xaml 的交互逻辑
    /// </summary>
    public partial class UserManage : UserControl
    {
        private readonly UserManageViewModel manageViewModel;

        private readonly IServiceProvider serviceProvider;

        public UserManage(LoginViewModel loginViewModel, UserManageViewModel userManageViewModel, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            manageViewModel = userManageViewModel;
            manageViewModel.SetValues(loginViewModel.CurrentUser);
            DataContext = manageViewModel;
            InitializeComponent();
        }

        private void btnControlUser_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = ((sender as Button).DataContext as UserViewModel)!;
            if(userViewModel.Role != "user")
            {
                MessageBox.Show("无法控制代理商！");
                return;
            }

            manageViewModel.SelectedUser = userViewModel;
            var content = serviceProvider.GetRequiredService<UserControllers>();
            (App.Current.MainWindow as MainWindow).GoContent(content, 2);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = ((sender as Button).DataContext as UserViewModel)!;

            manageViewModel.SaveUserCommand.Execute(userViewModel);
        }

        private void btnRegister_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var viewModel = new UserViewModel()
            {
                Role = "user"
            };
            viewModel.Groups = manageViewModel.Groups;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as UserViewModel)!;
            viewModel.Groups = manageViewModel.Groups;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as UserViewModel)!;
            manageViewModel.DeleteUserCommand.Execute(viewModel);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedUsers = manageViewModel.Users.Where(c => c.IsSelected);
            if(selectedUsers.Count() == 0)
            {
                MessageBox.Show("请选择用户！");
                return;
            }

            var dialogBox = new UserChangeGroupDialog(manageViewModel);
            manageViewModel.ShowDialogContentCommand.Execute(dialogBox);
        }

        private void btnDeleteAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var selectedUsers = manageViewModel.Users.Where(c => c.IsSelected).ToList();
            if (selectedUsers.Count == 0)
            {
                MessageBox.Show("请选择用户！");
                return;
            }

            manageViewModel.DeleteUserBatchCommand.Execute(null);
            manageViewModel.SetValues(manageViewModel.CurrentUser);
        }

        private void btnGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = new UserGroupViewModel();
            groupViewModel.Agents = manageViewModel.CurrentUser.Role == "agent" ? new List<UserViewModel> { manageViewModel.CurrentUser }
                    : manageViewModel.Users.Where(c => c.Role == "agent").ToList();
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var groupViewModel = ((sender as StackPanel).DataContext as UserGroupViewModel)!;
            manageViewModel.SetValues(manageViewModel.CurrentUser, groupViewModel.Id);
        }

        private void btnGroupSave_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = ((sender as Button).DataContext as UserGroupViewModel)!;

            manageViewModel.SaveGroupCommand.Execute(groupViewModel);
        }
    }
}
