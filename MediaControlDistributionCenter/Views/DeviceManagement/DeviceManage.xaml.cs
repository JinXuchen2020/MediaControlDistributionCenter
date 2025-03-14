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
        private readonly IUserService userService;

        public DeviceManage(DeviceManageViewModel deviceManageViewModel, IUserService userService)
        {
            this.userService = userService;
            manageViewModel = deviceManageViewModel;
            manageViewModel.LoadData();
            DataContext = deviceManageViewModel;

            InitializeComponent();
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
            var groupViewModel = ((sender as StackPanel).DataContext as DeviceGroupViewModel)!;
            manageViewModel.LoadData(groupViewModel.Id == -1 ? null : groupViewModel.Id);
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

            manageViewModel.EnableDeviceCommand.Execute(viewModel);
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = new DeviceViewModel();
            viewModel.UserId = manageViewModel.CurrentUser.Account;
            viewModel.OwnerName = userService.GetAll(new UserDto { Account = manageViewModel.CurrentUser.Account}).GetAwaiter().GetResult().Data!.First().Company;
            viewModel.DeviceId = "";
            viewModel.Status = 1;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Devices.Where(c => c.IsSelected);

            if (selectedMedias.Count() == 0)
            {
                MessageBox.Show("请选择显示器！");
                return;
            }

            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnChangeGroupConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.SelectedGroupId == null || manageViewModel.SelectedGroupId == -1)
            {
                MessageBox.Show("请选择有效分组！");
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
    }
}
