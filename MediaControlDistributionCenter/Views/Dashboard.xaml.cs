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
using Microsoft.Extensions.DependencyInjection;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private readonly DashboardViewModel manageViewModel;
        private readonly IServiceProvider serviceProvider;

        public Dashboard(DashboardViewModel dashboardViewModel, IServiceProvider serviceProvider)
        {
            manageViewModel = dashboardViewModel;
            this.serviceProvider = serviceProvider;
            InitializeComponent();

            DataContext = dashboardViewModel;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock tx = sender as TextBlock;

            var userViewModel = manageViewModel.Users.FirstOrDefault();            

            if (manageViewModel.CurrentUser.Role == "user")
            {
                //userViewModel = manageViewModel.CurrentUser;
                var content = serviceProvider.GetRequiredService<DeviceControlContent>();// new DeviceControlContent(userViewModel, tx.Tag.ToString(), true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 3);
            }
            else
            {
                if (userViewModel != null)
                {
                    var content = serviceProvider.GetRequiredService<UserControllers>();
                    var deviceControl =  serviceProvider.GetRequiredService<DeviceControlContent>();
                    content.GoCotent(deviceControl, 3);
                    //var userControllers = new UserControllers(userViewModel);
                    //var viewModel = (userControllers.DataContext as UserControllerViewModel)!;
                    //viewModel.CurrentTabName = tx.Tag.ToString();
                    //viewModel.CurrentPageName = (string)FindResource("LanguageKey_Code_Control_Device");
                    
                    (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
                }
            }
        }

        private void btnToMediaManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var userViewModel = manageViewModel.Users.FirstOrDefault();
            if(manageViewModel.CurrentUser.Role == "user")
            {
                userViewModel = manageViewModel.CurrentUser;
                var content = serviceProvider.GetRequiredService<MediaManage>(); //new MediaManage(userViewModel, true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
            else
            {
                if (userViewModel != null)
                {
                    var content = serviceProvider.GetRequiredService<UserControllers>();
                    (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
                }
            }            
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = ((sender as DataGrid).SelectedItem as DeviceViewModel)!;
            var userViewModel = manageViewModel.Users.FirstOrDefault(c => c.Account == viewModel.UserIdAccount);
            if (manageViewModel.CurrentUser.Role == "user")
            {
                userViewModel = manageViewModel.CurrentUser;
                var content = serviceProvider.GetRequiredService<MediaManage>();
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
            else
            {
                if (userViewModel != null)
                {
                    var content = serviceProvider.GetRequiredService<UserControllers>();
                    (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
                }
            }
        }

        private void btnToUserManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var content = serviceProvider.GetRequiredService<UserManage>();// new UserManage(manageViewModel.CurrentUser);
            (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var userViewModel = ((sender as DataGrid).SelectedItem as UserViewModel)!;
            if (userViewModel != null && userViewModel.Role == "user")
            {
                var content = serviceProvider.GetRequiredService<UserControllers>();
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
        }

        private void btnToDeviceManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var userViewModel = manageViewModel.CurrentUser;
            var content = serviceProvider.GetRequiredService<DeviceManage>(); //new DeviceManage(userViewModel, true);
            (App.Current.MainWindow as MainWindow).GoCotent(content, 3);
        }

        private void btnToMediaEdit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = ((sender as StackPanel).DataContext as MediaViewModel)!;
            var content = serviceProvider.GetRequiredService<MediaEdit>();
            (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
        }

        private void selectDevice_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = ((sender as StackPanel).DataContext as DeviceViewModel)!;
            var userViewModel = manageViewModel.CurrentUser;
            var content = serviceProvider.GetRequiredService<DeviceManage>(); //new DeviceManage(userViewModel, true);
            (App.Current.MainWindow as MainWindow).GoCotent(content, 3);
        }
    }
}
