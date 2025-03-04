using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MediaControlDistributionCenter.Views.UserManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using MediaControlDistributionCenter.Views.DeviceManagement;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private IDeviceService deviceService;
        private IUserService userService;

        public Dashboard(UserViewModel userViewModel)
        {
            deviceService = new DeviceService();
            userService = new UserService();
            InitializeComponent();

            var devices = userViewModel.Role == "user" ? deviceService.GetDevices(userViewModel.Id).GetAwaiter().GetResult()
                : userViewModel.Role == "admin" ? deviceService.GetDevices().GetAwaiter().GetResult() :
                deviceService.GetAgentDevices(userViewModel.Id).GetAwaiter().GetResult();

            var users = userViewModel.Role == "agent" ? userService.GetUsers(userViewModel.Id).GetAwaiter().GetResult() :
                userViewModel.Role == "admin" ? userService.GetUsers().GetAwaiter().GetResult() : new List<UserViewModel>();

            var medias = userViewModel.Role == "user" ? deviceService.GetMedias(userViewModel.Id).GetAwaiter().GetResult() : new List<MediaViewModel>();

            DataContext = new DashboardViewModel(userViewModel, devices, users, medias);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;
            TextBlock tx = sender as TextBlock;

            var userViewModel = manageViewModel.Users.FirstOrDefault();            

            if (manageViewModel.CurrentUser.Role == "user")
            {
                userViewModel = manageViewModel.CurrentUser;
                var content = new DeviceControlContent(userViewModel, tx.Tag.ToString(), true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 3);
            }
            else
            {
                if (userViewModel != null)
                {
                    var userControllers = new UserControllers(userViewModel);
                    var viewModel = (userControllers.DataContext as UserControllerViewModel)!;
                    viewModel.CurrentTabName = tx.Tag.ToString();
                    viewModel.CurrentPageName = (string)FindResource("LanguageKey_Code_Control_Device");
                    userControllers.GoCotent(new DeviceControlContent(userViewModel, tx.Tag.ToString()), 3);
                    (App.Current.MainWindow as MainWindow).GoCotent(userControllers, 2);
                }
            }
        }

        private void btnToMediaManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;
            var userViewModel = manageViewModel.Users.FirstOrDefault();
            if(manageViewModel.CurrentUser.Role == "user")
            {
                userViewModel = manageViewModel.CurrentUser;
                var content = new MediaManage(userViewModel, true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
            else
            {
                if (userViewModel != null)
                {
                    var content = new UserControllers(userViewModel);
                    (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
                }
            }            
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;

            var viewModel = ((sender as DataGrid).SelectedItem as DeviceViewModel)!;
            var userViewModel = manageViewModel.Users.FirstOrDefault(c=>c.Id == viewModel.UserId);
            if (manageViewModel.CurrentUser.Role == "user")
            {
                userViewModel = manageViewModel.CurrentUser;
                var content = new MediaManage(userViewModel, true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
            else
            {
                if (userViewModel != null)
                {
                    var content = new UserControllers(userViewModel);
                    (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
                }
            }
        }

        private void btnToUserManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;
            var content = new UserManage(manageViewModel.CurrentUser);
            (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;

            var userViewModel = ((sender as DataGrid).SelectedItem as UserViewModel)!;
            if (userViewModel != null && userViewModel.Role == "user")
            {
                var userControllers = new UserControllers(userViewModel);
                (App.Current.MainWindow as MainWindow).GoCotent(userControllers, 2);
            }
        }

        private void btnToDeviceManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;
            var userViewModel = manageViewModel.CurrentUser;
            var content = new DeviceManage(userViewModel, true);
            (App.Current.MainWindow as MainWindow).GoCotent(content, 3);
        }

        private void btnToMediaEdit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;
            var viewModel = ((sender as StackPanel).DataContext as MediaViewModel)!;
            (App.Current.MainWindow as MainWindow).GoCotent(new MediaEdit(viewModel, manageViewModel.CurrentUser, true), 2);
        }

        private void selectDevice_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as DashboardViewModel)!;

            var viewModel = ((sender as StackPanel).DataContext as DeviceViewModel)!;
            var userViewModel = manageViewModel.CurrentUser;
            var content = new DeviceManage(userViewModel, true);
            (App.Current.MainWindow as MainWindow).GoCotent(content, 3);
        }
    }
}
