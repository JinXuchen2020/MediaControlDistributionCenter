using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public abstract partial class DataViewModel<DTO> : ObservableValidator
    {
        public abstract DTO ToModel();

        public abstract void Binding(DTO model, bool isSelected = false);

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
