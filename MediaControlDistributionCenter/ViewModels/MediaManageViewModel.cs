using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using MediaControlDistributionCenter.Data.Entity;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using MediaControlDistributionCenter.Views.UserManagement;
using System.Windows;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Data;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using MediaControlDistributionCenter.Converters;
using Newtonsoft.Json;
using System.IO;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaManageViewModel : ObservableObject
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

        public MediaManageViewModel(IProgramService programService, IProgramGroupService programGroupService, IFileService fileService) 
        {
            this.programService = programService;
            this.programGroupService = programGroupService;
            this.fileService = fileService;
        }

        public void SetValues(UserViewModel userViewModel, long? groupId = null)
        {
            CurrentUser = userViewModel;

            var groups = programGroupService.GetAll(new ProgramGroupDto { UserAccount = userViewModel.Account}).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramGroupDto>();
            groups.Insert(0, new ProgramGroupDto
            {
                Id = -1,
                Name = "全部",
                UserAccount = userViewModel.Account,
            });
            this.MediaGroups = new ObservableCollection<MediaGroupViewModel>(groups.Select(c=>
            {
                var result = new MediaGroupViewModel();
                result.Binding(c, c.Id == -1 ? true : false);
                return result;
            }));

            var medias = programService.GetAll(new ProgramDto { UserAccount = userViewModel.Account, GroupId = groupId }).GetAwaiter().GetResult().Data?.ToList() ?? new List<ProgramDto>();
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
            var response = await programGroupService.Save(groupViewModel.ToModel());
            if (response.Code == 200)
            {
                var groupModel = await programGroupService.GetAll(new ProgramGroupDto { Name = groupViewModel.Name, UserAccount = groupViewModel.UserId });
                groupViewModel.Id = groupModel.Data!.First().Id;

                MediaGroups.Add(groupViewModel);
            }
        }

        [RelayCommand]
        private async Task ChangeGroup(MediaGroupViewModel groupViewModel)
        {
            var selectedMedias = Medias.Where(c => c.IsSelected);

            foreach (var item in selectedMedias)
            {
                item.GroupId = SelectedGroupId;
                item.Group = MediaGroups.FirstOrDefault(c => c.Id == SelectedGroupId)?.Name ?? "未分组";

                item.IsSelected = false;
                var response = await programService.Save(item.ToModel());
                if (response.Code == 200)
                {
                }
            }
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
            }
        }

        [RelayCommand]
        private async Task DeleteMedia(MediaViewModel viewModel)
        {
            Medias.Remove(viewModel);
            var response = await programService.DeleteById(viewModel.Id);
            if (response.Code == 200)
            {
            }
        }

        [RelayCommand]
        private async Task DeleteMedias()
        {
            var selectedIds = Medias.Where(c => c.IsSelected).Select(c => c.Id).ToList();
            var response = await programService.DeleteBatch(selectedIds);
            if (response.Code == 200)
            {
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
                    else
                    {
                        Medias.Add(viewModel);
                    }

                    CloseDialog();
                }                
            }
        }
    }
}
