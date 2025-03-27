using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
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

        public ProgramGroupViewModel SelectedGroup { get; set; }

        public bool ShowNavigation { get; set; }

        [ObservableProperty]
        private ObservableCollection<ProgramGroupViewModel> mediaGroups;

        [ObservableProperty]
        private ObservableCollection<ProgramViewModel> medias;

        [ObservableProperty]
        private long? selectedGroupId;

        [ObservableProperty]
        private ProgramViewModel selectedMedia;

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        private readonly IProgramGroupService programGroupService;
        private readonly IProgramService programService;
        private readonly IFileService fileService;

        public MediaManageViewModel(IFileService fileService) 
        {
            this.programService = GetService<IProgramService>();
            this.programGroupService = GetService<IProgramGroupService>();
            this.fileService = fileService;
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override void LoadData()
        {
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var groups = programGroupService.GetAll(new ProgramGroupDto { UserAccount = CurrentUser.Account}).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramGroupDto>();
            groups.Insert(0, new ProgramGroupDto
            {
                Id = -1,
                Name = FindResource("LanguageKey_Code_All"),
                UserAccount = CurrentUser.Account,
            });
            this.MediaGroups = new ObservableCollection<ProgramGroupViewModel>(groups.Select(c=>
            {
                var result = new ProgramGroupViewModel();
                result.Binding(c, c.Id == (groupId ?? -1) ? true : false);
                return result;
            }));
            var medias = programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
            this.Medias = new ObservableCollection<ProgramViewModel>(medias.OrderByDescending(c => c.Id).Select(c =>
            {
                var result = new ProgramViewModel();
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
        private async Task CreateGroup(ProgramGroupViewModel groupViewModel)
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
        private async Task ChangeGroup(ProgramGroupViewModel groupViewModel)
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
        private async Task ChangeMediaStatus(ProgramViewModel viewModel)
        {
            if (viewModel.Status == 1)
            {
                viewModel.Status = 0;
            }
            else
            {
                viewModel.Status = 1;
            }

            var response = await programService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteMedia(ProgramViewModel viewModel)
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
        private async Task SaveMedia(ProgramViewModel viewModel)
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
                            var oldFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, CurrentUser.Account, dbModel.Name);
                            var newFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, CurrentUser.Account, viewModel.Name);
                            if (Directory.Exists(oldFolderPath))
                            {
                                var config = fileService.ReadFileContent<MediaConfig>(Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, dbModel.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
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

                                    var mediaResourcePath = Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, dbModel.Name);
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

        [RelayCommand]
        private async Task DeleteGroup(ProgramGroupViewModel viewModel)
        {
            var response = await programGroupService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
                var agentUsers = programService.GetAll(new ProgramDto { GroupId = viewModel.Id }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
                foreach (var item in agentUsers)
                {
                    item.GroupId = null;
                    await programService.Save(item);
                }
            }

            LoadData();
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var medias = programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account, Name = SearchString, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
            this.Medias = new ObservableCollection<ProgramViewModel>(medias.Select(c =>
            {
                var viewModel = new ProgramViewModel();
                viewModel.Binding(c);
                return viewModel;
            }));

            await Task.CompletedTask;
        }
    }
}
