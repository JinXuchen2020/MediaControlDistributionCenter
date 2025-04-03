
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// UserChangeGroupDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPublishDialog : UserControl
    {
        private readonly MediaDevicesViewModel manageViewModel;

        public MediaPublishDialog(MediaDevicesViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            manageViewModel = viewModel;
            manageViewModel.LoadData();

            this.Loaded += MediaPublishDialog_Loaded;
        }

        private void MediaPublishDialog_Loaded(object sender, RoutedEventArgs e)
        {
            //if (manageViewModel.ConnectionMode.Mode == "Local")
            //{
            //    var communication = App.ServicesProvider.GetRequiredService<Communication>();
            //    foreach (var device in manageViewModel.Devices)
            //    {
            //        device.ConnectCommand.Execute(communication);
            //    }
            //}
        }

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                await manageViewModel.PublishCommand.ExecuteAsync(null);

                var screenCount = manageViewModel.Devices.Where(c => c.IsSelected).Count();
                manageViewModel.CurrentMedia.ScreensCount = screenCount;
                await manageViewModel.SaveCommand.ExecuteAsync(null);

                if (manageViewModel.PublishDevices.Count > 0)
                {
                    MaterialDesignThemes.Wpf.DialogHost.Close(Constants.DialogHostId);
                    manageViewModel.ShowConfirmDialogCommand.Execute(null);
                }
            });
        }

        private void SelectDevicesAll_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox.IsChecked.GetValueOrDefault())
            {
                foreach (var item in manageViewModel.Devices)
                {
                    if (item.IsConnected)
                    {
                        item.IsSelected = true;
                    }
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
    }
}
