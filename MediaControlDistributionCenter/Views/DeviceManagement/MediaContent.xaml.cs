

using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.LocalImps;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.Diagrams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// 界面 的交互逻辑
    /// </summary>
    public partial class MediaContent : FrameControl
    {
        private readonly MediaContentViewModel manageViewModel;
        private IFileService fileService;
        public MediaContent(MediaContentViewModel mediaContentViewModel, IFileService fileService)
        {
            InitializeComponent();
            manageViewModel = mediaContentViewModel;
            DataContext = mediaContentViewModel;
            this.fileService = fileService;

            this.Loaded += MediaContent_Loaded;
        }

        private void MediaContent_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                await manageViewModel.LoadData();
            });
        }

        private void btnGroupAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var groupViewModel = new MediaGroupViewModel();
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void btnGroupSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var groupViewModel = ((sender as Button).DataContext as MediaGroupViewModel)!;
            manageViewModel.CreateGroupCommand.Execute(groupViewModel);
        }

        private void btnConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (manageViewModel.SelectedGroupId == null || manageViewModel.SelectedGroupId == -1)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_MediaStore_Tooltip_106");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }
            manageViewModel.ChangeGroupCommand.Execute(null);
        }

        private void btnMediaSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.SaveMediaCommand.Execute(viewModel);
        }

        private void btnMediaCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manageViewModel.CloseDialogCommand.Execute(null);
        }

        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var groupViewModel = ((sender as DockPanel).DataContext as MediaGroupViewModel)!;
            manageViewModel.SelectedGroup = groupViewModel;
            Dispatcher.Invoke(async () =>
            {
                await manageViewModel.LoadData();
            });
        }

        private void btnUploadStart_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = manageViewModel.CreateMedia();
            viewModel.Groups = new ObservableCollection<MediaGroupViewModel>(manageViewModel.MediaGroups.Where(c => c.Id != -1));
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnChangeGroup_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_MediaStore_Tooltip_107");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnDeleteAll_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected).ToList();
            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_MediaStore_Tooltip_107");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    await manageViewModel.DeleteMediasCommand.ExecuteAsync(null);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void SelectAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox.IsChecked.GetValueOrDefault())
            {
                foreach (var item in manageViewModel.Medias)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (var item in manageViewModel.Medias)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void btnEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            viewModel.Groups = new ObservableCollection<MediaGroupViewModel>(manageViewModel.MediaGroups.Where(c => c.Id != -1));
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
                    await manageViewModel.DeleteMediaCommand.ExecuteAsync(viewModel);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnPreview_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            var previewWindow = new MediaContentPreview(viewModel);
            previewWindow.Show();
        }

        private void btnUpload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            if (string.IsNullOrEmpty(viewModel.Name))
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_MediaStore_Tooltip_109");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (viewModel.Type == "Image")
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型
            }
            else if (viewModel.Type == "Video")
            {
                openFileDialog.Filter = "Video Files|*.mp4;*.mp3;*.avi;*.wmv;*.mkv"; // 过滤器，允许的文件类型
            }

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;
                viewModel.Extension = Path.GetExtension(filePath);
                var fileSize = (double)new FileInfo(filePath).Length / 1024 / 1024;
                viewModel.Size = Math.Round(fileSize, 2);
                var fileName = Path.GetFileName(filePath);

                this.Dispatcher.Invoke(async () =>
                {
                    var uploadService = Utility.GetService<IUploadService>();
                    if (uploadService is UploadServiceLocal local)
                    {
                        var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                        local.FtpClient = ftpClient;
                    }

                    fileName = string.IsNullOrEmpty(viewModel.Name) ? fileName : $"{viewModel.Name}{viewModel.Extension}";

                    await uploadService.UploadFile(filePath, fileName);

                    viewModel.Src = fileName;
                });
            }
        }

        private void RefreshData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = ((sender as Border).Tag as string)!;
            manageViewModel.SelectedType = tag;
            manageViewModel.SearchString = null;
            Dispatcher.Invoke(async () =>
            {
                await manageViewModel.LoadData();
            });
        }

        private void btnGroupDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaGroupViewModel)!;
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
    }
}
