using Dm.filter;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
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
        private IUserService userService;
        public UserManage(UserViewModel userViewModel)
        {
            userService = new UserService();

            var groups = userService.GetUserGroups(userViewModel.Role == "agent" ? userViewModel.Id : null).GetAwaiter().GetResult().ToList();
            groups.Insert(0, new UserGroupViewModel(new UserGroup
            {
                Id = -1,
                Name = "全部",
                AgentId = userViewModel.Id,
            }, true));
            var users = userService.GetUsers(userViewModel.Role == "agent" ? userViewModel.Id : null).GetAwaiter().GetResult();

            DataContext = new UserManageViewModel(userViewModel, users, groups);
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

            var userControllers = new UserControllers();
            (App.Current.MainWindow as MainWindow).GoCotent(userControllers, 2);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = ((sender as Button).DataContext as UserViewModel)!;
            userViewModel.Group = userViewModel.Groups.FirstOrDefault(c => c.Id == userViewModel.GroupId)?.Name ?? "未分组";
            //userViewModel.SaveCommand.Execute(null);

            var manageViewModel = (DataContext as UserManageViewModel)!;
            var existUser = manageViewModel.Users.FirstOrDefault(c => c.Account == userViewModel.Account);
            if (existUser != null) 
            {
                existUser = userViewModel;
                //manageViewModel.Users.Remove(existUser);
                //manageViewModel.Users.Add(userViewModel);
                SQLite.UpdateTable<User>(userViewModel.ToModel());
                manageViewModel.CloseDialogCommand.Execute(null);
            }
            else
            {
                userViewModel.Id = SQLite.InserTable<User>(userViewModel.ToModel());
                manageViewModel.Users.Add(userViewModel);
                manageViewModel.CloseDialogCommand.Execute(null);
                userViewModel.ShowConfirmDialogCommand.Execute(null);
            }
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
            var manageViewModel = (DataContext as UserManageViewModel)!;
            viewModel.Groups = manageViewModel.Groups;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as UserViewModel)!;
            var manageViewModel = (DataContext as UserManageViewModel)!;
            manageViewModel.Users.Remove(viewModel);
            SQLite.DeleteById<User>(viewModel.Id);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var dialogBox = new UserChangeGroupDialog(manageViewModel);
            manageViewModel.ShowDialogContentCommand.Execute(dialogBox);
        }

        private void btnDeleteAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var selectedUsers = manageViewModel.Users.Where(c => c.IsSelected).ToList();
            if (selectedUsers.Any()) 
            {
                foreach(var user in selectedUsers)
                {
                    manageViewModel.Users.Remove(user);
                    SQLite.DeleTeable<User>(user.ToModel());
                }
            }
        }

        private void btnGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var groupViewModel = new UserGroupViewModel();
            groupViewModel.Agents = manageViewModel.CurrentUser.Role == "agent" ? new List<UserViewModel> { manageViewModel.CurrentUser }
                    : manageViewModel.Users.Where(c => c.Role == "agent").ToList();
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            manageViewModel.Groups.First(c => c.IsSelected).IsSelected = false;
            var groupViewModel = ((sender as StackPanel).DataContext as UserGroupViewModel)!;
            groupViewModel.IsSelected = true;

            var viewModels = userService.GetUsers(
                    manageViewModel.CurrentUser.Role == "agent" ? manageViewModel.CurrentUser.Id : null,
                    groupViewModel.Id != -1 ? groupViewModel.Id : null).GetAwaiter().GetResult();
            manageViewModel.Users = new ObservableCollection<UserViewModel>(viewModels);
        }

        private void btnGroupSave_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;

            var groupViewModel = ((sender as Button).DataContext as UserGroupViewModel)!;
            if(groupViewModel.Id == 0)
            {
                groupViewModel.Id = SQLite.InserTable(groupViewModel.ToModel());
                manageViewModel.Groups.Add(groupViewModel);
            }
            else
            {
                SQLite.UpdateTable(groupViewModel.ToModel());
            }

            manageViewModel.CloseDialogCommand.Execute(false);
        }
    }
}
