using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using Newtonsoft.Json;
using System.Data;
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
        private readonly DeviceControlViewModel manageViewModel;

        public DeviceControlContent(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, DeviceControlViewModel deviceControlViewModel)
        {
            InitializeComponent();
            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                deviceControlViewModel.ShowNavigation = true;
                deviceControlViewModel.CurrentUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                deviceControlViewModel.CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
            }
            manageViewModel = deviceControlViewModel;
            manageViewModel.LoadData();
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
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.CommandRTValue = "1";
            manageViewModel.ExecuteRealTimeControlCommand.Execute(null);
        }

        private async void Volume_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
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
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }
            if (manageViewModel.CommandRTValue == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Control_Tooltip_117");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ExecuteRealTimeControlCommand.Execute(null);

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
            if (manageViewModel.CurrentDevice != null)
            {
                manageViewModel.ConnectDeviceCommand.Execute(null);
                RefreshData();
            }
        }

        private void btnAddTimeControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(manageViewModel.CurrentDevice == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (manageViewModel.CommandType == "Restart")
            {
                manageViewModel.CommandRTValue = "1";
            }

            var viewModel = new DeviceTimeControlViewModel()
            {
                DeviceId = manageViewModel.CurrentDevice.DeviceId,
                Type = manageViewModel.CommandType,
                RepeatMode = "day",
                ExecuteMethod = "SCHEDULED",
                Status = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                UserAccount = manageViewModel.CurrentUser.Account,
            };

            viewModel.SetGridColumnName();
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnTimeControlSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as DeviceTimeControlViewModel)!;
            manageViewModel.SaveTimeControlCommand.Execute(viewModel);
            dgTimeControls.ItemsSource = manageViewModel.DeviceTimeControls;
        }

        private void btnDeleteTimeControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItems = manageViewModel.DeviceTimeControls.Where(c => c.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Control_Tooltip_118");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.DeleteBatchCommand.Execute(null);
        }

        private void btnPublish_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItems = manageViewModel.DeviceTimeControls.Where(c => c.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Control_Tooltip_118");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ExecuteScheduleControlCommand.Execute(null);
        }

        private void RefreshData()
        {
            if (manageViewModel.CurrentDevice != null)
            {
                manageViewModel.GetDeviceTimeControlsCommand.Execute(null);
                dgTimeControls.ItemsSource = manageViewModel.DeviceTimeControls;

                //if (!string.IsNullOrEmpty(manageViewModel.CommandTypeColumnName))
                //{
                //    var valueColumn = new DataGridTextColumn()
                //    {
                //        Binding = new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                //        Header = manageViewModel.CommandTypeColumnName
                //    };
                //    dgTimeControls.Columns.Insert(1, valueColumn);
                //}
            }
        }

        private void UpdatePageContent(string commandType)
        {
            if (commandType == manageViewModel.CommandTypeColumnName)
            {
                return;
            }

            //var valueColumn = dgTimeControls.Columns.FirstOrDefault(c => c.Header.ToString() == manageViewModel.CommandTypeColumnName);
            //dgTimeControls.Columns.Remove(valueColumn);

            manageViewModel.CommandType = commandType;
            manageViewModel.CommandRTValue = null;
            manageViewModel.DeviceTimeControls = null;

            switch (commandType)
            {
                case "Brightness":
                    manageViewModel.CommandTypeName = (string)FindResource("LanguageKey_Code_Control_Tooltip_119");
                    manageViewModel.CommandTypeHint = "请输入亮度";
                    manageViewModel.CommandTypeDesciption = (string)FindResource("LanguageKey_Code_Control_Tooltip_123");
                    manageViewModel.CommandTypeColumnName = (string)FindResource("LanguageKey_Code_Control_Tooltip_119");
                    break;
                case "Volume":
                    manageViewModel.CommandTypeName = (string)FindResource("LanguageKey_Code_Control_Tooltip_120");
                    manageViewModel.CommandTypeHint = "请输入音量";
                    manageViewModel.CommandTypeDesciption = (string)FindResource("LanguageKey_Code_Control_Tooltip_123");
                    manageViewModel.CommandTypeColumnName = (string)FindResource("LanguageKey_Code_Control_Tooltip_120");

                    break;
                case "TimeSync":
                    manageViewModel.CommandTypeName = (string)FindResource("LanguageKey_Code_Control_Tooltip_121");
                    manageViewModel.CommandTypeHint = "";
                    manageViewModel.CommandTypeDesciption = (string)FindResource("LanguageKey_Code_Control_Tooltip_124");
                    manageViewModel.CommandTypeColumnName = "manual";
                    break;
                case "Restart":
                    manageViewModel.CommandTypeName = (string)FindResource("LanguageKey_Code_Control_Tooltip_122");
                    manageViewModel.CommandTypeHint = (string)FindResource("LanguageKey_Code_Control_Tooltip_125");
                    manageViewModel.CommandTypeDesciption = (string)FindResource("LanguageKey_Code_Control_Tooltip_123");
                    manageViewModel.CommandTypeColumnName = "";
                    break;
                default:
                    break;
            }

            dgDevices.SelectedItem = dgDevices.SelectedItem ?? manageViewModel.Devices.FirstOrDefault();
            RefreshData();
        }

        private void btnTimeSyncReset_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.CommandRTValue = null;
        }

        private void btnTimeSyncConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.CurrentDevice == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_114");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (manageViewModel.CommandRTValue == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Control_Tooltip_117");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ExecuteRealTimeTimeSyncCommand.Execute(null);
        }

        private void ChangeTimeSyncMode_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as RadioButton;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                manageViewModel.CommandTypeColumnName = checkBox.Tag?.ToString();
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

        private void SelectWeekDay_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tagString = button.Tag.ToString()!;
            var viewModel = (button.DataContext as DeviceTimeControlViewModel)!;
            if (viewModel.RepeatString.Contains(tagString))
            {
                viewModel.RepeatString = viewModel.RepeatString.Replace($"{tagString}#", string.Empty);
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1E23"));
            }
            else
            {
                viewModel.RepeatString = $"{viewModel.RepeatString}{tagString}#";
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30479C"));
            }
        }

        private void SelectMonthDay_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tagString = button.Tag.ToString()!;
            var viewModel = FindParentDataContext(button);
            if (viewModel.RepeatString.Contains(tagString))
            {
                viewModel.RepeatString = viewModel.RepeatString.Replace($"{tagString}#", string.Empty);
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1E23"));
            }
            else
            {
                viewModel.RepeatString = $"{viewModel.RepeatString}{tagString}#";
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30479C"));
            }
        }

        private void SelectQuarter_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var viewModel = (button.DataContext as DeviceTimeControlViewModel)!;
            if (viewModel.RepeatMode == "quarter")
            {
                switch (button.Tag.ToString())
                {
                    case "quarter1":
                        viewModel.RepeatString = "Q1";
                        break;
                    case "quarter2":
                        viewModel.RepeatString = "Q2";
                        break;
                    case "quarter3":
                        viewModel.RepeatString = "Q3";
                        break;
                    case "quarter4":
                        viewModel.RepeatString = "Q4";
                        break;
                }
            }
        }

        protected DeviceTimeControlViewModel FindParentDataContext(Visual child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                if (parentObject is FrameworkElement parent && parent.DataContext is DeviceTimeControlViewModel)
                {
                    return (parent.DataContext as DeviceTimeControlViewModel);
                }

                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null;
        }

        private void RepeatMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = ((sender as ComboBox).DataContext as DeviceTimeControlViewModel)!;
            viewModel.RepeatString = string.Empty;
        }

        private void SelectQuarterMonth_Changed(object sender, SelectionChangedEventArgs e)
        {
            var button = sender as ComboBox;
            var month = button.SelectedValue;
            var viewModel = (button.DataContext as DeviceTimeControlViewModel)!;
            viewModel.RepeatString += $"";
        }

        private void SelectYearMonth_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tagString = button.Tag.ToString()!;
            var viewModel = (button.DataContext as DeviceTimeControlViewModel)!;
            if (viewModel.RepeatString.Contains(tagString))
            {
                viewModel.RepeatString = viewModel.RepeatString.Replace($"{tagString}#", string.Empty);
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1E23"));
            }
            else
            {
                viewModel.RepeatString = $"{viewModel.RepeatString}{tagString}#";
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30479C"));
            }
        }
    }
}
