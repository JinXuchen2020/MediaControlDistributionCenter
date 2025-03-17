using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaManageViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";
        public UserViewModel CurrentUser { get; set; }

        public bool ShowNavigation { get; set; }

        [ObservableProperty]
        private ObservableCollection<MediaGroupViewModel> mediaGroups;

        [ObservableProperty]
        private ObservableCollection<MediaViewModel> medias;

        [ObservableProperty]
        private int selectedGroupId;

        [ObservableProperty]
        private MediaViewModel selectedMedia;

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        private readonly IProgramGroupService programGroupService;
        private readonly IProgramService programService;
        private readonly IFileService fileService;

        public MediaManageViewModel(DashboardViewModel dashboardViewModel, UserManageViewModel userManageViewModel, IFileService fileService) 
        {
            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                ShowNavigation = true;
                CurrentUser = dashboardViewModel.CurrentUser;
            }
            else
            {
                CurrentUser = dashboardViewModel.SelectedUser ?? userManageViewModel.SelectedUser!;
            }

            this.programService = GetService<IProgramService>();
            this.programGroupService = GetService<IProgramGroupService>();
            this.fileService = fileService;
        }

        public override void LoadData(long? groupId = null)
        {
            var groups = programGroupService.GetAll(new ProgramGroupDto { UserAccount = CurrentUser.Account}).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramGroupDto>();
            groups.Insert(0, new ProgramGroupDto
            {
                Id = -1,
                Name = "全部",
                UserAccount = CurrentUser.Account,
            });
            this.MediaGroups = new ObservableCollection<MediaGroupViewModel>(groups.Select(c=>
            {
                var result = new MediaGroupViewModel();
                result.Binding(c, c.Id == (groupId ?? -1) ? true : false);
                return result;
            }));

            var medias = programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
            this.Medias = new ObservableCollection<MediaViewModel>(medias.Select(c =>
            {
                var result = new MediaViewModel();
                result.Binding(c);
                return result;
            }));
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, DialogHostId);
        }

        [RelayCommand]
        private void CloseDialog()
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(DialogHostId);
        }

        [RelayCommand]
        private async Task CreateGroup(MediaGroupViewModel groupViewModel)
        {
            groupViewModel.SubmitCommand.Execute(null);
            if (groupViewModel.HasErrors)
            {
                return;
            }

            var response = await programGroupService.Save(groupViewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
                CloseDialog();
            }
        }

        [RelayCommand]
        private async Task ChangeGroup(MediaGroupViewModel groupViewModel)
        {
            var selectedMedias = Medias.Where(c => c.IsSelected);

            foreach (var item in selectedMedias)
            {
                item.GroupId = SelectedGroupId;

                item.IsSelected = false;
                var response = await programService.Save(item.ToModel());
                if (response.Code == 200)
                {
                }
            }

            LoadData();
            CloseDialog();
        }

        [RelayCommand]
        private async Task ChangeMediaStatus(MediaViewModel viewModel)
        {
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

            var response = await programService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteMedia(MediaViewModel viewModel)
        {
            var response = await programService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteMedias()
        {
            var selectedIds = Medias.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await programService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task SaveMedia(MediaViewModel viewModel)
        {
            viewModel.SubmitCommand.Execute(null);
            if (!viewModel.HasErrors)
            {
                ProgramDto? dbModel = null;
                if (viewModel.Id != 0)
                {
                    dbModel = (await programService.GetById(viewModel.Id)).Data!;
                }                    

                var response = await programService.Save(viewModel.ToModel());
                if (response.Code == 200)
                {
                    if (dbModel != null)
                    {
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
                    }

                    LoadData();
                    viewModel.Id = Medias.First(c => c.Name == viewModel.Name).Id;
                    CloseDialog();
                }                
            }
        }
    }
}
