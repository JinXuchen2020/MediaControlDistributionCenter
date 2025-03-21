using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaControlDistributionCenter.Views.MediaManagement
{
    /// <summary>
    /// MediaManage.xaml 的交互逻辑
    /// </summary>
    public partial class MediaManage : UserControl
    {
        private readonly IFileService fileService;
        private readonly IServiceProvider serviceProvider;
        private readonly MediaManageViewModel manageViewModel;
        private readonly UserManageViewModel userManageViewModel;

        public MediaManage(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, MediaManageViewModel mediaManageViewModel, IFileService fileService, IServiceProvider serviceProvider)
        {
            this.fileService = fileService; 
            this.serviceProvider = serviceProvider;
            this.userManageViewModel = userManageViewModel;

            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                mediaManageViewModel.ShowNavigation = true;
                mediaManageViewModel.CurrentUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                mediaManageViewModel.CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
            }

            manageViewModel = mediaManageViewModel;

            manageViewModel.LoadData();
            DataContext = manageViewModel;

            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = ((sender as Button).DataContext as MediaGroupViewModel)!;

            manageViewModel.CreateGroupCommand.Execute(groupViewModel);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var groupViewModel = new MediaGroupViewModel();
            groupViewModel.UserId = manageViewModel.CurrentUser.Account;
            manageViewModel.ShowDialogCommand.Execute(groupViewModel);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            manageViewModel.MediaGroups.First(c => c.IsSelected).IsSelected = false;
            var groupViewModel = ((sender as StackPanel).DataContext as MediaGroupViewModel)!;
            groupViewModel.IsSelected = true;
            manageViewModel.SelectedGroup = groupViewModel;
            manageViewModel.LoadData();
        }

        private void btnRacking_Click(object sender, RoutedEventArgs e)
        {
            var btnObject = sender as Button;
            var viewModel = (btnObject!.DataContext as MediaViewModel)!;

            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    manageViewModel.ChangeMediaStatusCommand.Execute(viewModel);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                manageViewModel.CanDelete = false;
                await manageViewModel.ShowConfirmDialogCommand.ExecuteAsync(null);

                if (manageViewModel.CanDelete.HasValue && manageViewModel.CanDelete.Value)
                {
                    var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
                    manageViewModel.DeleteMediaCommand.ExecuteAsync(viewModel);
                }

                manageViewModel.CanDelete = null;
            });
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var newMediaModel = new ProgramDto
            {
                Name =  $"{FindResource("LanguageKey_Code_Program_Tooltip_100")}{DateTime.Now.ToString("yyyyMMddhhmmss")}",
                MediaType = "PROGRAM",
                UserAccount = manageViewModel.CurrentUser.Account,
                Status = 1,
                CreatedSource = userManageViewModel.CurrentUser.Role == "admin" ? (string)FindResource("LanguageKey_Code_Role_Admin") : (string)FindResource("LanguageKey_Code_Role_User"),
            };

            var newViewModel = new MediaViewModel();
            newViewModel.Binding(newMediaModel);

            manageViewModel.ShowDialogCommand.Execute(newViewModel);
        }

        private void btnChangeGroup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Program_Tooltip_108");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.SelectedGroupId == null || manageViewModel.SelectedGroupId == -1)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Monitor_Tooltip_115");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }
            manageViewModel.ChangeGroupCommand.Execute(null);
        }

        private void btnCopy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Program_Tooltip_108");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (selectedMedias.Count() != 1)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Program_Tooltip_109");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var selectedModel = selectedMedias.First().ToModel();
            selectedModel.ProgramGroupName = selectedMedias.First().Group;

            var newMedia = new MediaViewModel();
            newMedia.Binding(selectedModel);
            newMedia.Name += $"_{FindResource("LanguageKey_Code_Media_Copy")}";
            newMedia.Id = 0;
            newMedia.CreatedSource = userManageViewModel.CurrentUser.Role == "admin" ? (string)FindResource("LanguageKey_Code_Role_Admin") : (string)FindResource("LanguageKey_Code_Role_User");
            manageViewModel.ShowDialogCommand.Execute(newMedia);
        }

        private void btnDeleteAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected).ToList();
            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Program_Tooltip_108");
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

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaDevicesViewModel)!;
            this.Dispatcher.Invoke(async () =>
            {
                await viewModel.PublishCommand.ExecuteAsync(null);

                if (viewModel.PublishDevices.Count > 0)
                {
                    manageViewModel.CloseDialogCommand.Execute(null);
                    viewModel.ShowConfirmDialogCommand.Execute(null);
                }
            });
        }

        private void btnPublish_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Program_Tooltip_108");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (selectedMedias.Count() != 1)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_Program_Tooltip_109");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var selectedMedia = selectedMedias.First();

            var sourceDic = System.IO.Path.Combine(Helpers.Constants.OutPath, selectedMedia.Name);
            var desZipFilePath = System.IO.Path.Combine(Helpers.Constants.OutPath, selectedMedia.Name + ".zip");

            fileService.CreatZip(sourceDic, desZipFilePath);

            var viewModel = serviceProvider.GetRequiredService<MediaDevicesViewModel>();
            viewModel.CurrentMedia = selectedMedia;
            viewModel.LoadData();
            selectedMedia.IsSelected = false;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnMediaSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;  
            viewModel.Resolution = $"{viewModel.Width}*{viewModel.Height}";
            viewModel.LastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            manageViewModel.SaveMediaCommand.Execute(viewModel);
            if (!viewModel.HasErrors)
            {
                manageViewModel.SelectedMedia = viewModel;
                var content = serviceProvider.GetRequiredService<MediaEdit>();
                (App.Current.MainWindow as MainWindow)!.GoContent(content, 2);
            }
        }

        private void btnMediaCancel_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.CloseDialogCommand.Execute(null);

            if (viewModel.Id != 0)
            {
                manageViewModel.SelectedMedia = viewModel;
                var content = serviceProvider.GetRequiredService<MediaEdit>();
                (App.Current.MainWindow as MainWindow)!.GoContent(content, 2);
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
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
    }
}
