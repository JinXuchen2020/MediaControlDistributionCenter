using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.Diagrams;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using System.Xml.Linq;

namespace MediaControlDistributionCenter.Views
{
    /// <summary>
    /// MediaEdit.xaml 的交互逻辑
    /// </summary>
    public partial class MediaEdit : UserControl
    {
        private IFileService fileService;
        private readonly IServiceProvider serviceProvider;
        private readonly MediaEditViewModel manageViewModel;
        
        public MediaEdit(DashboardViewModel dashboardViewModel, MediaManageViewModel mediaManageViewModel, MediaEditViewModel mediaEditViewModel, IFileService fileService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            this.fileService = fileService;
            this.serviceProvider = serviceProvider;

            if (dashboardViewModel.CurrentUser.Role == "user")
            {
                mediaEditViewModel.CurrentMedia = dashboardViewModel.SelectedMedia ?? mediaManageViewModel.SelectedMedia!;
                mediaEditViewModel.CurrentUser = dashboardViewModel.CurrentUser;
                mediaEditViewModel.ShowNavigation = true;
            }
            else
            {
                mediaEditViewModel.CurrentMedia = mediaManageViewModel.SelectedMedia;
                mediaEditViewModel.CurrentUser = mediaManageViewModel.CurrentUser;
                mediaEditViewModel.ShowNavigation = mediaManageViewModel.ShowNavigation;
            }

            manageViewModel = mediaEditViewModel;
            manageViewModel.SetValues(MainCanvas);
            DataContext = mediaEditViewModel;

            this.Loaded += MediaEdit_Loaded;
            this.Unloaded += MediaEdit_Unloaded;
        }

        private void MediaEdit_Unloaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.Children.Clear();
            manageViewModel.DisposeCommand.Execute(null);
        }

        private void MediaEdit_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCanvasComponents(manageViewModel);
        }

        private void LoadCanvasComponents(MediaEditViewModel viewModel)
        {
            MainCanvas.Children.Clear();
            if (viewModel.SelectedPage != null)
            {
                foreach (var component in viewModel.SelectedPage.Components)
                {
                    if (component == null) continue;
                    switch (component.Type)
                    {
                        case "Image":
                            var imageComponent = component as ImageComponentViewModel;
                            imageComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "Video":
                            var videoComponent = component as VideoComponentViewModel;
                            videoComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "Text":
                            var textComponent = component as TextComponentViewModel;
                            textComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                    }
                }
            }
        }

        private void btnBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (manageViewModel.ShowNavigation)
            {
                var content = serviceProvider.GetRequiredService<MediaManage>();
                (App.Current.MainWindow as MainWindow).GoContent(content, 2);
            }
            else
            {
                var content = serviceProvider.GetRequiredService<UserControllers>();
                (App.Current.MainWindow as MainWindow).GoContent(content, 2);
            }
        }

        private void MainCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainCanvas_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void MainCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string filePath = files.FirstOrDefault()!;

                UpdateFileComponent(filePath, e.GetPosition(MainCanvas).X, e.GetPosition(MainCanvas).Y);
            }
        }

        private bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            string ext = System.IO.Path.GetExtension(filePath);
            return imageExtensions.Contains(ext);
        }

        private bool IsVideoFile(string filePath)
        {
            string[] imageExtensions = { ".mp4", ".avi", ".wmv", ".mkv" };
            string ext = System.IO.Path.GetExtension(filePath);
            return imageExtensions.Contains(ext);
        }

        private void UpdateFileComponent(string filePath, double left = 0 , double top = 0)
        {
            var pageViewModel = manageViewModel.SelectedPage;
            var currentComponent = pageViewModel!.Components.FirstOrDefault(c => c!.IsSelected);
            if (currentComponent == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_147");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            string fileName = System.IO.Path.GetFileName(filePath);
            string localFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, manageViewModel.MediaConfig.Name, pageViewModel.Name, currentComponent.Name);

            if (IsImageFile(filePath))
            {
                if (currentComponent.Type != MediaType.Image.ToString())
                {
                    manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_148");
                    manageViewModel.ShowConfirmDialogCommand.Execute(null);
                    return;
                }

                var uploadPath = System.IO.Path.Combine(localFolder, fileName);
                if (!File.Exists(uploadPath))
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    uploadPath = fileService.SaveFileContent(localFolder, fileName, fileStream);
                }

                var viewModel = (currentComponent as ImageComponentViewModel)!;
                viewModel.Left = left;
                viewModel.Top = top;
                viewModel.Width = 300;
                viewModel.Height = 200;
                viewModel.Source = uploadPath;
                viewModel.FileName = fileName;
                viewModel.IsShowInfo = true;

                viewModel.DrawContentCommand.Execute(MainCanvas);
                manageViewModel.SelectedElement = viewModel.FrameworkElement;
            }
            else if (IsVideoFile(filePath))
            {
                if (currentComponent.Type != MediaType.Video.ToString())
                {
                    manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_149");
                    manageViewModel.ShowConfirmDialogCommand.Execute(null);
                    return;
                }

                var uploadPath = System.IO.Path.Combine(localFolder, fileName);
                if (!File.Exists(uploadPath))
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    uploadPath = fileService.SaveFileContent(localFolder, fileName, fileStream);
                }

                var viewModel = currentComponent;
                viewModel.Left = left;
                viewModel.Top = top;
                viewModel.Width = 300;
                viewModel.Height = 200;
                viewModel.Source = uploadPath;
                viewModel.FileName = fileName;
                viewModel.IsShowInfo = true;

                viewModel.DrawContentCommand.Execute(MainCanvas);
                manageViewModel.SelectedElement = viewModel.FrameworkElement;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.MediaConfig.Pages.Count == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_150");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            manageViewModel.MediaConfig.CaptureCommand.Execute(MainCanvas);

            var configModel = manageViewModel.MediaConfig.ToModel();

            var configContent = JsonConvert.SerializeObject(configModel);

            var mediaResourcePath = System.IO.Path.Combine(Helpers.Constants.OutPath, configModel.Name);

            fileService.SaveFileContent(mediaResourcePath, Helpers.Constants.ConfigFileName, configContent);

            manageViewModel.CurrentMedia.ShowConfirmDialogCommand.Execute(null);
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            var sourceDic = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentMedia.Name);

            if(!File.Exists(System.IO.Path.Combine(sourceDic, Helpers.Constants.ConfigFileName)))
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_151");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var desZipFilePath = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentMedia.Name + ".zip");
            fileService.CreatZip(sourceDic, desZipFilePath);

            var fileSize = (double)new FileInfo(desZipFilePath).Length / 1024 /1024;

            manageViewModel.CurrentMedia.Size = fileSize;
            manageViewModel.SaveCommand.Execute(null);

            var viewModel = serviceProvider.GetRequiredService<MediaDevicesViewModel>();
            viewModel.CurrentMedia = manageViewModel.CurrentMedia;
            viewModel.LoadData();
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaDevicesViewModel)!;
            this.Dispatcher.Invoke(async () =>
            {
                await viewModel.PublishCommand.ExecuteAsync(null);

                var screenCount = viewModel.Devices.Select(c => c.IsSelected).Count();
                manageViewModel.CurrentMedia.ScreensCount = screenCount;
                manageViewModel.SaveCommand.Execute(null);

                if (viewModel.PublishDevices.Count > 0)
                {
                    manageViewModel.CloseDialogCommand.Execute(null);

                    viewModel.ShowConfirmDialogCommand.Execute(null);

                    //if (manageViewModel.ShowNavigation)
                    //{
                    //    var content = new MediaManage(manageViewModel.CurrentUser, true);
                    //    (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
                    //}
                    //else
                    //{
                    //    var userControllers = new UserControllers(manageViewModel.CurrentUser!);
                    //    (App.Current.MainWindow as MainWindow).GoCotent(userControllers, 2);
                    //}
                }
            });
        }

        private void SelectDay_Click(object sender, RoutedEventArgs e)
        {
            var dayViewModel = ((sender as Button).DataContext as SchedulerDayViewModel)!;
            if(dayViewModel.IsSelected)
            {
                dayViewModel.IsSelected = false;
                manageViewModel.SelectedPage.Schedulers.First(c => c.Id == dayViewModel.SchedulerId).ScheduleDays.Remove(dayViewModel.Id);
            }
            else
            {
                dayViewModel.IsSelected = true;
                manageViewModel.SelectedPage.Schedulers.First(c => c.Id == dayViewModel.SchedulerId).ScheduleDays.Add(dayViewModel.Id);
            }
        }

        private void AddScheduler_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var schedulers = manageViewModel.SelectedPage.Schedulers;

            var newScheduler = new SchedulerViewModel(schedulers.Count() + 1, string.Empty, string.Empty, new List<int>());
            schedulers.Add(newScheduler);
        }

        private void DeleteScheduler_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var schedulers = manageViewModel.SelectedPage.Schedulers;

            if (schedulers.Count > 1)
            {
                var viewModel = ((sender as PackIcon).DataContext as SchedulerViewModel)!;
                manageViewModel.SelectedPage.Schedulers.Remove(viewModel);
            }
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (manageViewModel.SelectedComponent != null && manageViewModel.SelectedElement != null)
            {
                Canvas.SetLeft(manageViewModel.SelectedElement, manageViewModel.SelectedComponent.Left);
                Canvas.SetTop(manageViewModel.SelectedElement, manageViewModel.SelectedComponent.Top);
            }
        }

        private void btnPageAdd_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new MediaPageViewModel(new MediaPage
            {
                Id = manageViewModel.MediaConfig.Pages.Count > 0 ? manageViewModel.MediaConfig.Pages.Select(c => c.Id).Max() + 1 : 1,
                Order = manageViewModel.MediaConfig.Pages.Count > 0 ? manageViewModel.MediaConfig.Pages.Select(c => c.Order).Max() + 1 : 1,
                PlayCount = 1,
                Schedulers = new List<Scheduler> { new Scheduler { Id = 1, ScheduleDays = new List<int>() } },
                Components = new List<BaseComponent>()
            });

            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnPageSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = ((sender as Button).DataContext as MediaPageViewModel)!;
            viewModel.SubmitCommand.Execute(null);
            if (!viewModel.HasErrors)
            {
                if (manageViewModel.SelectedPage != null)
                {
                    manageViewModel.SelectedPage.IsSelected = false;
                }
                manageViewModel.SelectedPage = viewModel;
                viewModel.IsSelected = true;
                manageViewModel.MediaConfig.Pages.Add(viewModel);
                manageViewModel.CloseDialogCommand.Execute(null);
                LoadCanvasComponents(manageViewModel);
            }
        }

        private void btnPageDelete_Click(object sender, RoutedEventArgs e)
        {
            if(manageViewModel.MediaConfig.Pages.Count > 1)
            {
                var currentPage = manageViewModel.SelectedPage;
                currentPage.IsSelected = false;
                manageViewModel.MediaConfig.Pages.Remove(currentPage);
                manageViewModel.SelectedPage = manageViewModel.MediaConfig.Pages.First();
                manageViewModel.SelectedPage.IsSelected = true;
            }
        }

        private void btnPageUpOrder_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.MediaConfig.Pages.Count > 1)
            {
                var prePage = manageViewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order < manageViewModel.SelectedPage.Order);
                if (prePage != null) 
                {
                    var currentOrder = manageViewModel.SelectedPage.Order;
                    manageViewModel.SelectedPage.Order = prePage.Order;
                    prePage.Order = currentOrder;
                }

                manageViewModel.MediaConfig.Pages = new ObservableCollection<MediaPageViewModel>(manageViewModel.MediaConfig.Pages.OrderBy(c => c.Order).ToList());
            }
        }

        private void btnPageDownOrder_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.MediaConfig.Pages.Count > 1)
            {
                var nextPage = manageViewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order > manageViewModel.SelectedPage.Order);
                if (nextPage != null)
                {
                    var currentOrder = manageViewModel.SelectedPage.Order;
                    manageViewModel.SelectedPage.Order = nextPage.Order;
                    nextPage.Order = currentOrder;
                }

                manageViewModel.MediaConfig.Pages = new ObservableCollection<MediaPageViewModel>(manageViewModel.MediaConfig.Pages.OrderBy(c => c.Order).ToList());
            }
        }

        private void SelectPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ((sender as StackPanel).DataContext as MediaPageViewModel)!;
            manageViewModel.MediaConfig.Pages.First(c => c.IsSelected).IsSelected = false;
            manageViewModel.SelectedPage = viewModel;
            manageViewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
            viewModel.IsSelected = true;
            LoadCanvasComponents(manageViewModel);
        }

        private void SelectComponent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = ((sender as DockPanel).DataContext as BaseComponentViewModel)!;
            manageViewModel.SelectedElement = null;
            SwitchComponent(viewModel);
        }

        private void btnComponentDelete_Click(object sender, RoutedEventArgs e)
        {
            if(manageViewModel.SelectedComponent != null)
            {
                var currentCom = manageViewModel.SelectedComponent;
                currentCom.IsSelected = false;
                manageViewModel.SelectedPage.Components.Remove(currentCom);
                if (currentCom.FrameworkElement != null)
                {
                    MainCanvas.Children.Remove(currentCom.FrameworkElement);
                }
                manageViewModel.SelectedComponent = null;
                manageViewModel.SelectedElement = null;
            }
        }

        //private void btnMediaUpOrder_Click(object sender, RoutedEventArgs e)
        //{
        //    var manageViewModel = (DataContext as MediaEditViewModel)!;
        //    var currentMedia = manageViewModel.SelectedPage.MixedMedias.First(c => c.IsSelected);
        //    if (manageViewModel.SelectedPage.MixedMedias.Count > 1)
        //    {
        //        var prePage = manageViewModel.SelectedPage.MixedMedias.FirstOrDefault(c => c.Order < currentMedia.Order);
        //        if (prePage != null)
        //        {
        //            var currentOrder = currentMedia.Order;
        //            currentMedia.Order = prePage.Order;
        //            prePage.Order = currentOrder;
        //        }

        //        manageViewModel.SelectedPage.MixedMedias = new System.Collections.ObjectModel.ObservableCollection<MixedMediaViewModel>(manageViewModel.SelectedPage.MixedMedias.OrderBy(c => c.Order).ToList());
        //    }
        //}

        //private void btnMediaDownOrder_Click(object sender, RoutedEventArgs e)
        //{
        //    var manageViewModel = (DataContext as MediaEditViewModel)!;
        //    var currentMedia = manageViewModel.SelectedPage.MixedMedias.First(c => c.IsSelected);
        //    if (manageViewModel.SelectedPage.MixedMedias.Count > 1)
        //    {
        //        var nextPage = manageViewModel.SelectedPage.MixedMedias.FirstOrDefault(c => c.Order > currentMedia.Order);
        //        if (nextPage != null)
        //        {
        //            var currentOrder = currentMedia.Order;
        //            currentMedia.Order = nextPage.Order;
        //            nextPage.Order = currentOrder;
        //        }

        //        manageViewModel.SelectedPage.MixedMedias = new System.Collections.ObjectModel.ObservableCollection<MixedMediaViewModel>(manageViewModel.SelectedPage.MixedMedias.OrderBy(c => c.Order).ToList());
        //    }
        //}

        private void SwitchComponent(BaseComponentViewModel viewModel)
        {
            if(manageViewModel.SelectedComponent != null)
            {
                manageViewModel.SelectedComponent.IsSelected = false;
            }

            manageViewModel.SelectedComponent = viewModel;
            manageViewModel.SelectedComponent.IsSelected = true;
            manageViewModel.SelectedElement = viewModel.FrameworkElement;
        }

        private void PlayModeChanged_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton.IsChecked.HasValue && radioButton.IsChecked.Value)
            {
                switch(manageViewModel.SelectedComponent.Type)
                {
                    case "Video":
                        (manageViewModel.SelectedComponent as VideoComponentViewModel).PlayMode = radioButton.Tag?.ToString();
                        break;
                    case "Text":
                        (manageViewModel.SelectedComponent as TextComponentViewModel).PlayMode = radioButton.Tag?.ToString();
                        break;

                }
            }
        }

        private void CreateComponent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var createBtn = (sender as StackPanel)!;

            if(manageViewModel.SelectedPage == null)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_150");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var maxId = manageViewModel.SelectedPage.Components.Count == 0 ? 0 : manageViewModel.SelectedPage.Components.Select(c => c!.Id).Max() + 0;

            if (Enum.TryParse(typeof(MediaType), createBtn.Tag.ToString()!, out var type))
            {
                if (manageViewModel.SelectedComponent != null)
                {
                    manageViewModel.SelectedComponent.IsSelected = false;
                }

                manageViewModel.SelectedElement = null;

                switch (type)
                {
                    case MediaType.Video:
                        manageViewModel.SelectedComponent = new VideoComponentViewModel(new VideoComponent
                        {
                            Id = maxId + 1,
                            Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_103")}{maxId + 1}",
                            ZIndex = 1,
                            PlayMode = "fullscreen",
                            Type = (MediaType)type,
                            PlayCount = 1,
                            PlayDuration = "",
                        });
                        break;
                    case MediaType.Image:
                        manageViewModel.SelectedComponent = new ImageComponentViewModel(new ImageComponent
                        {
                            Id = maxId + 1,
                            Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_104")}{maxId + 1}",
                            ZIndex = 1,
                            Type = (MediaType)type,
                            PlayCount = 1,
                            PlayDuration = "00:00:05",
                            Timeline = 5, 
                            ComponentEffect = "FadeIn",
                            EffectDuration = 1000
                        });
                        break;
                    case MediaType.Text:
                        manageViewModel.SelectedComponent = new TextComponentViewModel(new TextComponent
                        {
                            Id = maxId + 1,
                            Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_105")}{maxId + 1}",
                            ZIndex = 1,
                            Type = (MediaType)type,
                            Source = " ",
                            PlayCount = 1,
                            PlayDuration = "00:00:05",
                            PlayMode = "pageTurning",
                            ComponentEffect = "FadeIn",
                            EffectDuration = 1000,
                            Direction = "rollingLeft",
                            Timeline = 5,
                            Background ="black",
                            TextColor = "white",
                            TextSize = 16,
                            IsLoopEnabled = true,
                            LetterSpacing = 2,
                            LineSpacing = 2,
                            RollingSpeed = 2,
                        });
                        break;
                }

                manageViewModel.SelectedComponent.IsSelected = true;

                if (!manageViewModel.SelectedComponent.IsFile)
                {
                    switch (manageViewModel.SelectedComponent.Type)
                    {
                        case "Text":
                            var textComponent = (manageViewModel.SelectedComponent as TextComponentViewModel)!;
                            textComponent.Width = 300;
                            textComponent.Height = 200;
                            textComponent.DrawContentCommand.Execute(MainCanvas);
                            manageViewModel.SelectedElement = textComponent.FrameworkElement;
                            break;
                    }
                }

                manageViewModel.SelectedPage.Components.Add(manageViewModel.SelectedComponent);
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.MediaConfig.Pages.Count == 0)
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_150");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var previewWindow = new MediaPreview(manageViewModel);
            previewWindow.Show();
        }

        private void ClearContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is CustomIcon packIcon)
            {
                var viewModel = (BaseComponentViewModel)packIcon.DataContext;
                if (viewModel.IsFile)
                {
                    MainCanvas.Children.Remove(viewModel.FrameworkElement);
                    viewModel.FrameworkElement = null;
                    manageViewModel.SelectedElement = null;
                    viewModel.Source = null;
                    viewModel.FileName = null;
                    viewModel.IsShowInfo = false;
                }
                else
                {
                    viewModel.Source = null;
                }
            }
        }

        private void Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (manageViewModel.SelectedComponent != null && manageViewModel.SelectedElement != null)
            {
                Canvas.SetZIndex(manageViewModel.SelectedElement, manageViewModel.SelectedComponent.ZIndex);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox textBox)
            {
                ((TextComponentViewModel)manageViewModel.SelectedComponent).Foreground = (Color)ColorConverter.ConvertFromString(textBox.Text);
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (TextComponentViewModel)manageViewModel.SelectedComponent;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnUpload_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;

                UpdateFileComponent(filePath);
            }
        }

        private void btnPageCapture_Click(object sender, RoutedEventArgs e)
        {
            manageViewModel.MediaConfig.CaptureCommand.Execute(MainCanvas);
        }

        private void LeftTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is TabControl tab)
            {
                var tabItem = tab.SelectedIndex;
                if (tabItem == 1)
                {
                    manageViewModel.MediaConfig.CaptureCommand.Execute(MainCanvas);
                }
            }
        }
    }
}
