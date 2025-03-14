using CommunityToolkit.Mvvm.ComponentModel;

namespace MediaControlDistributionCenter.ViewModels
{
    public abstract class PageViewModel : ObservableObject
    {
        public abstract void LoadData(long? groupId = null);
    }
}
