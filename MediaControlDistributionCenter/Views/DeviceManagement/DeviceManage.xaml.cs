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
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;

namespace MediaControlDistributionCenter.Views.DeviceManagement
{
    /// <summary>
    /// DeviceManage.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceManage : UserControl
    {
        private readonly DeviceManageViewModel manageViewModel;

        public DeviceManage(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, DeviceManageViewModel deviceManageViewModel)
        {
            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                deviceManageViewModel.ShowNavigation = true;
                deviceManageViewModel.CurrentUser = dashboardViewModel.CurrentUser;
                deviceManageViewModel.LoginUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                deviceManageViewModel.CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
                deviceManageViewModel.LoginUser = dashboardViewModel.CurrentUser;
            }

            manageViewModel = deviceManageViewModel;
            manageViewModel.LoadData();
            DataContext = deviceManageViewModel;

            this.Unloaded += DeviceManage_Unloaded;

            InitializeComponent();
        }

        private void DeviceManage_Unloaded(object sender, RoutedEventArgs e)
        {
            manageViewModel.SelectDisabled = 1;
        }

        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
            }
        }

        private void btnGroupSave_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = ((sender as Button).DataContext as DeviceGroupViewModel)!;
            manageViewModel.CreateGroupCommand.Execute(groupViewModel);
        }

        private void btnGroupAdd_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = new DeviceGroupViewModel();
            groupViewModel.UserId = manageViewModel.CurrentUser.Account;
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var groupViewModel = ((sender as DockPanel).DataContext as DeviceGroupViewModel)!;
            manageViewModel.SelectedGroup = groupViewModel;
            manageViewModel.LoadData();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            if(viewModel.Enabled == 0)
            {
                viewModel.Enabled = 1;
            }
            else
            {
                viewModel.Enabled = 0;
            }

            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    manageViewModel.EnableDeviceCommand.Execute(viewModel);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = manageViewModel.CreateDevice();
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Devices.Where(c => c.IsSelected);

            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnChangeGroupConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.SelectedGroupId == null || manageViewModel.SelectedGroupId == -1)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_115");
                var dialog = new ResultConfirmDialog(manageViewModel);
                DialogHost.Show(dialog, Helpers.Constants.LoginDialogHostId);
                return;
            }
            manageViewModel.ChangeGroupCommand.Execute(null);
        }

        private void btnDeviceSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            manageViewModel.SaveDeviceCommand.Execute(viewModel);
        }

        private void btnDeviceCancel_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            manageViewModel.CloseDialogCommand.Execute(null);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            manageViewModel.ConnectDeviceCommand.Execute(viewModel);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox.IsChecked.GetValueOrDefault())
            {
                foreach (var item in manageViewModel.Devices)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (var item in manageViewModel.Devices)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void btnGroupDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceGroupViewModel)!;
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

        private void ShowDisabledDevices_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(manageViewModel.SelectDisabled == 0)
            {
                manageViewModel.SelectDisabled = 1;
            }
            else
            {
                manageViewModel.SelectDisabled = 0;
            }

            manageViewModel.LoadData();
        }
    }
}
