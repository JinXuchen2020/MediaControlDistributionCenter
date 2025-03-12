

using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// 界面 的交互逻辑
    /// </summary>
    public partial class DeviceControlContent : FrameControl
    {
        private DeviceControlViewModel manageViewModel;

        public DeviceControlContent(DeviceControlViewModel deviceControlViewModel)
        {
            InitializeComponent();
            manageViewModel = deviceControlViewModel;
            DataContext = manageViewModel;
            InitPage("Brightness");
        }

        public void InitPage(string fun)
        {
            ChangePage(fun);
        }

        private void Menu_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            ChangePage(border.Tag.ToString());
        }

        private async void Restart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                MessageBox.Show("请先选择显示器！");
                return;
            }

            if (manageViewModel.CommandType == "Restart")
            {
                await manageViewModel.CurrentDevice.RestartCommand.ExecuteAsync("1");
            }
        }

        private async void Volume_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                MessageBox.Show("请先选择显示器！");
                return;
            }

            //音量設置
            Communication communication = new Communication();
            communication.Connect("192.168.1.3", "6767");
            //communication.StartHeart();
            string path = CommunicationCmd.CmdVolume + "80";
            bool t = await communication.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (t)
            {
                //SendState.Text += "命令处理成功\r\n";
            }
            else
            {
                //SendState.Text += "命令无法被处理\r\n";
            }
            AddVolume();
        }
        private async void AddVolume()
        {
            //亮度定時
            Communication communication = new Communication();
            communication.Connect("192.168.1.3", "6767");
            //communication.StartHeart();
            VolumeControl volumeControl = new VolumeControl();
            volumeControl.DateTime = DateTime.Now.AddMinutes(1).ToLongTimeString();
            volumeControl.JobPara = "1";
            volumeControl.Enable = true;
            volumeControl.DateTime = DateTime.Now.AddMinutes(2).ToLongTimeString();
            volumeControl.Volume = 60;

            string path = CommunicationCmd.CmdBrightness + JsonConvert.SerializeObject(volumeControl, Newtonsoft.Json.Formatting.Indented);
            bool t = await communication.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (t)
            {
                //SendState.Text += "命令处理成功\r\n";
            }
            else
            {
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        private void Brightness_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                MessageBox.Show("请先选择显示器！");
                return;
            }
            if (manageViewModel.CommandRTValue == null)
            {
                MessageBox.Show("请先选择值！");
                return;
            }


            if (manageViewModel.CommandType == "Brightness")
            {
                manageViewModel.CurrentDevice.ChangeBrightnessCommand.Execute(manageViewModel.CommandRTValue);
            }

            if (manageViewModel.CommandType == "Volume")
            {
                manageViewModel.CurrentDevice.ChangeVolumeCommand.Execute(manageViewModel.CommandRTValue);
            }

            ////亮度
            //Communication communication = new Communication();
            //communication.Connect("192.168.1.3", "6767");
            ////communication.StartHeart();
            //string path = CommunicationCmd.CmdBrightness + "70";
            //bool t = await communication.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            //if (t)
            //{
            //    //SendState.Text += "命令处理成功\r\n";
            //}
            //else
            //{
            //    //SendState.Text += "命令无法被处理\r\n";
            //}
            //AddBrightness();
        }
        private async void AddBrightness()
        {
            //亮度定時
            Communication communication = new Communication();
            communication.Connect("192.168.1.3", "6767");
            //communication.StartHeart();
            BrightnessControl brightnessControl = new BrightnessControl();
            brightnessControl.Type = "1";
            brightnessControl.DateTime = DateTime.Now.AddMinutes(1).ToLongTimeString();
            brightnessControl.JobPara = "1";
            brightnessControl.Enable = true;
            brightnessControl.DateTime = DateTime.Now.AddMinutes(2).ToLongTimeString();
            brightnessControl.Brightness =90;

            string path = CommunicationCmd.CmdBrightness + JsonConvert.SerializeObject(brightnessControl, Newtonsoft.Json.Formatting.Indented);
            bool t = await communication.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (t)
            {
                //SendState.Text += "命令处理成功\r\n";
            }
            else
            {
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = ((sender as DataGrid).SelectedItem as DeviceViewModel);
            manageViewModel.CurrentDevice = viewModel;
            if (viewModel != null)
            {
                manageViewModel.GetDeviceTimeControlsCommand.Execute(null);
                dgTimeControls.ItemsSource = manageViewModel.DeviceTimeControls;

                if (!string.IsNullOrEmpty(manageViewModel.CommandTypeColumnName))
                {
                    var valueColumn = new DataGridTextColumn()
                    {
                        Binding = new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                        Header = manageViewModel.CommandTypeColumnName
                    };
                    dgTimeControls.Columns.Insert(1, valueColumn);
                }
            }
            else
            {
                var valueColumn = dgTimeControls.Columns.FirstOrDefault(c => c.Header.ToString() == manageViewModel.CommandTypeColumnName);
                dgTimeControls.Columns.Remove(valueColumn);
            }
        }

        private void btnAddTimeControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = new DeviceTimeControlViewModel();
            if(manageViewModel.CurrentDevice == null)
            {
                MessageBox.Show("请先选择显示器！");
                return;
            }

            viewModel.DeviceId = manageViewModel.CurrentDevice.DeviceId;
            viewModel.Type = manageViewModel.CommandType;
            viewModel.Status = 1;
            viewModel.SetGridColumnName();
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDeleteTimeControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItems = manageViewModel.DeviceTimeControls.Where(c => c.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("请先选择记录！");
                return;
            }

            manageViewModel.DeleteBatchCommand.Execute(null);
        }

        private void btnTimeControlSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = ((sender as System.Windows.Controls.Button).DataContext as DeviceTimeControlViewModel)!;

            manageViewModel.SaveTimeControlCommand.Execute(viewModel);
        }

        private void UpdatePageContent(string commandType)
        {
            var manageViewModel = (DataContext as DeviceControlViewModel)!;
            manageViewModel.CommandType = commandType;
            manageViewModel.CurrentDevice = null;
            manageViewModel.CommandRTValue = null;
            manageViewModel.DeviceTimeControls = null;
            dgDevices.SelectedItem = null;

            switch (commandType)
            {
                case "Brightness":
                    manageViewModel.CommandTypeName = "亮度：";
                    manageViewModel.CommandTypeHint = "请输入亮度";
                    manageViewModel.CommandTypeDesciption = "[实时控制] 命令会覆盖当前正在执行的 [定时控制] 命令，直至下个时间点的 [定时控制]，命令生效";
                    manageViewModel.CommandTypeColumnName = "亮度值(%)";
                    break;
                case "Volume":
                    manageViewModel.CommandTypeName = "音量：";
                    manageViewModel.CommandTypeHint = "请输入音量";
                    manageViewModel.CommandTypeDesciption = "[实时控制] 命令会覆盖当前正在执行的 [定时控制] 命令，直至下个时间点的 [定时控制]，命令生效";
                    manageViewModel.CommandTypeColumnName = "音量值 (%)";

                    break;
                case "TimeSync":
                    manageViewModel.CommandTypeName = "时区：";
                    manageViewModel.CommandTypeHint = "";
                    manageViewModel.CommandTypeDesciption = "手动是根据当前所选时区的时间对时，NTP是根据所选服务器和时区对时，射频是根据对时基准设备对时，GPS是根据GPS卫星进行对时。";
                    manageViewModel.CommandTypeColumnName = "手动";
                    break;
                case "Restart":
                    manageViewModel.CommandTypeName = "重启：";
                    manageViewModel.CommandTypeHint = "立即重启后,播放器将在20s左右完成重启,在此期间,播放器会处于离线状态.请在播放器重新上线后,再进行其他操作。";
                    manageViewModel.CommandTypeDesciption = "[实时控制] 命令会覆盖当前正在执行的 [定时控制] 命令，直至下个时间点的 [定时控制]，命令生效";
                    manageViewModel.CommandTypeColumnName = "";
                    break;
                default:
                    break;
            }

            dgDevices.SelectedItem = manageViewModel.Devices.FirstOrDefault();
        }

        private void btnTimeSyncReset_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as DeviceControlViewModel)!;
            if (manageViewModel.CurrentDevice == null)
            {
                MessageBox.Show("请先选择显示器！");
                return;
            }
        }

        private void btnTimeSyncConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                MessageBox.Show("请先选择显示器！");
                return;
            }

            if (manageViewModel.CommandTypeColumnName == "手动")
            {
                var currentTime = DateTime.Now;
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(manageViewModel.CommandRTValue!);
                manageViewModel.CurrentDevice.TimeSyncCommand.Execute(TimeZoneInfo.ConvertTime(currentTime, timeZone));
            }

            if (manageViewModel.CommandTypeColumnName == "GPS")
            {
                manageViewModel.CurrentDevice.TimeGPSSyncCommand.Execute(DateTime.Now);
            }
        }

        private void ChangeTimeSyncMode_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                manageViewModel.CommandTypeColumnName = checkBox.Content?.ToString();
            }
        }


        #region 私有方法
        /// <summary>
        /// 将字符串转换为Color
        /// </summary>
        /// <param name="color">带#号的16进制颜色</param>
        /// <returns></returns>
        Color HexToColor(string color)
        {
            int red, green, blue, alpha;
            char[] rgb;
            color = color.TrimStart('#');
            color = Regex.Replace(color.ToLower(), "[g-zG-Z]", "");
            switch (color.Length)
            {
                case 3:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0].ToString() + rgb[0].ToString(), 16);
                    green = Convert.ToInt32(rgb[1].ToString() + rgb[1].ToString(), 16);
                    blue = Convert.ToInt32(rgb[2].ToString() + rgb[2].ToString(), 16);
                    return Color.FromRgb(Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));

                case 4:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0].ToString() + rgb[0].ToString(), 16);
                    green = Convert.ToInt32(rgb[1].ToString() + rgb[1].ToString(), 16);
                    blue = Convert.ToInt32(rgb[2].ToString() + rgb[2].ToString(), 16);
                    alpha = Convert.ToInt32(rgb[3].ToString() + rgb[3].ToString(), 16);
                    return Color.FromArgb(Convert.ToByte(alpha), Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));

                case 6:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0].ToString() + rgb[1].ToString(), 16);
                    green = Convert.ToInt32(rgb[2].ToString() + rgb[3].ToString(), 16);
                    blue = Convert.ToInt32(rgb[4].ToString() + rgb[5].ToString(), 16);
                    return Color.FromRgb(Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));

                case 8:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0].ToString() + rgb[1].ToString(), 16);
                    green = Convert.ToInt32(rgb[2].ToString() + rgb[3].ToString(), 16);
                    blue = Convert.ToInt32(rgb[4].ToString() + rgb[5].ToString(), 16);
                    alpha = Convert.ToInt32(rgb[6].ToString() + rgb[7].ToString(), 16);
                    return Color.FromArgb(Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue), Convert.ToByte(alpha));
                default:
                    return Colors.Black;

            }
        }

        /// <summary>
        /// 界面切換
        /// </summary>
        /// <param name="fun">功能名字</param>
        void ChangePage(string fun)
        {
            UpdatePageContent(fun);
            switch (fun)
            {
                case "Brightness":
                    Volume.Background = new SolidColorBrush(Colors.Transparent);
                    TimeSync.Background = new SolidColorBrush(Colors.Transparent);
                    Restart.Background = new SolidColorBrush(Colors.Transparent);
                    Brightness.Background = new SolidColorBrush(HexToColor("#30479C"));
                    //界面切換
                    //VolumePara.Visibility = System.Windows.Visibility.Collapsed;
                    //TimeSyncPara.Visibility = System.Windows.Visibility.Collapsed;
                    //RestartPara.Visibility = System.Windows.Visibility.Collapsed;
                    //BrightnessPara.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Volume":

                    Brightness.Background = new SolidColorBrush(Colors.Transparent);
                    TimeSync.Background = new SolidColorBrush(Colors.Transparent);
                    Restart.Background = new SolidColorBrush(Colors.Transparent);
                    Volume.Background = new SolidColorBrush(HexToColor("#30479C"));

                    //界面切換
                    //VolumePara.Visibility = System.Windows.Visibility.Collapsed;
                    //TimeSyncPara.Visibility = System.Windows.Visibility.Collapsed;
                    //BrightnessPara.Visibility = System.Windows.Visibility.Collapsed;
                    //VolumePara.Visibility = System.Windows.Visibility.Visible;

                    break;
                case "TimeSync":

                    Volume.Background = new SolidColorBrush(Colors.Transparent);
                    Brightness.Background = new SolidColorBrush(Colors.Transparent);
                    Restart.Background = new SolidColorBrush(Colors.Transparent);
                    TimeSync.Background = new SolidColorBrush(HexToColor("#30479C"));

                    //界面切換 
                    //VolumePara.Visibility = System.Windows.Visibility.Collapsed;
                    //TimeSyncPara.Visibility = System.Windows.Visibility.Collapsed;
                    //BrightnessPara.Visibility = System.Windows.Visibility.Collapsed;
                    //TimeSyncPara.Visibility = System.Windows.Visibility.Visible;


                    break;
                case "Restart":
                    Volume.Background = new SolidColorBrush(Colors.Transparent);
                    TimeSync.Background = new SolidColorBrush(Colors.Transparent);
                    Brightness.Background = new SolidColorBrush(Colors.Transparent);
                    Restart.Background = new SolidColorBrush(HexToColor("#30479C"));

                    //界面切換
                    //VolumePara.Visibility = System.Windows.Visibility.Collapsed;
                    //TimeSyncPara.Visibility = System.Windows.Visibility.Collapsed;
                    //BrightnessPara.Visibility = System.Windows.Visibility.Collapsed;
                    //RestartPara.Visibility = System.Windows.Visibility.Visible;

                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}
