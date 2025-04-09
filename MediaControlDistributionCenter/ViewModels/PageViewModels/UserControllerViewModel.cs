using CommunityToolkit.Mvvm.ComponentModel;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserControllerViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        [ObservableProperty]
        private string currentTabName;

        [ObservableProperty]
        private string currentPageName;

        public UserControllerViewModel()
        {
            RegisterLanguageProperty(this.GetType(), nameof(CurrentPageName));
        }

        public void SetValues(string tabName, string pageName)
        {
            CurrentTabName = tabName;
            CurrentPageName = pageName;
        }

        public override void LoadData()
        {
            return;
        }
    }
}
