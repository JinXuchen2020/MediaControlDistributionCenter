using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.LocalImps;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenCvSharp;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaViewModel : DataViewModel<MediaDto>
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]
        [Required]
        private string name;

        [ObservableProperty]
        [Required]
        private string extension;

        [ObservableProperty]
        private string resolution;

        [ObservableProperty]
        private double? size;

        [ObservableProperty]
        private string? sizeText;

        [ObservableProperty]
        [Required]
        private string src;

        [ObservableProperty]
        private string type;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string? mediaGroupName;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        [Required]
        private double width;

        [ObservableProperty]
        [Required]
        private double height;

        [ObservableProperty]
        private BitmapImage? thumbnail;        

        [ObservableProperty]
        private ObservableCollection<MediaGroupViewModel> groups;

        public MediaViewModel()
        {
        }

        public override MediaDto ToModel()
        {
            return new MediaDto
            {
                Id = Id,
                Name = Name,
                GroupId = GroupId,
                Extension = Extension,
                Resolution = $"{Width}*{Height}",
                Size = Size,
                Src = Src,
                Type = Type,
            };
        }

        public override void Binding(MediaDto model, bool isSelected = false)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.Type;
            Resolution = model.Resolution;
            Width = string.IsNullOrEmpty(model.Resolution) ? 0 : double.Parse(model.Resolution.Split("*")[0]);
            Height = string.IsNullOrEmpty(model.Resolution) ? 0 : double.Parse(model.Resolution.Split("*")[1]);
            Size = model.Size;
            SizeText = Utility.GetSizeText(model.Size);
            GroupId = model.GroupId;
            MediaGroupName = model.MediaGroupName ?? FindResource("LanguageKey_Code_NoGroup");
            Src = model.Src; 
            IsSelected = isSelected;
            Extension = model.Extension;
            //Thumbnail = GetBitmap(model.Src);
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }

        public async Task GetBitmap()
        {
            if (!string.IsNullOrEmpty(Src))
            {
                try
                {
                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, Src);
                    if (!File.Exists(filePath))
                    {
                        var uploadService = Utility.GetService<IUploadService>();
                        if (uploadService is UploadServiceLocal local)
                        {
                            var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                            local.FtpClient = ftpClient;
                        }

                        await uploadService.DownloadFile(Src);
                    }

                    if (File.Exists(filePath))
                    {
                        if (Type == "Image")
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bitmap.UriSource = new Uri(filePath);
                            bitmap.EndInit();

                            Thumbnail = bitmap;
                        }
                        else
                        {
                            var videoPath = filePath;
                            var thumbnailPath = filePath.Replace(Extension, ".png");
                            VideoScreenCapture.CaptureFrame(videoPath, thumbnailPath, 1);
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bitmap.UriSource = new Uri(thumbnailPath);
                            bitmap.EndInit();

                            Thumbnail = bitmap;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }                
            }            
        }
    }
}
