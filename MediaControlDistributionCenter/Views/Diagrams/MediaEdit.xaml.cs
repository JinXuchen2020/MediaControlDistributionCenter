using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Helpers;
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
            manageViewModel.Canvas = MainCanvas;
            manageViewModel.LoadData();
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
                        case "Hdmi":
                            var hdmiComponent = component as HdmiComponentViewModel;
                            hdmiComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "Stream":
                            var streamComponent = component as StreamComponentViewModel;
                            streamComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "Web":
                            var webComponent = component as WebComponentViewModel;
                            webComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "Rss":
                            var rssComponent = component as RssComponentViewModel;
                            rssComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "Word":
                            var wordComponent = component as WordComponentViewModel;
                            wordComponent!.DrawContentCommand.Execute(MainCanvas);
                            break;
                        case "ColorText":
                            var colorComponent = component as ColorTextComponentViewModel;
                            colorComponent!.DrawContentCommand.Execute(MainCanvas);
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
            string[] imageExtensions = { ".mp4", ".mp3", ".avi", ".wmv", ".mkv" };
            string ext = System.IO.Path.GetExtension(filePath);
            return imageExtensions.Contains(ext);
        }

        private bool IsWordFile(string filePath)
        {
            string[] extensions = { ".pdf", ".doc", ".docx", ".pptx" };
            string ext = System.IO.Path.GetExtension(filePath);
            return extensions.Contains(ext);
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
            string localFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, manageViewModel.CurrentUser.Account, manageViewModel.CurrentMedia.Name, pageViewModel.Name, currentComponent.Name);

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
            else if (IsWordFile(filePath))
            {
                if (currentComponent.Type != MediaType.Word.ToString())
                {
                    manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_195");
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
                viewModel.Width = 229;
                viewModel.Height = 329;
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

            manageViewModel.CaptureCommand.Execute(MainCanvas);

            var configModel = manageViewModel.MediaConfig.ToModel();

            var configContent = JsonConvert.SerializeObject(configModel);

            var mediaResourcePath = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentUser.Account, configModel.Program.Name);

            fileService.SaveFileContent(mediaResourcePath, Helpers.Constants.ConfigFileName, configContent);

            manageViewModel.CurrentMedia.ShowConfirmDialogCommand.Execute(null);

            foreach(var deletePage in manageViewModel.MediaConfig.Pages.Where(c => c.IsDeleted))
            {
                fileService.DeleteResourcePath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, manageViewModel.CurrentUser.Account, manageViewModel.CurrentMedia.Name, deletePage.Name));
            }

            foreach (var deletePage in manageViewModel.MediaConfig.Pages.Where(c => c.Components.Any(c => c.IsDeleted)))
            {
                var deleteCompos = deletePage.Components.Where(c => c.IsDeleted);
                foreach (var compo in deleteCompos) 
                {
                    var componentPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, manageViewModel.CurrentUser.Account, manageViewModel.CurrentMedia.Name, deletePage.Name, compo.Name);
                    fileService.DeleteResourcePath(componentPath);
                }
            }
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            var sourceDic = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentUser.Account, manageViewModel.CurrentMedia.Name);

            if(!File.Exists(System.IO.Path.Combine(sourceDic, Helpers.Constants.ConfigFileName)))
            {
                manageViewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_151");
                manageViewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var desZipFilePath = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentUser.Account, manageViewModel.CurrentMedia.Name + ".zip");
            fileService.CreatZip(sourceDic, desZipFilePath);

            var fileSize = (double)new FileInfo(desZipFilePath).Length / 1024 /1024;

            manageViewModel.CurrentMedia.Size = Math.Round(fileSize, 2);
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
                double maxLeft = MainCanvas.Width - manageViewModel.SelectedElement.Width;
                double maxTop = MainCanvas.Height - manageViewModel.SelectedElement.Height;
                double minLeft = 0;
                double minTop = 0;
                manageViewModel.SelectedComponent.Left = Math.Min(Math.Max(minLeft, manageViewModel.SelectedComponent.Left), maxLeft);
                manageViewModel.SelectedComponent.Top = Math.Min(Math.Max(minTop, manageViewModel.SelectedComponent.Top), maxTop);
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
            }, manageViewModel.CurrentUser.Account);

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
            if (manageViewModel.MediaConfig.Pages.Where(c =>!c.IsDeleted).Count() > 1)
            {
                var currentPage = manageViewModel.SelectedPage;
                currentPage.IsSelected = false;
                currentPage.IsDeleted = true;
                manageViewModel.SelectedPage = manageViewModel.MediaConfig.Pages.First();
                manageViewModel.SelectedPage.IsSelected = true;
                LoadCanvasComponents(manageViewModel);
            }
        }

        private void btnPageUpOrder_Click(object sender, RoutedEventArgs e)
        {
            if (manageViewModel.MediaConfig.Pages.Where(c => !c.IsDeleted).Count() > 1)
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
            if (manageViewModel.MediaConfig.Pages.Where(c => !c.IsDeleted).Count() > 1)
            {
                var nextPage = manageViewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order > manageViewModel.SelectedPage.Order && !c.IsDeleted);
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
            if (manageViewModel.SelectedComponent != null)
            {
                var currentCom = manageViewModel.SelectedComponent;
                currentCom.IsSelected = false;
                currentCom.IsDeleted = true;
                if (currentCom.FrameworkElement != null)
                {
                    MainCanvas.Children.Remove(currentCom.FrameworkElement);
                }
                manageViewModel.SelectedComponent = null;
                manageViewModel.SelectedElement = null;
            }
        }

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
                    case "Rss":
                        (manageViewModel.SelectedComponent as RssComponentViewModel).PlayMode = radioButton.Tag?.ToString();
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

                manageViewModel.SelectedComponent = manageViewModel.CreateComponent((MediaType)type, maxId + 1);

                if (manageViewModel.SelectedComponent != null)
                {
                    manageViewModel.SelectedComponent.IsSelected = true;
                    if (!manageViewModel.SelectedComponent.IsFile)
                    {
                        manageViewModel.DrawingComponent(MainCanvas, manageViewModel.SelectedComponent);
                        manageViewModel.SelectedElement = manageViewModel.SelectedComponent.FrameworkElement;
                    }

                    manageViewModel.SelectedPage.Components.Add(manageViewModel.SelectedComponent);
                }
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
            if (manageViewModel.SelectedComponent.Type == "Image")
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // 过滤器，允许的文件类型
            }
            else if (manageViewModel.SelectedComponent.Type == "Video")
            {
                openFileDialog.Filter = "Video Files|*.mp4;*.mp3;*.avi;*.wmv;*.mkv"; // 过滤器，允许的文件类型
            }
            else if (manageViewModel.SelectedComponent.Type == "Word")
            {
                openFileDialog.Filter = "DOC Files|*.pdf; *.doc; *.docx;*.pptx"; // 过滤器，允许的文件类型
            }

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件的路径
                string filePath = openFileDialog.FileName;

                UpdateFileComponent(filePath);
            }
        }

        private void btnPageCapture_Click(object sender, RoutedEventArgs e)
        {
            manageViewModel.CaptureCommand.Execute(MainCanvas);
        }

        private void LeftTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is TabControl tab)
            {
                var tabItem = tab.SelectedIndex;
                if (tabItem == 1)
                {
                    manageViewModel.CaptureCommand.Execute(MainCanvas);
                }
            }
        }

        private void SetRssFontStyle_Click(object sender, RoutedEventArgs e)
        {
            var element = (Button)sender;
            var viewModel = element.DataContext as RssContentViewModel;
            switch (element.Tag.ToString())
            {
                case "Bold":
                    viewModel.IsBold = !viewModel.IsBold;
                    break;
                case "Italic":
                    viewModel.IsItalic = !viewModel.IsItalic;
                    break;
                case "Underline":
                    viewModel.IsUnderline = !viewModel.IsUnderline;
                    break;
            }
        }

        private void SelectRssColor_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (sender as Button).DataContext as RssContentViewModel;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void CreateComponentFromStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (Image)sender;
            var viewModel = element.DataContext as MediaViewModel;

            UpdateFileComponent(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, viewModel.Src));
        }

        private void SelectColorTextColor_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (sender as Button).DataContext as ColorTextComponentViewModel;
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void SetColorTexFontStyle_Click(object sender, RoutedEventArgs e)
        {
            var element = (Button)sender;
            var viewModel = element.DataContext as ColorTextComponentViewModel;
            switch (element.Tag.ToString())
            {
                case "Bold":
                    viewModel.IsBold = !viewModel.IsBold;
                    viewModel.FontWeight = viewModel.IsBold ? FontWeights.Bold : FontWeights.Normal;
                    break;
                case "Italic":
                    viewModel.IsItalic = !viewModel.IsItalic;
                    viewModel.FontStyle = viewModel.IsItalic ? FontStyles.Italic : FontStyles.Normal;
                    break;
                case "Underline":
                    viewModel.IsUnderline = !viewModel.IsUnderline;
                    viewModel.TextDecoration = viewModel.IsUnderline ? TextDecorations.Underline : null;
                    break;
            }
        }

        private void RefreshData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = ((sender as Border).Tag as string)!;
            manageViewModel.SelectedType = tag;
            manageViewModel.SearchString = null;
            manageViewModel.RefreshMedias();
        }

        private void SelectDevicesAll_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var mediaDevicesViewModel = checkbox.DataContext as MediaDevicesViewModel;
            if (checkbox.IsChecked.GetValueOrDefault())
            {
                foreach (var item in mediaDevicesViewModel.Devices)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (var item in mediaDevicesViewModel.Devices)
                {
                    item.IsSelected = false;
                }
            }
        }
    }

    public class ComponentDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dialogBox = FindDialog(container);
            if(item is BaseComponentViewModel viewModel)
            {
                var resourceKey = $"{viewModel.Type}Component";
                return dialogBox.FindResource(resourceKey) as DataTemplate;
            }

            return null;
        }

        private MediaEdit FindDialog(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                if (parentObject is MediaEdit)
                {
                    return parentObject as MediaEdit;
                }

                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null; // 如果没有找到Canvas，则返回null
        }
    }
}
