using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog.Formatting.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaManageViewModel : PageViewModel
    {
        private const string DialogHostId = "RootDialogHostId";

        public UserViewModel CurrentUser { get; set; }

        public ProgramGroupViewModel SelectedGroup { get; set; }

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        [ObservableProperty]
        private ObservableCollection<ProgramGroupViewModel> mediaGroups;

        [ObservableProperty]
        private ObservableCollection<ProgramViewModel> medias;

        [ObservableProperty]
        private long? selectedGroupId;

        [ObservableProperty]
        private ProgramViewModel selectedMedia;

        private bool isSynced;
        private readonly IProgramGroupService programGroupService;
        private readonly IProgramService programService;
        private readonly IFileService fileService;
        private readonly IPlaybackRecordService playbackRecordService;

        public MediaManageViewModel(IFileService fileService) 
        {
            this.programService = GetService<IProgramService>();
            this.programGroupService = GetService<IProgramGroupService>();
            this.playbackRecordService = GetService<IPlaybackRecordService>();
            this.fileService = fileService;
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
        }

        public override async Task LoadData()
        {
            if (CurrentUser != null)
            {
                var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
                var groups = (await programGroupService.GetAll(new ProgramGroupDto { UserAccount = CurrentUser.Account })).Data?.ToList() ?? new List<ProgramGroupDto>();
                groups.Insert(0, new ProgramGroupDto
                {
                    Id = -1,
                    Name = FindResource("LanguageKey_Code_All"),
                    UserAccount = CurrentUser.Account,
                });
                this.MediaGroups = new ObservableCollection<ProgramGroupViewModel>(groups.Select(c =>
                {
                    var result = new ProgramGroupViewModel();
                    result.Binding(c, c.Id == (groupId ?? -1) ? true : false);
                    return result;
                }));

                var medias = (await programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account, GroupId = groupId })).Data?.ToList() ?? new List<ProgramDto>();
                this.Medias = new ObservableCollection<ProgramViewModel>(medias.OrderByDescending(c => c.Id).Select(c =>
                {
                    var result = new ProgramViewModel();
                    result.Binding(c);
                    return result;
                }));
            }
        }

        public async Task SyncPrograms()
        {
            var localDevice = OnlineDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet)?.DeviceViewModel;
            if (ConnectionMode.Mode == "Local" && localDevice != null && !isSynced)
            {
                await localDevice.SyncProgramsCommand.ExecuteAsync(null);
                isSynced = true;
            }
        }

        public ProgramViewModel CreateProgram()
        {
            var resultModel = new ProgramDto
            {
                Name = $"{FindResource("LanguageKey_Code_Program_Tooltip_100")}{DateTime.Now.ToString("yyyyMMddhhmmss")}",
                MediaType = "PROGRAM",
                UserAccount = CurrentUser.Account,
                Status = 0,
                Size = 0,
                Resolution = "256*192",
            };

            var result = new ProgramViewModel();
            result.Binding(resultModel);
            return result;
        }

        [RelayCommand]
        private async Task ShowDialog(ObservableObject content)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(content, DialogHostId);
        }

        [RelayCommand]
        private async Task ShowDialogContent(UserControl dialogContent)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialogContent, Constants.DialogHostId);
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
                await LoadData();
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

            await LoadData();
            CloseDialog();
        }

        [RelayCommand]
        private async Task ChangeMediaStatus(ProgramViewModel viewModel)
        {
            if (viewModel.Status == 1)
            {
                viewModel.Status = 2;
            }
            else
            {
                viewModel.Status = 1;
            }

            var playbackRecordService = GetService<IPlaybackRecordService>();
            var deviceService = GetService<IMonitorService>();
            var playRecords = (await playbackRecordService.GetAll(new PlaybackRecordDto { MediaName = viewModel.Name })).Data?.ToList() ?? new List<PlaybackRecordDto>();
            foreach (var playbackRecord in playRecords)
            {
                foreach(var device in OnlineDevices.Where(c => c.SnCode == playbackRecord.MonitorSnCode && c.DeviceViewModel != null).Select(c => c.DeviceViewModel!))
                {
                    await device.ChangeProgramCommand.ExecuteAsync(viewModel);
                    if (!string.IsNullOrEmpty(device.ErrorMessage))
                    {
                        ErrorMessage = device.ErrorMessage;
                        await ShowConfirmDialogCommand.ExecuteAsync(null);
                        device.ErrorMessage = null;
                        return;
                    }
                }
            }

            if (viewModel.Status == 1)
            {
                var shelfMedias = (await programService.GetAll(new ProgramDto { Status = 1, MediaType = viewModel.Type })).Data?.ToList() ?? new List<ProgramDto>();
                foreach (var media in shelfMedias)
                {
                    media.Status = 2;
                    await programService.Save(media);
                }
            }

            var response = await programService.Save(viewModel.ToModel());
            if (response.Code == 200)
            {
                await LoadData();
            }
        }

        [RelayCommand]
        private async Task DeleteMedia()
        {
            var selectedItems = Medias.Where(c => c.IsSelected).ToList();
            var playbackRecordService = GetService<IPlaybackRecordService>();
            var publishedPrograms = new List<ProgramDto>();

            foreach (var connectedDevice in OnlineDevices.Where(c=>c.DeviceViewModel!= null).Select(c => c.DeviceViewModel!))
            {
                foreach (var selectedItem in selectedItems)
                {
                    var playRecords = (await playbackRecordService.GetAll(new PlaybackRecordDto { MediaName = selectedItem.Name, MonitorSnCode = connectedDevice.SNumber })).Data?.ToList() ?? new List<PlaybackRecordDto>();
                    if (playRecords.Count > 0)
                    {
                        publishedPrograms.Add(selectedItem.ToModel());
                    }
                }

                if (publishedPrograms.Count > 0)
                {
                    var modelString = JsonConvert.SerializeObject(publishedPrograms);
                    await connectedDevice.DeleteProgramCommand.ExecuteAsync(modelString);
                    if (!string.IsNullOrEmpty(connectedDevice.ErrorMessage))
                    {
                        ErrorMessage = connectedDevice.ErrorMessage;
                        await ShowConfirmDialogCommand.ExecuteAsync(null);
                        connectedDevice.ErrorMessage = null;
                        return;
                    }
                }
            }

            foreach (var selectedItem in selectedItems)
            {
                var playRecords = (await playbackRecordService.GetAll(new PlaybackRecordDto { MediaName = selectedItem.Name })).Data?.ToList() ?? new List<PlaybackRecordDto>();
                await playbackRecordService.DeleteBatch(playRecords.Select(c => c.Id).ToList());

                fileService.DeleteResourcePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, selectedItem.UserId, selectedItem.Name));
            }

            var response = await programService.DeleteBatch(selectedItems.Select(c => c.Id).ToList());
            if (response.Code == 200) 
            {
                await LoadData();
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
                if (response.Code == 200 && response.Data)
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
                                    config.Program = viewModel.ToModel();
                                    config.Pages.ForEach(page => 
                                    {
                                        page.ThumbnailFilePath = page.ThumbnailFilePath.Replace(dbModel.Name, viewModel.Name);
                                        page.Components.ForEach(c =>
                                        {
                                            switch (c.Type)
                                            {
                                                case Models.MediaType.Image:
                                                case Models.MediaType.Video:
                                                case MediaType.Word:
                                                    c.Source = c.Source.Replace(dbModel.Name, viewModel.Name);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        });

                                    });
                                    var configContent = JsonConvert.SerializeObject(config);

                                    var mediaResourcePath = Path.Combine(Helpers.Constants.OutPath, CurrentUser.Account, dbModel.Name);
                                    fileService.SaveFileContent(mediaResourcePath, Helpers.Constants.ConfigFileName, configContent);
                                }

                                // 重命名文件夹
                                Directory.Move(oldFolderPath, newFolderPath);
                            }
                        }
                    }

                    await LoadData();
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
                var agentUsers = (await programService.GetAll(new ProgramDto { GroupId = viewModel.Id })).Data?.ToList() ?? new List<ProgramDto>();
                foreach (var item in agentUsers)
                {
                    item.GroupId = null;
                    await programService.Save(item);
                }
            }

            await LoadData();
        }

        protected override async Task SearchContent()
        {
            if (string.IsNullOrEmpty(SearchString)) SearchString = null;
            var groupId = SelectedGroup?.Id == -1 ? null : SelectedGroup?.Id;
            var medias = (await programService.GetAll(new ProgramDto { UserAccount = CurrentUser.Account, Name = SearchString, GroupId = groupId }, true)).Data?.ToList() ?? new List<ProgramDto>();
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
