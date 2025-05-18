using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public abstract partial class DataViewModel<DTO> : ObservableValidator
    {
        public abstract DTO ToModel();

        public abstract void Binding(DTO model, bool isSelected = false);

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private ConnectionMode connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();        

        protected static string FindResource(string key)
        {
            return (string)LanguageTool.Instance.FindResource(key);
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
