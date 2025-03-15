using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class PageViewModel : ObservableObject
    {
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
                    return App.ServicesProvider.GetServices<T>().First(c => !c.GetType().Name.EndsWith("Local"));
                default:
                    throw new ArgumentException("未知的服务名称");
            }
        }
    }
}
