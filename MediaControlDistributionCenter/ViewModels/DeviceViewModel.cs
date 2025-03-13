using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
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
        private long id;

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
        private int enabled;

        [ObservableProperty]
        private string statusText;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string group;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private string mediaNames;

        [ObservableProperty]
        private List<int> mediaIds;

        [ObservableProperty]
        private string ownerName;

        [ObservableProperty]
        private double width;

        [ObservableProperty]
        private double height;

        [ObservableProperty]
        private string startDate;

        [ObservableProperty]
        private string endDate;

        [ObservableProperty]
        private string contactName;

        [ObservableProperty]
        private string contactNumber;

        [ObservableProperty]
        private double? brightness;

        [ObservableProperty]
        private double? volume;

        [ObservableProperty]
        private string deviceId;

        [ObservableProperty]
        private string enableBtnContent;

        [ObservableProperty]
        private string? sendResult;

        [ObservableProperty]
        private string? uploadResult;

        private Communication client;

        public DeviceViewModel()
        {
            client = new Communication();
        }

        public MonitorDto ToModel()
        {
            return new MonitorDto
            {
                Id = Id,
                Name = Name,
                SnCode = SNumber,
                Status = StatusText,
                GroupId = GroupId,
                UserAccount = UserId,
                Enabled = Enabled,
                Width = Width,
                Height = Height,
                ValidStart = StartDate,
                ValidEnd = EndDate,
                ContactName = ContactName,
                ContactPhone = ContactNumber,
                Brightness = Brightness,
                Volume = Volume,
                DeviceId = DeviceId,                 
            };
        }

        public void Binding(MonitorDto model)
        {
            Id = model.Id;
            Name = model.Name;
            SNumber = model.SnCode;
            Resolution = $"{model.Width}*{model.Height}";
            LastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            Status = model.Status == "ONLINE" ? 1 : 0;
            GroupId = model.GroupId;
            UserId = model.UserAccount;
            OwnerName = model.UserName;
            Group = model.MonitorGroupName ?? "未分组";
            IsSelected = false;
            StatusText = model.Status;
            Enabled = model.Enabled;
            EnableBtnContent = model.Enabled ==  0 ? "启用" : "停用";
            Width = model.Width;
            Height = model.Height;
            StartDate = model.ValidStart;
            EndDate = model.ValidEnd;
            ContactName = model.ContactName;
            ContactNumber = model.ContactPhone;
            Brightness = model.Brightness;
            Volume = model.Volume;
            DeviceId = model.DeviceId;
            MediaNames = string.Empty;
            MediaIds = new List<int>();
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

            return this.Enabled == 0 ? "停用" : client.netClient.State == Helpers.SocketClient.SocketState.Connected ? "在线" : "离线";
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
        private async Task UploadFile(string filePath)
        {
            var result = await client.UploadFileToFtpServer(filePath);
            if (result)
            {
                UploadResult = "上传成功";
            }
            else
            {
                UploadResult = "上传失败";
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
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result) 
            {
                SendResult = "发布成功";
            }
            else
            {
                SendResult = "发布失败";
            }
            
        }


    }
}
