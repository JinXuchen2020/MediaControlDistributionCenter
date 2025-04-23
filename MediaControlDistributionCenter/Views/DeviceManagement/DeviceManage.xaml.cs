using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            this.Loaded += DeviceManage_Loaded;
            this.Unloaded += DeviceManage_Unloaded;

            InitializeComponent();
        }

        private void DeviceManage_Loaded(object sender, RoutedEventArgs e)
        {
            manageViewModel.DetectConnectedDeviceCommand.Execute(null);
            //if (manageViewModel.ConnectionMode.Mode == "Local")
            //{
            //    var communication = App.ServicesProvider.GetRequiredService<Communication>();
            //    foreach (var device in manageViewModel.Devices)
            //    {
            //        device.ConnectCommand.Execute(communication);
            //    }
            //}
        }

        private void DeviceManage_Unloaded(object sender, RoutedEventArgs e)
        {
            manageViewModel.SelectDisabled = "1";
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
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            if (viewModel.Enabled == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_133");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnEnable_Click(object sender, RoutedEventArgs e)
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
            if(manageViewModel.SelectDisabled == "0")
            {
                manageViewModel.SelectDisabled = "1";
            }
            else
            {
                manageViewModel.SelectDisabled = "0";
            }

            manageViewModel.LoadData();
        }

        private void btnDetectConnectedDevice(object sender, MouseButtonEventArgs e)
        {
            if (manageViewModel.IsSearching)
            {
                return;
            }
            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.IsSearching = true;
                await manageViewModel.DetectConnectedDeviceCommand.ExecuteAsync(null);
                manageViewModel.IsSearching = false;
            });
        }

        private void btnActivate_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);
                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    await manageViewModel.ActivateDeviceCommand.ExecuteAsync(viewModel);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    manageViewModel.DeleteDeviceCommand.Execute(viewModel);
                }

                manageViewModel.CanDelete = null;
            });
        }
    }
}
