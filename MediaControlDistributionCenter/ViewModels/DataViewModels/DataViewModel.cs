using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public abstract partial class DataViewModel<DTO> : ObservableValidator
    {
        public abstract DTO ToModel();

        public abstract void Binding(DTO model, bool isSelected = false);

        protected static T GetService<T>() where T : class
        {
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            switch (connectionMode.Mode)
            {
                case "Local":
                    return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                case "Remote":
                    if (string.IsNullOrEmpty(connectionMode.ServiceUri))
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

        [RelayCommand]
        protected void Submit()
        {
            ValidateAllProperties();
        }

        [RelayCommand]
        protected void LogDrag(object element)
        {
            ValidateAllProperties();
        }
    }
}
