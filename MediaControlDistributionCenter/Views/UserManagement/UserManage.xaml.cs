using Dm.filter;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
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

        public event EventHandler ConnectedDeviceChanged;

        public UserManage(UserManageViewModel userManageViewModel, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            manageViewModel = userManageViewModel;
            manageViewModel.LoadData();
            DataContext = manageViewModel;
            InitializeComponent();
        }

        private void btnControlUser_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = ((sender as Button).DataContext as UserViewModel)!;
            if(userViewModel.Role != "user")
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Users_Tooltip_104");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.SelectedUser = userViewModel;
            var content = serviceProvider.GetRequiredService<UserControllers>();
            content.ConnectedDeviceChanged += ConnectedDeviceChanged;
            (App.Current.MainWindow as MainWindow).GoContent(content, 2);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = ((sender as Button).DataContext as UserViewModel)!;
            if (manageViewModel.CurrentUser.Role == "agent")
            {
                userViewModel.AgentUserGroupId = userViewModel.SelectedGroupId;
            }
            else
            {
                userViewModel.AdminUserGroupId = userViewModel.SelectedGroupId;
            }

            manageViewModel.SaveUserCommand.Execute(userViewModel);
        }

        private void btnRegister_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as UserManageViewModel)!;
            var viewModel = new UserViewModel()
            {
                Role = "user",
                Status = 1,
                TimeZone = TimeZoneInfo.Local.DisplayName,
            };
            viewModel.Groups = new ObservableCollection<UserGroupViewModel>(manageViewModel.Groups.Where(c => c.Id != -1));
            viewModel.Agents = manageViewModel.CurrentUser.Role == "admin" ? new ObservableCollection<UserViewModel>(manageViewModel.Users.Where(c => c.Role == "agent")) :
                new ObservableCollection<UserViewModel>(new List<UserViewModel> { manageViewModel.CurrentUser });
            viewModel.RoleList = manageViewModel.CurrentUser.Role == RoleType.Admin.ToString().ToLower() ? new ObservableCollection<object>(new List<RoleModel>
            {
                new RoleModel
                {
                    Role = RoleType.Agent.ToString().ToLower(),
                    RoleText = (string)FindResource("LanguageKey_Code_Role_Agent")
                },
                new RoleModel
                {
                    Role = RoleType.User.ToString().ToLower(),
                    RoleText = (string)FindResource("LanguageKey_Code_Role_User")
                }
            }) : new ObservableCollection<object>(new List<RoleModel>
            {
                new RoleModel
                {
                    Role = RoleType.User.ToString().ToLower(),
                    RoleText = (string)FindResource("LanguageKey_Code_Role_User")
                }
            });
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as UserViewModel)!;
            viewModel.Groups = new ObservableCollection<UserGroupViewModel>(manageViewModel.Groups.Where(c => c.Id != -1));
            viewModel.Agents = manageViewModel.CurrentUser.Role == "admin" ? new ObservableCollection<UserViewModel>(manageViewModel.Users.Where(c => c.Role == "agent")) :
                new ObservableCollection<UserViewModel>(new List<UserViewModel> { manageViewModel.CurrentUser });
            viewModel.SelectedGroupId = manageViewModel.CurrentUser.Role == "admin" ? viewModel.AdminUserGroupId : viewModel.AgentUserGroupId;
            viewModel.RoleList = manageViewModel.CurrentUser.Role == RoleType.Admin.ToString().ToLower() ? new ObservableCollection<object>(new List<RoleModel>
            {
                new RoleModel
                {
                    Role = RoleType.Agent.ToString().ToLower(),
                    RoleText = (string)FindResource("LanguageKey_Code_Role_Agent")
                },
                new RoleModel
                {
                    Role = RoleType.User.ToString().ToLower(),
                    RoleText = (string)FindResource("LanguageKey_Code_Role_User")
                }
            }) : new ObservableCollection<object>(new List<RoleModel>
            {
                new RoleModel
                {
                    Role = RoleType.User.ToString().ToLower(),
                    RoleText = (string)FindResource("LanguageKey_Code_Role_User")
                }
            });
            viewModel.LoadLogo();
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);
                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    var viewModel = ((sender as Button).DataContext as UserViewModel)!;
                    await manageViewModel.DeleteUserCommand.ExecuteAsync(viewModel);
                }

                manageViewModel.CanDelete = null;
            });                      
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedUsers = manageViewModel.Users.Where(c => c.IsSelected);
            if(selectedUsers.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Users_Tooltip_105");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var dialogBox = serviceProvider.GetRequiredService<UserChangeGroupDialog>();
            manageViewModel.ShowDialogContentCommand.Execute(dialogBox);
        }

        private void btnDeleteAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedUsers = manageViewModel.Users.Where(c => c.IsSelected).ToList();
            if (selectedUsers.Count == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Users_Tooltip_105");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    await manageViewModel.DeleteUserBatchCommand.ExecuteAsync(null);
                    manageViewModel.LoadData();
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = new UserGroupViewModel();
            groupViewModel.AgentId = manageViewModel.CurrentUser.Account;
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var groupViewModel = ((sender as DockPanel).DataContext as UserGroupViewModel)!;
            manageViewModel.SelectedGroup = groupViewModel;
            manageViewModel.LoadData();
        }

        private void btnGroupSave_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = ((sender as Button).DataContext as UserGroupViewModel)!;

            manageViewModel.SaveGroupCommand.Execute(groupViewModel);
        }

        private void btnUpload_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ((sender as Border).DataContext as UserViewModel)!;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(filePath);
                this.Dispatcher.Invoke(async () =>
                {
                    var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                    await ftpClient.UploadFileToFtpServer(filePath, $"{viewModel.Account}{extension}");

                    // 显示缩略图
                    BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                    viewModel.LogoThumbnail = bitmap;
                    viewModel.Logo = filePath;
                    viewModel.LogoFileName = $"{viewModel.Account}{extension}";
                    viewModel.IsUpload = true;
                });
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox.IsChecked.GetValueOrDefault())
            {
                foreach (var item in manageViewModel.Users)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (var item in manageViewModel.Users)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void btnGroupDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as UserGroupViewModel)!;
            if (viewModel.Id != -1)
            {
                this.Dispatcher.Invoke(async () =>
                {
                    manageViewModel.CanDelete = false;
                    await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);
                    if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                    {
                        await manageViewModel.DeleteGroupCommand.ExecuteAsync(viewModel);
                    }

                    manageViewModel.CanDelete = null;
                });

            }
            
        }
    }
}
