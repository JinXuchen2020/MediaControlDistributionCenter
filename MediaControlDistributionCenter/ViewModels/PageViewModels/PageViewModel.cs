using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class PageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ConnectionMode connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
        
        [ObservableProperty]
        private static bool isDeviceConnected = App.ServicesProvider.GetRequiredService<Communication>().netClient.IsConnected;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string? searchString;

        [ObservableProperty]
        private bool? canDelete;

        [ObservableProperty]
        private static ObservableCollection<InternetDevice> connectedDevices = new ObservableCollection<InternetDevice>();

        private static Dictionary<Type, List<string>> languagePropertyCache = new Dictionary<Type, List<string>>();

        public virtual void LoadData()
        {

        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageboxId);
        }

        [RelayCommand]
        private async Task Search()
        {
            await SearchContent();
        }

        protected T GetService<T>() where T : class
        {
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            switch (connectionMode.Mode)
            {
                case "Local":
                    return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                case "Remote":
                    if(string.IsNullOrEmpty(connectionMode.ServiceUri))
                    {
                        return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                    }
                    else
                    {
                        return App.ServicesProvider.GetServices<T>().First(c => !c.GetType().Name.EndsWith("Local"));
                    }
                default:
                    throw new ArgumentException("未知的服务名称");
            }
        }

        protected string FindResource(string key)
        {
            return (string)LanguageTool.Instance.FindResource(key);
        }

        protected virtual async Task SearchContent()
        {
            await Task.CompletedTask;
        }

        protected void RegisterLanguageProperty(Type parentType, string propertyName)
        {
            if (!languagePropertyCache.ContainsKey(parentType))
            {
                languagePropertyCache.Add(parentType, new List<string> { propertyName });
            }
            else if (!languagePropertyCache[parentType].Contains(propertyName))
            {
                languagePropertyCache[parentType].Add(propertyName);
            }
        }

        protected async Task DetectCommunication(string userAccount)
        {
            var client = App.ServicesProvider.GetRequiredService<Communication>();
            var localDevice = ConnectedDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet);
            var localDeviceModel = localDevice?.DeviceViewModel;
            if (localDevice != null && localDeviceModel != null && localDeviceModel.UserId == userAccount && localDeviceModel.SelectedIpAddress == client.IpAddr && client.netClient.State == Helpers.SocketClient.SocketState.Connected)
            {
                return;
            }

            var ipAddress = NetworkTool.GetGatewayIp();
            foreach (var address in ipAddress)
            {
                client.Connect(address, "5001");
                int count = 10;
                while (client.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
                {
                    await Task.Delay(500);
                    count--;
                }

                if (client.netClient.State == Helpers.SocketClient.SocketState.Connected)
                {
                    break;
                }
            }

            if (client.netClient.State != Helpers.SocketClient.SocketState.Connected)
            {
                if (localDevice != null)
                {
                    localDevice.DeviceViewModel = null;
                }
                return;
            }

            string path = CommunicationCmd.CmdSyncSnCode + "Connect";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSyncSnCode} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                await ShowConfirmDialogCommand.ExecuteAsync(null);
                return;
            }

            var snCode = client.SyncSnCodeResult ?? string.Empty;

            var monitorService = GetService<IMonitorService>();
            var connectedDevice = monitorService.GetAll(new MonitorDto { SnCode = snCode }).GetAwaiter().GetResult().Data?.FirstOrDefault();
            if (connectedDevice != null)
            {
                if (localDevice == null)
                {
                    localDevice = new InternetDevice
                    {
                        SnCode = snCode,
                        IpAddress = client.IpAddr,
                        Status = 1,
                        StatusText = GetStatus(1),
                        TypeText = GetDeviceType(false)
                    };

                    ConnectedDevices.Add(localDevice);
                }
                localDevice.DeviceViewModel = new DeviceViewModel();
                localDevice.DeviceViewModel.Binding(connectedDevice);
                localDevice.DeviceViewModel.ConnectCommand.Execute(client);
                client.StartHeart();
            }
        }

        public void TranslateLanguageProperties()
        {
            foreach (var item in languagePropertyCache)
            {
                foreach(var proName in item.Value)
                {
                    var typeObj = App.ServicesProvider.GetRequiredService(item.Key);
                    var property = item.Key.GetProperty(proName);
                    if (property != null)
                    {
                        var propertyValue = (string)property.GetValue(typeObj)!;
                        property.SetValue(typeObj, LanguageTool.Instance.GetResourceTextValue(propertyValue));
                    }
                    else
                    {
                        var method = item.Key.GetMethod(proName);
                        var parameters = method?.GetParameters();
                        if (parameters != null)
                        {
                            var parameterValues = new List<object?>();
                            foreach (var parameter in parameters)
                            {
                                parameterValues.Add(Activator.CreateInstance(parameter.ParameterType));
                            }
                            method?.Invoke(typeObj, [.. parameterValues]);
                        }
                        else
                        {
                            method?.Invoke(typeObj, null);  
                        }
                    }
                }                
            }
        }

        public string GetStatus(int status)
        {
            return status == 1 ? FindResource("LanguageKey_Code_Connected") : FindResource("LanguageKey_Code_Disconnected");
        }

        public string GetDeviceType(bool isInternet)
        {
            return isInternet ? FindResource("LanguageKey_Code_Device_Tooltip_118") : FindResource("LanguageKey_Code_Device_Tooltip_119");
        }
    }
}
