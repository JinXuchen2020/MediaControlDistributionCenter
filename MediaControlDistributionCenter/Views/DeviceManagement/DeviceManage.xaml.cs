using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Data;
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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using MediaControlDistributionCenter.Views.UserManagement;
using MaterialDesignThemes.Wpf;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using MediaControlDistributionCenter.Services;

namespace MediaControlDistributionCenter.Views.DeviceManagement
{
    /// <summary>
    /// DeviceManage.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceManage : UserControl
    {
        private IDeviceService deviceService;
        public DeviceManage(UserViewModel userViewModel, bool showNavigation = false)
        {
            deviceService = new DeviceService();
            var deviceGroups = deviceService.GetDeviceGroups(userViewModel.Id).GetAwaiter().GetResult().ToList();
            deviceGroups.Insert(0, new DeviceGroupViewModel(new DeviceGroup
            {
                Id = -1,
                Name = "全部",
                UserId = userViewModel.Id,
            }, true));

            var devices = deviceService.GetDevices(userViewModel.Id).GetAwaiter().GetResult();
            var viewModel = new DeviceManageViewModel(userViewModel, deviceGroups, devices);
            viewModel.ShowNavigation = showNavigation;
            DataContext = viewModel;

            InitializeComponent();
        }

        private void btnGroupSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as DeviceManageViewModel)!;

            var groupViewModel = ((sender as Button).DataContext as DeviceGroupViewModel)!;
            groupViewModel.Id = SQLite.InserTable(groupViewModel.ToModel());
            viewModel.DeviceGroups.Add(groupViewModel);

            viewModel.CloseDialogCommand.Execute(null);
        }

        private void btnGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var groupViewModel = new DeviceGroupViewModel();
            groupViewModel.UserId = manageViewModel.CurrentUser.Id;
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            manageViewModel.DeviceGroups.First(c => c.IsSelected).IsSelected = false;
            var groupViewModel = ((sender as StackPanel).DataContext as DeviceGroupViewModel)!;
            groupViewModel.IsSelected = true;

            if (groupViewModel.Id != -1)
            {
                var devices = deviceService.GetDevices(manageViewModel.CurrentUser.Id, groupViewModel.Id).GetAwaiter().GetResult();
                manageViewModel.Devices = new ObservableCollection<DeviceViewModel>(devices);
            }
            else
            {
                var devices = deviceService.GetDevices(manageViewModel.CurrentUser.Id).GetAwaiter().GetResult();
                manageViewModel.Devices = new ObservableCollection<DeviceViewModel>(devices);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(viewModel.Clone());
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            if(viewModel.Status != -1)
            {
                viewModel.Status = -1;
                viewModel.StatusText = "停用";
                viewModel.EnableBtnContent = "启用";
            }
            else
            {
                viewModel.Status = 1;
                viewModel.StatusText = "在线";
                viewModel.EnableBtnContent = "停用";
            }
            SQLite.UpdateTable(viewModel.ToModel());
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var viewModel = new DeviceViewModel();
            viewModel.UserId = manageViewModel.CurrentUser.Id;
            viewModel.OwnerName = SQLite.QueryTable<User>().First(x => x.Id == viewModel.UserId).Name;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnChangeGroupConfirm_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var selectedItems = manageViewModel.Devices.Where(c => c.IsSelected);

            if(manageViewModel.SelectedGroupId != -1)
            {
                foreach (var item in selectedItems)
                {
                    item.GroupId = manageViewModel.SelectedGroupId;
                    item.Group = manageViewModel.DeviceGroups.FirstOrDefault(c => c.Id == manageViewModel.SelectedGroupId)?.Name ?? "未分组";

                    item.IsSelected = false;
                    SQLite.UpdateTable(item.ToModel());
                }
            }

            manageViewModel.SelectedGroupId = null;
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void btnDeviceSave_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;

            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            viewModel.Resolution = $"{viewModel.Width}*{viewModel.Height}";
            viewModel.Group = manageViewModel.DeviceGroups.FirstOrDefault(c => c.Id == viewModel.GroupId)?.Name ?? "未分组";
            viewModel.StatusText = viewModel.GetStatus();
            viewModel.LastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            viewModel.EnableBtnContent = viewModel.Status == -1 ? "启用" : "停用";
            var devices = manageViewModel.Devices.ToList();
            var existIndex = devices.FindIndex(c => c.Id == viewModel.Id);
            if (existIndex != -1)
            {
                devices.RemoveAt(existIndex);
                devices.Insert(existIndex, viewModel);
                manageViewModel.Devices = new ObservableCollection<DeviceViewModel>(devices);
                SQLite.UpdateTable(viewModel.ToModel());
            }
            else
            {
                viewModel.Id = SQLite.InserTable(viewModel.ToModel());
                manageViewModel.Devices.Add(viewModel);
            }

            manageViewModel.CloseDialogCommand.Execute(null);
        }

        private void btnDeviceCancel_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            manageViewModel.CloseDialogCommand.Execute(null);
        }
    }
}
