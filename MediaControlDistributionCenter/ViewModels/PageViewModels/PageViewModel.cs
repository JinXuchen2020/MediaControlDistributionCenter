using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
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
        private static DeviceViewModel? connectedDevice;

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
            if (ConnectedDevice != null && ConnectedDevice.UserId == userAccount && ConnectedDevice.SelectedIpAddress == client.IpAddr && client.netClient.State == Helpers.SocketClient.SocketState.Connected)
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

            //if (client.netClient.State != Helpers.SocketClient.SocketState.Connected)
            //{
            //    ConnectedDevice = null;
            //    return;
            //}

            //string path = CommunicationCmd.CmdSyncSnCode + "Connect";
            //bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            //if (!result)
            //{
            //    ErrorMessage = $"{CommunicationCmd.CmdSyncSnCode} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
            //    await ShowConfirmDialogCommand.ExecuteAsync(null);
            //    return;
            //}

            var snCode = client.SyncSnCodeResult;

            var monitorService = GetService<IMonitorService>();
            var connectedDevice = monitorService.GetAll(new MonitorDto { SnCode = snCode }).GetAwaiter().GetResult().Data?.FirstOrDefault();
            if (connectedDevice != null)
            {
                ConnectedDevice = new DeviceViewModel();
                ConnectedDevice.Binding(connectedDevice);
                ConnectedDevice.ConnectCommand.Execute(client);
                //client.StartHeart();
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
    }
}
