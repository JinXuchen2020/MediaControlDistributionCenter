using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class PageViewModel : ObservableObject
    {
        protected ConnectionMode ConnectionMode => App.ServicesProvider.GetRequiredService<ConnectionMode>();
        
        [ObservableProperty]
        private bool isDeviceConnected = App.ServicesProvider.GetRequiredService<Communication>().netClient.IsConnected;

        public virtual void LoadData(long? groupId = null)
        {

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
    }
}
