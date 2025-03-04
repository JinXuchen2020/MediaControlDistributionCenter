using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    /// <summary>
    /// MediaPreview.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPreview : Window
    {
        private DispatcherTimer _timer;

        private double _ratio;

        private int currentPlayCount = 0;
        public MediaPreview(MediaEditViewModel viewModel)
        {
            InitializeComponent();
            viewModel.SelectedPage = viewModel.MediaConfig.Pages.First();
            DataContext = viewModel;

            InitializeTimer(viewModel);

            MainCanvas.Width = Width;
            MainCanvas.Height = Height;
            _ratio = MainCanvas.Width / 768;

            LoadCanvasComponents(viewModel);
            this.SizeChanged += MediaPreview_SizeChanged;
        }

        private void MediaPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainCanvas.Width = (sender as Window).Width;
            MainCanvas.Height = (sender as Window).Width; 
            _ratio = MainCanvas.Width / 768;

            LoadCanvasComponents((MediaEditViewModel)DataContext);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InitializeTimer(MediaEditViewModel viewModel)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            var pageTimeline = viewModel.SelectedPage.Components.Count == 0 ? 5 : viewModel.SelectedPage.Components.Select(c => c.Timeline).Max() + 1;
            _timer.Interval = TimeSpan.FromSeconds(pageTimeline);
            _timer.Tick += Timer_Tick; ;
            _timer.Start();
            currentPlayCount++;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var viewModel = (MediaEditViewModel)DataContext;
            if (currentPlayCount < viewModel.SelectedPage.PlayCount)
            {
                LoadCanvasComponents(viewModel);
                InitializeTimer(viewModel);
            }
            else
            {
                currentPlayCount = 0;
                var nextPage = viewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order > viewModel.SelectedPage.Order);
                if (nextPage != null)
                {
                    viewModel.SelectedPage = nextPage;
                    LoadCanvasComponents(viewModel);
                    InitializeTimer(viewModel);
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        private void LoadCanvasComponents(MediaEditViewModel viewModel)
        {
            MainCanvas.Children.Clear();
            if (viewModel.SelectedPage != null)
            {
                foreach (var component in viewModel.SelectedPage.Components)
                {
                    if (component == null) continue;
                    component.Ratio = _ratio;
                    switch (component.Type)
                    {
                        case "Image":
                            var imageComponent = component as ImageComponentViewModel;
                            imageComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                            break;
                        case "Video":
                            var videoComponent = component as VideoComponentViewModel;
                            videoComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                            break;
                        case "Text":
                            var textComponent = component as TextComponentViewModel;
                            textComponent!.DrawRunningContentCommand.Execute(MainCanvas);
                            break;
                    }
                }
            }
        }
    }
}
