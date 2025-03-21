using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

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
            manageViewModel.LoadData();
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
                var content = serviceProvider.GetRequiredService<DeviceControlContent>(); // new DeviceControlContent(userViewModel, tx.Tag.ToString(), true);
                (App.Current.MainWindow as MainWindow).GoContent(content, 3);
            }
            else
            {
                if (userViewModel != null)
                {
                    manageViewModel.SelectedUser = userViewModel;
                    var content = serviceProvider.GetRequiredService<UserControllers>();
                    var deviceControl =  serviceProvider.GetRequiredService<DeviceControlContent>();
                    deviceControl.InitPage(tx.Tag.ToString());
                    content.InitPage(tx.Tag.ToString(), (string)FindResource("LanguageKey_Code_Control_Device"));
                    content.GoCotent(deviceControl, 3);
                    
                    (App.Current.MainWindow as MainWindow).GoContent(content, 2);
                }
            }
        }

        private void btnToMediaManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var userViewModel = manageViewModel.Users.FirstOrDefault();
            if(manageViewModel.CurrentUser.Role == "user")
            {
                var content = serviceProvider.GetRequiredService<MediaManage>();
                (App.Current.MainWindow as MainWindow).GoContent(content, 2);
            }
            else
            {
                if (userViewModel != null)
                {
                    manageViewModel.SelectedUser = userViewModel;
                    var content = serviceProvider.GetRequiredService<UserControllers>();
                    (App.Current.MainWindow as MainWindow).GoContent(content, 2);
                }
            }            
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = ((sender as DataGrid).SelectedItem as DeviceViewModel)!;
            var userViewModel = manageViewModel.Users.FirstOrDefault(c => c.Account == viewModel.UserId);
            if (manageViewModel.CurrentUser.Role == "user")
            {
                var content = serviceProvider.GetRequiredService<MediaManage>();
                (App.Current.MainWindow as MainWindow).GoContent(content, 2);
            }
            else
            {
                if (userViewModel != null)
                {
                    manageViewModel.SelectedUser = userViewModel;
                    var content = serviceProvider.GetRequiredService<UserControllers>();
                    (App.Current.MainWindow as MainWindow).GoContent(content, 2);
                }
            }
        }

        private void btnToUserManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var content = serviceProvider.GetRequiredService<UserManage>();// new UserManage(manageViewModel.CurrentUser);
            (App.Current.MainWindow as MainWindow).GoContent(content, 2);
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var userViewModel = ((sender as DataGrid).SelectedItem as UserViewModel)!;
            if (userViewModel != null && userViewModel.Role == "user")
            {
                manageViewModel.SelectedUser = userViewModel;
                var content = serviceProvider.GetRequiredService<UserControllers>();
                (App.Current.MainWindow as MainWindow).GoContent(content, 2);
            }
        }

        private void btnToDeviceManage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var content = serviceProvider.GetRequiredService<DeviceManage>();
            (App.Current.MainWindow as MainWindow).GoContent(content, 3);
        }

        private void btnToMediaEdit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = ((sender as StackPanel).DataContext as MediaViewModel)!;
            manageViewModel.SelectedMedia = viewModel;
            var content = serviceProvider.GetRequiredService<MediaEdit>();
            (App.Current.MainWindow as MainWindow).GoContent(content, 2);
        }

        private void selectDevice_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var content = serviceProvider.GetRequiredService<DeviceManage>(); //new DeviceManage(userViewModel, true);
            (App.Current.MainWindow as MainWindow).GoContent(content, 3);
        }
    }
}
