using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.MediaManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// UserControls.xaml 的交互逻辑
    /// </summary>
    public partial class UserControllers : UserControl
    {
        public UserControllers(UserViewModel userViewModel)
        {
            InitializeComponent();
            DataContext = new UserControllerViewModel(userViewModel);
            TabContentControl.Content = new MediaManage(userViewModel);
        }

        private void TopMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock topMenu = sender as TextBlock;
            var menuName = topMenu!.Name;

            var viewModel = (topMenu.DataContext as UserControllerViewModel)!;
            viewModel.CurrentTabName = menuName;
            viewModel.CurrentPageName = topMenu.Text;

            switch (menuName)
            {
                case "MediaManage":
                    GoCotent(new MediaManage(viewModel.CurrentUser), 1);
                    break;
                case "DeviceManage":
                    GoCotent(new DeviceManage(viewModel.CurrentUser), 2);
                    break;
                case "DeviceControl":
                    GoCotent(new DeviceControlContent(viewModel.CurrentUser), 3);
                    break;
                case "Settings":
                    GoCotent(new UserSettingsContent(viewModel.CurrentUser), 4);
                    break;
            }
        }

        public void GoCotent(UserControl contet, int menuIndex)
        {
            TabContentControl.Content = contet;
            switch (menuIndex)
            {
                case 1:
                    MediaManage.Opacity = 1;
                    DeviceManage.Opacity = 0.7;
                    DeviceControl.Opacity = 0.7;
                    Settings.Opacity = 0.7;
                    break;
                case 2:
                    MediaManage.Opacity = 0.7;
                    DeviceManage.Opacity = 1;
                    DeviceControl.Opacity = 0.7;
                    Settings.Opacity = 0.7;
                    break;
                case 3:
                    MediaManage.Opacity = 0.7;
                    DeviceManage.Opacity = 0.7;
                    DeviceControl.Opacity = 1;
                    Settings.Opacity = 0.7;
                    break;
                case 4:
                    MediaManage.Opacity = 0.7;
                    DeviceManage.Opacity = 0.7;
                    DeviceControl.Opacity = 0.7;
                    Settings.Opacity = 1;
                    break;
                default:
                    break;
            }
        }
    }
}
