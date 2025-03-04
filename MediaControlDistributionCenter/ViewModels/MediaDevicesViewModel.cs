using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaDevicesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private MediaViewModel currentMedia;

        public MediaDevicesViewModel(MediaViewModel currentMedia, IEnumerable<DeviceViewModel> devices) 
        {
            this.currentMedia = currentMedia;
            this.devices = new ObservableCollection<DeviceViewModel>(devices);
        }

        [RelayCommand]
        private void Publish()
        {
            foreach (var item in Devices)
            {
                var model = new DeviceMedia { DeviceId = item.Id, MediaId = CurrentMedia.Id };
                if (item.IsSelected)
                {
                    if (SQLite.QueryTable<DeviceMedia>().Where(c => c.DeviceId == item.Id && c.MediaId == CurrentMedia.Id).First() == null) 
                    {
                        SQLite.InserTable(model);

                        string filePath = $"{CurrentMedia.Name}.zip";
                        item.UploadFileCommand.Execute(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, filePath));
                        item.SyncFileSyncCommand.Execute(filePath);
                    }
                }
                else
                {
                    SQLite.DeleTeable(model);
                }                
            }
        }
    }
}
