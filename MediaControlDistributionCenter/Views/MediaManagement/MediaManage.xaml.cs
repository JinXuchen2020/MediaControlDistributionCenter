using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
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

        public MediaManage(UserManageViewModel userManageViewModel, MediaManageViewModel mediaManageViewModel, IFileService fileService, IServiceProvider serviceProvider)
        {
            this.fileService = fileService; 
            this.serviceProvider = serviceProvider;
            this.userManageViewModel = userManageViewModel;

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

            manageViewModel.LoadData(groupViewModel.Id == -1 ? null : groupViewModel.Id);
        }

        private void btnRacking_Click(object sender, RoutedEventArgs e)
        {
            var btnObject = sender as Button;
            var viewModel = (btnObject!.DataContext as MediaViewModel)!;

            manageViewModel.ChangeMediaStatusCommand.Execute(viewModel);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaViewModel)!;
            manageViewModel.DeleteMediaCommand.Execute(viewModel);
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var newMediaModel = new ProgramDto
            {
                Name = $"新节目{DateTime.Now.ToString("yyyyMMddhhmmss")}",
                MediaType = "PROGRAM",
                UserAccount = manageViewModel.CurrentUser.Account,
                Status = 1,
                CreatedSource = userManageViewModel.CurrentUser.Role == "admin" ? "管理员" : "用户",
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
                MessageBox.Show("请选择一条节目单！");
                return;
            }

            manageViewModel.ShowDialogCommand.Execute(manageViewModel);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            manageViewModel.ChangeGroupCommand.Execute(null);
        }

        private void btnCopy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected);

            if (selectedMedias.Count() != 1)
            {
                MessageBox.Show("请选择一条节目单进行复制！");
                return;
            }

            var selectedModel = selectedMedias.First().ToModel();
            selectedModel.ProgramGroupName = selectedMedias.First().Group;

            var newMedia = new MediaViewModel();
            newMedia.Binding(selectedModel);
            newMedia.Name += "_Copy";
            newMedia.Id = 0;
            newMedia.CreatedSource = userManageViewModel.CurrentUser.Role == "admin" ? "管理员" : "用户";
            manageViewModel.ShowDialogCommand.Execute(newMedia);
        }

        private void btnDeleteAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedMedias = manageViewModel.Medias.Where(c => c.IsSelected).ToList();
            if (selectedMedias.Count() == 0)
            {
                MessageBox.Show("请选择一条节目单！");
                return;
            }

            manageViewModel.DeleteMediasCommand.Execute(null);
        }

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaDevicesViewModel)!;
            viewModel.PublishCommand.Execute(null);
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void btnPublish_MouseDown(object sender, MouseButtonEventArgs e)
        {
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


            manageViewModel.SelectedMedia = selectedMedia;
            var viewModel = serviceProvider.GetRequiredService<MediaDevicesViewModel>();
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
    }
}
