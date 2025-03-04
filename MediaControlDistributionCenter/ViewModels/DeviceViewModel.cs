using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string sNumber;

        [ObservableProperty]
        private string resolution;

        [ObservableProperty]
        private string lastUpdatedTime;

        [ObservableProperty]
        private int status;

        [ObservableProperty]
        private string statusText;

        [ObservableProperty]
        private int? groupId;

        [ObservableProperty]
        private string group;

        [ObservableProperty]
        private int userId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private string mediaNames;

        [ObservableProperty]
        private List<int> mediaIds;

        [ObservableProperty]
        private string ownerName;

        [ObservableProperty]
        private string width;

        [ObservableProperty]
        private string height;

        [ObservableProperty]
        private string startDate;

        [ObservableProperty]
        private string endDate;

        [ObservableProperty]
        private string contactName;

        [ObservableProperty]
        private string contactNumber;

        [ObservableProperty]
        private string enableBtnContent;

        private Communication client;

        public DeviceViewModel()
        {
            client = new Communication();
        }

        public DeviceViewModel(Device model)
        {
            client = new Communication();
            Id = model.Id;
            Name = model.Name;
            sNumber = model.SNumber;
            Resolution = model.Resolution;
            lastUpdatedTime = model.LastUpdatedTime.ToString("yyyy-MM-dd HH:mm");
            status = model.Status;
            groupId = model.Group?.Id;
            userId = model.User.Id;
            ownerName = model.User.Name;
            group = model.Group?.Name ?? "未分组";
            isSelected = false;
            statusText = GetStatus();
            enableBtnContent = model.Status == -1 ? "启用" : "停用";
            width = model.Resolution.Split("*")[0];
            height = model.Resolution.Split("*")[1];
            startDate = model.StartDate.ToShortDateString();
            endDate = model.EndDate.ToShortDateString();
            contactName = model.ContactName;
            contactNumber = model.ContactNumber;
            mediaNames = model.Medias == null ? string.Empty : string.Join("\n", model.Medias.Select(c => c.Name));
            mediaIds = model.Medias == null ? new List<int>() : model.Medias.Select(c => c.Id).ToList();
        }

        public DeviceViewModel Clone()
        {
            var model = this.ToModel();
            model.User = SQLite.QueryTable<User>().First(x => x.Id == UserId);
            var result = new DeviceViewModel(model)
            {
                MediaNames = this.MediaNames,
                MediaIds = this.MediaIds,
                OwnerName = this.OwnerName,
                Group = this.Group,
            };
            return result;
        }

        public Device ToModel()
        {
            return new Device
            {
                Id = Id,
                Name = Name,
                SNumber = SNumber,
                Resolution = Resolution,
                LastUpdatedTime = DateTime.Parse(LastUpdatedTime),
                GroupId = GroupId,
                UserId = UserId,
                Status = Status,
                StartDate = DateTime.Parse(StartDate),
                EndDate = DateTime.Parse(EndDate),
                ContactName = ContactName,
                ContactNumber = ContactNumber,
            };
        }

        public string GetStatus()
        {
            client.Connect("192.168.41.1", "5001");
            int count = 1;
            while(client.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
            {
                Thread.Sleep(500);
                count--;
            }

            return this.Status == -1 ? "停用" : client.netClient.State == Helpers.SocketClient.SocketState.Connected ? "在线" : "离线";
        }

        [RelayCommand]
        private async Task ChangeBrightness(string value)
        {
            string path = CommunicationCmd.CmdBrightness + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("命令处理成功!");
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        [RelayCommand]
        private async Task ChangeVolume(string value)
        {
            string path = CommunicationCmd.CmdVolume + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("命令处理成功!");
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        [RelayCommand]
        private async Task Restart(string value)
        {
            string path = CommunicationCmd.CmdReStart + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("命令处理成功!");
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        [RelayCommand]
        private async Task TimeSync(DateTime value)
        {
            string path = CommunicationCmd.CmdTime + value.ToString();
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("命令处理成功!");
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        [RelayCommand]
        private async Task TimeGPSSync(DateTime value)
        {
            string path = CommunicationCmd.CmdTimeGPS + value.ToString();
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("命令处理成功!");
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        [RelayCommand]
        private void UploadFile(string filePath)
        {
            var result = client.UploadFileToFtpServer(filePath);
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("上传文件成功!");
            }
            else
            {
                MessageBox.Show("上传文件失败!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }

        [RelayCommand]
        private async Task SyncFileSync(string fileName)
        {
            var fileSize = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, fileName)).LongLength;
            var syncObj = new FileSync
            {
                HostName = client.ftpServer._Ip,
                ServerPort = int.Parse(client.ftpServer._port),
                UserName = client.ftpServer._userName,
                Password = client.ftpServer._userPwd,
                FileName = fileName,
                FileSize = fileSize
            };
            string fielSyncString = JsonConvert.SerializeObject(syncObj, Formatting.Indented);
            string path = CommunicationCmd.CmdSyncFile + fielSyncString;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                //SendState.Text += "命令处理成功\r\n";
                MessageBox.Show("命令处理成功!");
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
                //SendState.Text += "命令无法被处理\r\n";
            }
        }
    }
}
