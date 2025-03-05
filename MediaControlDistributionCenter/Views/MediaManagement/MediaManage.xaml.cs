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
using SqlSugar;
using System.IO;
using MediaControlDistributionCenter.Helpers;
using Path = System.IO.Path;
using MediaControlDistributionCenter.Converters;
using Newtonsoft.Json;

namespace MediaControlDistributionCenter.Views.MediaManagement
{
    /// <summary>
    /// MediaManage.xaml 的交互逻辑
    /// </summary>
    public partial class MediaManage : UserControl
    {
        private IDeviceService deviceService; 
        private IFileService fileService;
        public MediaManage(UserViewModel userViewModel, bool showNavigation = false)
        {
            deviceService = new DeviceService();
            fileService = new FileServiceLocal();

            var groups = deviceService.GetMediaGroups(userViewModel.Id).GetAwaiter().GetResult().ToList();
            groups.Insert(0, new MediaGroupViewModel(new MediaGroup
            {
                Id = -1,
                Name = "全部",
                UserId = userViewModel.Id,
            }, true));
            var medias = deviceService.GetMedias(userViewModel.Id).GetAwaiter().GetResult();
            var viewModel = new MediaManageViewModel(userViewModel, groups, medias);
            viewModel.ShowNavigation = showNavigation;
            DataContext = viewModel;

            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as MediaManageViewModel)!;

            var groupViewModel = ((sender as Button).DataContext as MediaGroupViewModel)!;

            groupViewModel.Id = SQLite.InserTable(groupViewModel.ToModel());
            viewModel.MediaGroups.Add(groupViewModel);
            viewModel.CloseDialogCommand.Execute(null);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(new MediaGroupViewModel());
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            manageViewModel.MediaGroups.First(c => c.IsSelected).IsSelected = false;
            var groupViewModel = ((sender as StackPanel).DataContext as MediaGroupViewModel)!;
            groupViewModel.IsSelected = true;

            if (groupViewModel.Id != -1)
            {
                var medias = deviceService.GetMedias(manageViewModel.CurrentUser.Id, groupViewModel.Id).GetAwaiter().GetResult();
                manageViewModel.Medias = new ObservableCollection<MediaViewModel>(medias);
            }
            else
            {
                var medias = deviceService.GetMedias(manageViewModel.CurrentUser.Id).GetAwaiter().GetResult();
                manageViewModel.Medias = new ObservableCollection<MediaViewModel>(medias);
            }
        }

        private void btnRacking_Click(object sender, RoutedEventArgs e)
        {
            var btnObject = sender as Button;
            var viewModel = (btnObject!.DataContext as MediaViewModel)!;
            if (viewModel.Status == 1)
            {
                viewModel.Status = 0;
                viewModel.RackingBtnContent = "上架";
            }
            else
            {
                viewModel.Status = 1;
                viewModel.RackingBtnContent = "下架";
            }
            SQLite.UpdateTable(viewModel.ToModel());
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.Medias.Remove(viewModel);
            SQLite.DeleteById<Media>(viewModel.Id);
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var newMedia = new MediaViewModel(new Media
            {
                Name = $"新节目{DateTime.Now.ToString("yyyyMMddhhmmss")}",
                Status = 1,
                User = manageViewModel.CurrentUser.ToModel(),
                Size = "0",
                CreatedSource = (App.Current.MainWindow.DataContext as UserViewModel).Role == "admin" ? "管理员" : "用户",
            });

            manageViewModel.ShowDialogCommand.Execute(newMedia);
            //(App.Current.MainWindow as MainWindow).MainContentControl.Content = new MediaEdit(newMedia, manageViewModel.CurrentUser, manageViewModel.ShowNavigation);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            foreach (var item in selectedMedias)
            {
                item.GroupId = manageViewModel.SelectedGroupId;
                item.Group = manageViewModel.MediaGroups.FirstOrDefault(c => c.Id == manageViewModel.SelectedGroupId)?.Name ?? "未分组";

                item.IsSelected = false;
                SQLite.UpdateTable(item.ToModel());
            }

            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void btnCopy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() != 1)
            {
                MessageBox.Show("请选择一条节目单进行复制！");
                return;
            }

            var selectedModel = selectedMedias.First().ToModel();
            selectedModel.Group = manageViewModel.MediaGroups.FirstOrDefault(c => c.Id == selectedModel.GroupId)?.ToModel();
            selectedModel.User = manageViewModel.CurrentUser.ToModel();

            var newMedia = new MediaViewModel(selectedModel);
            newMedia.Name += "_Copy";
            newMedia.Id = 0;
            newMedia.CreatedSource = (App.Current.MainWindow.DataContext as UserViewModel).Role == "admin" ? "管理员" : "用户";
            manageViewModel.ShowDialogCommand.Execute(newMedia);
        }

        private void btnDeleteAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected).ToList();
            if (selectedMedias.Any())
            {
                foreach (var item in selectedMedias)
                {
                    manageViewModel.Medias.Remove(item);
                    //SQLite.DeleteById<Media>(item.Id);
                }
            }
        }

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaDevicesViewModel)!;
            viewModel.PublishCommand.Execute(null);
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void btnPublish_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() != 1)
            {
                MessageBox.Show("请选择一条节目单进行发布！");
                return;
            }

            var selectedMedia = selectedMedias.First();

            var sourceDic = System.IO.Path.Combine(Helpers.Constants.OutPath, selectedMedia.Name);
            var desZipFilePath = System.IO.Path.Combine(Helpers.Constants.OutPath, selectedMedia.Name + ".zip");

            fileService.CreatZip(sourceDic, desZipFilePath);

            var devices = deviceService.GetDevices().GetAwaiter().GetResult().ToList();
            foreach (var device in devices) 
            {
                if(device.MediaIds.Contains(selectedMedia.Id))
                {
                    device.IsSelected = true;
                }
            }

            var viewModel = new MediaDevicesViewModel(selectedMedia, devices);
            selectedMedia.IsSelected = false;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnMediaSave_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;

            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;  
            viewModel.Resolution = $"{viewModel.Width}*{viewModel.Height}";
            viewModel.Group = viewModel.Group ?? "未分组";
            viewModel.LastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            viewModel.SubmitCommand.Execute(null);
            if (!viewModel.HasErrors)
            {
                if (viewModel.Id != 0)
                {
                    var dbModel = SQLite.QueryTable<Media>().First(c => c.Id == viewModel.Id);
                    if (dbModel.Name != viewModel.Name)
                    {
                        var oldFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, dbModel.Name);
                        var newFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, viewModel.Name);
                        if (Directory.Exists(oldFolderPath))
                        {
                            var config = fileService.ReadFileContent<MediaConfig>(Path.Combine(Helpers.Constants.OutPath, dbModel.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
                            if (config != null)
                            {
                                config.Name = viewModel.Name;
                                config.Pages.ForEach(page => page.Components.ForEach(c =>
                                {
                                    switch (c.Type)
                                    {
                                        case Models.MediaType.Image:
                                        case Models.MediaType.Video:
                                            c.Source = c.Source.Replace(dbModel.Name, viewModel.Name);
                                            break;
                                        default:
                                            break;
                                    }
                                }));

                                var configContent = JsonConvert.SerializeObject(config);

                                var mediaResourcePath = Path.Combine(Helpers.Constants.OutPath, dbModel.Name);

                                fileService.SaveFileContent(mediaResourcePath, Helpers.Constants.ConfigFileName, configContent);
                            }

                            // 重命名文件夹
                            Directory.Move(oldFolderPath, newFolderPath);
                        }
                    }

                    SQLite.UpdateTable(viewModel.ToModel());
                }
                else
                {
                    viewModel.Id = SQLite.InserTable(viewModel.ToModel());
                    manageViewModel.Medias.Add(viewModel);
                }

                manageViewModel.CloseDialogCommand.Execute(null);

                (App.Current.MainWindow as MainWindow).GoCotent(new MediaEdit(viewModel, manageViewModel.CurrentUser, manageViewModel.ShowNavigation), 2);
            }
        }

        private void btnMediaCancel_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaManageViewModel)!;

            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.CloseDialogCommand.Execute(null);

            if(viewModel.Id != 0)
            {
                (App.Current.MainWindow as MainWindow).GoCotent(new MediaEdit(viewModel, manageViewModel.CurrentUser, manageViewModel.ShowNavigation), 2);
            }
        }
    }
}
