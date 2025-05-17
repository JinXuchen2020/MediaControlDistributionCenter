using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows;

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

        private static Dictionary<Type, List<string>> languagePropertyCache = new Dictionary<Type, List<string>>();

        private static Dictionary<Type, List<string>> devicesChangedRegisterActions = new Dictionary<Type, List<string>>();

        protected IDetectService detectService;

        public List<InternetDevice> OnlineDevices 
        {
            get
            {
                var devices = detectService.GetOnlineDevices();
                foreach (var item in devices)
                {
                    item.StatusText = GetStatus(item.Status);
                    item.TypeText = GetDeviceType(item.IsInternet);
                }
                return devices.ToList();
            }
        }

        public PageViewModel()
        {
            detectService = Utility.GetService<IDetectService>();
            detectService.InvokeDevicesChanged += (sender, e) => InvokeDevicesChanged();
        }

        public virtual async Task LoadData()
        {
            await Task.CompletedTask;

        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageBoxId);
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

        protected void RegisterDevicesChangedAction(Type parentType, string propertyName)
        {
            if (!devicesChangedRegisterActions.ContainsKey(parentType))
            {
                devicesChangedRegisterActions.Add(parentType, new List<string> { propertyName });
            }
            else if (!devicesChangedRegisterActions[parentType].Contains(propertyName))
            {
                devicesChangedRegisterActions[parentType].Add(propertyName);
            }
        }

        public void InvokeDevicesChanged()
        {
            foreach (var item in devicesChangedRegisterActions)
            {
                foreach (var proName in item.Value)
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
                            if (method != null && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task))
                            {
                                Task.Run(async () =>
                                {
                                    if (method.Invoke(typeObj, [.. parameterValues]) is Task methodTask)
                                    {
                                        await methodTask;
                                    }

                                }).Wait();
                            }
                            else
                            {
                                method?.Invoke(typeObj, [.. parameterValues]);
                            }
                        }
                        else
                        {
                            method?.Invoke(typeObj, null);
                        }
                    }
                }
            }
        }

        public void TranslateLanguageProperties()
        {
            foreach (var item in languagePropertyCache)
            {
                foreach (var proName in item.Value)
                {
                    var typeObj = App.ServicesProvider.GetRequiredService(item.Key);
                    var property = item.Key.GetProperty(proName);
                    if (property != null)
                    {
                        var propertyValue = (string)property.GetValue(typeObj)!;
                        if (propertyValue != null)
                        {
                            property.SetValue(typeObj, LanguageTool.Instance.GetResourceTextValue(propertyValue));
                        }
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

                            if (method != null && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task))
                            {
                                Application.Current.Dispatcher.Invoke(async () =>
                                {
                                    if (method.Invoke(typeObj, [.. parameterValues]) is Task methodTask)
                                    {
                                        await methodTask;
                                    }

                                }).Wait();
                            }
                            else
                            {
                                method?.Invoke(typeObj, [.. parameterValues]);
                            }
                        }
                        else
                        {
                            method?.Invoke(typeObj, null);
                        }
                    }
                }
            }
        }

        //protected async Task DetectCommunication(string userAccount)
        //{
        //    var client = App.ServicesProvider.GetRequiredService<Communication>();
        //    var localDevice = OnlineDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet);
        //    var localDeviceModel = localDevice?.DeviceViewModel;
        //    if (localDevice != null && localDeviceModel != null && localDeviceModel.UserId == userAccount && localDeviceModel.SelectedIpAddress == client.IpAddr && client.netClient.State == Helpers.SocketClient.SocketState.Connected)
        //    {
        //        return;
        //    }

        //    var ipAddress = NetworkTool.GetGatewayIp();
        //    foreach (var address in ipAddress)
        //    {
        //        client.Connect(address, "5001");
        //        int count = 10;
        //        while (client.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
        //        {
        //            await Task.Delay(500);
        //            count--;
        //        }

        //        if (client.netClient.State == Helpers.SocketClient.SocketState.Connected)
        //        {
        //            break;
        //        }
        //    }

        //    if (client.netClient.State != Helpers.SocketClient.SocketState.Connected)
        //    {
        //        if (localDevice != null)
        //        {
        //            localDevice.DeviceViewModel = null;
        //        }
        //        return;
        //    }

        //    string path = CommunicationCmd.CmdSyncSnCode + "Connect";
        //    bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
        //    if (!result)
        //    {
        //        ErrorMessage = $"{CommunicationCmd.CmdSyncSnCode} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
        //        await ShowConfirmDialogCommand.ExecuteAsync(null);
        //        return;
        //    }

        //    var snCode = client.SyncSnCodeResult ?? string.Empty;

        //    var monitorService = GetService<IMonitorService>();
        //    var connectedDevice = monitorService.GetAll(new MonitorDto { SnCode = snCode })).Data?.FirstOrDefault();
        //    if (connectedDevice != null)
        //    {
        //        if (localDevice == null)
        //        {
        //            localDevice = new InternetDevice
        //            {
        //                SnCode = snCode,
        //                IpAddress = client.IpAddr,
        //                Status = 1,
        //                StatusText = GetStatus(1),
        //                IsInternet = false,
        //                TypeText = GetDeviceType(false)
        //            };

        //            OnlineDevices.Add(localDevice);
        //        }
        //        else
        //        {
        //            if (localDevice.IpAddress != client.IpAddr)
        //            {
        //                OnlineDevices.Remove(localDevice);
        //                localDevice = new InternetDevice
        //                {
        //                    SnCode = snCode,
        //                    IpAddress = client.IpAddr,
        //                    Status = 1,
        //                    StatusText = GetStatus(1),
        //                    IsInternet = false,
        //                    TypeText = GetDeviceType(false)
        //                };

        //                OnlineDevices.Add(localDevice);
        //            }
        //        }

        //        localDevice.DeviceViewModel = new DeviceViewModel();
        //        localDevice.DeviceViewModel.Binding(connectedDevice);
        //        localDevice.DeviceViewModel.ConnectCommand.Execute(client);
        //        client.StartHeart();

        //        InvokeDevicesChanged();
        //    }
        //}

        [RelayCommand]
        private async Task DetectInternetDevices()
        {
            await detectService.StartDetect();            
        }

        [RelayCommand]
        private async Task SendBroadcastMessage()
        {
            await detectService.SendBroadcastMessage();
        }

        [RelayCommand]
        private async Task ConnectInternetDevice(InternetDevice device)
        {
            await detectService.ConnectDevice(device);
            if(device.DeviceViewModel != null && !string.IsNullOrEmpty(device.DeviceViewModel.ErrorMessage))
            {
                ErrorMessage = FindResource(device.DeviceViewModel.ErrorMessage);
                await ShowConfirmDialogCommand.ExecuteAsync(null);
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
