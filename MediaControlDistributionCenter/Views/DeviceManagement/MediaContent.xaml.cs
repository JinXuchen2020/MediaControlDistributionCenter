

using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.ViewModels.PageViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using System.Windows.Controls;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// 界面 的交互逻辑
    /// </summary>
    public partial class MediaContent : FrameControl
    {
        private readonly MediaContentViewModel manageViewModel;
        public MediaContent(MediaContentViewModel mediaContentViewModel)
        {
            InitializeComponent();
            manageViewModel = mediaContentViewModel;
            manageViewModel.LoadData();
            DataContext = mediaContentViewModel;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(sender is TabControl tabControl)
            {
                var tabItem = tabControl.SelectedItem as TabItem;
                if (tabItem.Tag.ToString() == "All")
                {
                    manageViewModel.LoadData();
                }
                else
                {
                    manageViewModel.GetData(tabItem.Tag.ToString());
                }
            }
        }
    }
}
