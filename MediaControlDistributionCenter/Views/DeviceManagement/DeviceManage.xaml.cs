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
            manageViewModel.SetValues();
            DataContext = deviceManageViewModel;

            InitializeComponent();
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
            manageViewModel.SetValues(groupViewModel.Id);
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

            manageViewModel.EnableDeviceCommand.Execute(viewModel);
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = new DeviceViewModel();
            viewModel.UserId = manageViewModel.CurrentUser.Account;
            viewModel.OwnerName = userService.GetAll(new Services.DTO.Models.UserDto { Account = manageViewModel.CurrentUser.Account}).GetAwaiter().GetResult().Data!.First().Company;
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
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            var selectedItems = manageViewModel.Devices.Where(c => c.IsSelected);
            if (manageViewModel.SelectedGroupId != -1)
            {
                return;
            }

            manageViewModel.ChangeGroupCommand.Execute(null);
        }

        private void btnDeviceSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceViewModel)!;
            viewModel.Resolution = $"{viewModel.Width}*{viewModel.Height}";
            viewModel.Group = manageViewModel.DeviceGroups.FirstOrDefault(c => c.Id == viewModel.GroupId)?.Name ?? "未分组";
            viewModel.StatusText = viewModel.GetStatus();
            viewModel.LastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            viewModel.EnableBtnContent = viewModel.Status == -1 ? "启用" : "停用";

            manageViewModel.SaveDeviceCommand.Execute(viewModel);
        }

        private void btnDeviceCancel_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceManageViewModel)!;
            manageViewModel.CloseDialogCommand.Execute(null);
        }
    }
}
