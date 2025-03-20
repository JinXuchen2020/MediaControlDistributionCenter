using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class PageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ConnectionMode connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
        
        [ObservableProperty]
        private bool isDeviceConnected = App.ServicesProvider.GetRequiredService<Communication>().netClient.IsConnected;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool? canDelete;

        private static Dictionary<Type, List<string>> languagePropertyCache = new Dictionary<Type, List<string>>();

        public virtual void LoadData(long? groupId = null)
        {

        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageboxId);
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
