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
    public partial class MediaEdit : FrameControl
    {
        private IFileService fileService;
        private IDeviceService deviceService;
        //private bool isDragging = false;
        //private Point startPoint;
        //private FrameworkElement selectedElement;        

        public MediaEdit(MediaViewModel currentMedia, UserViewModel currentUser, bool showNavigation)
        {
            InitializeComponent();
            deviceService = new DeviceService();
            fileService = new FileServiceLocal();
            MediaConfig? config = null;
            if (Directory.Exists(System.IO.Path.Combine(Helpers.Constants.OutPath, currentMedia.Name)))
            {
                config = fileService.ReadFileContent<MediaConfig>(System.IO.Path.Combine(Helpers.Constants.OutPath, currentMedia.Name), Helpers.Constants.ConfigFileName, new MediaTypeConverter());
                if (config != null)
                {
                    config.Width = string.IsNullOrEmpty(currentMedia.Width) ? 0 : double.Parse(currentMedia.Width);
                    config.Height = string.IsNullOrEmpty(currentMedia.Height) ? 0 : double.Parse(currentMedia.Height);
                    config.Name = currentMedia.Name;
                    config.Ratio = MainCanvas.Width / double.Parse(currentMedia.Width);
                }
            }

            config ??= new MediaConfig
            {
                Id = currentMedia.Id,
                Name = currentMedia.Name,
                Width = string.IsNullOrEmpty(currentMedia.Width) ? 0 : double.Parse(currentMedia.Width),
                Height = string.IsNullOrEmpty(currentMedia.Height) ? 0 : double.Parse(currentMedia.Height),
                Ratio = MainCanvas.Width / double.Parse(currentMedia.Width),
                Pages = new List<MediaPage>()
            };

            var configViewModel = new MediaConfigViewModel(config);
            var viewModel = new MediaEditViewModel(currentMedia, currentUser, configViewModel);
            viewModel.ShowNavigation = showNavigation;
            DataContext = viewModel;

            this.Loaded += MediaEdit_Loaded;
        }

        private void MediaEdit_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as MediaEditViewModel)!;
            LoadCanvasComponents(viewModel);
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

        //private Image CreateTextElement(TextComponentViewModel viewModel)
        //{            
        //    FormattedText formattedText = new FormattedText(
        //            viewModel.Source,
        //            CultureInfo.CurrentCulture,
        //            FlowDirection.LeftToRight,
        //            new Typeface("Arial"),
        //            20,
        //            Brushes.White,1.25);

        //    // 创建一个 DrawingVisual 对象
        //    DrawingVisual drawingVisual = new DrawingVisual();
        //    DrawingContext drawingContext = drawingVisual.RenderOpen();

        //    // 绘制格式化文本
        //    drawingContext.DrawText(formattedText, new Point(10, 10));
        //    drawingContext.Close();

        //    // 创建一个 DrawingImage 对象
        //    DrawingImage drawingImage = new DrawingImage(drawingVisual.Drawing);

        //    // 创建一个 Image 对象并将其添加到 Canvas
        //    Image result = new Image()
        //    {
        //        Source = drawingImage,
        //        Width = viewModel.Width,
        //        Height = viewModel.Height,
        //        DataContext = viewModel
        //    };

        //    var widthBinding = new Binding("Width")
        //    {
        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        //        Mode = BindingMode.TwoWay
        //    };

        //    var heightBinding = new Binding("Height")
        //    {
        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        //        Mode = BindingMode.TwoWay
        //    };

        //    result.SetBinding(WidthProperty, widthBinding);
        //    result.SetBinding(HeightProperty, heightBinding);

        //    // 设置 Image 的 Canvas 定位属性
        //    Canvas.SetLeft(result, viewModel.Left);
        //    Canvas.SetTop(result, viewModel.Top);

        //    // 添加鼠标事件处理
        //    result.MouseLeftButtonDown += Element_MouseLeftButtonDown;
        //    result.MouseLeftButtonUp += Element_MouseLeftButtonUp;
        //    result.MouseMove += Element_MouseMove;
        //    result.MouseWheel += Element_MouseWheel;

        //    return result;
        //}

        private void PackIcon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (DataContext as MediaEditViewModel)!;
            viewModel.IsEditingName = true;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var viewModel = (DataContext as MediaEditViewModel)!;
                viewModel.IsEditingName = false;
            }
        }

        private void btnBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (DataContext as MediaEditViewModel)!;
            if (viewModel.IsEditingName)
            {
                MessageBox.Show("请先保存节目名");
                return;
            }

            if (viewModel.ShowNavigation)
            {
                var content = new MediaManage(viewModel.CurrentUser, true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
            else
            {
                var userControllers = new UserControllers(viewModel.CurrentUser!);
                (App.Current.MainWindow as MainWindow).GoCotent(userControllers, 2);
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
                var manageViewModel = (DataContext as MediaEditViewModel)!;
                var pageViewModel = manageViewModel.SelectedPage;
                var currentComponent = pageViewModel!.Components.FirstOrDefault(c => c!.IsSelected);
                if (currentComponent == null) 
                {
                    MessageBox.Show("请先选择组件！");
                    return;
                }

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string filePath = files.FirstOrDefault()!;
                string fileName = System.IO.Path.GetFileName(filePath);
                string localFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, manageViewModel.MediaConfig.Name, pageViewModel.Name, currentComponent.Name);

                if (IsImageFile(filePath))
                {
                    if (currentComponent.Type != MediaType.Image.ToString())
                    {
                        MessageBox.Show("图片组件只能包含图片格式文件！");
                        return;
                    }

                    var uploadPath = System.IO.Path.Combine(localFolder, fileName);
                    if (!File.Exists(uploadPath))
                    {
                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        uploadPath = fileService.SaveFileContent(localFolder, fileName, fileStream);
                    }

                    var viewModel = (currentComponent as ImageComponentViewModel)!;
                    viewModel.Left = e.GetPosition(MainCanvas).X;
                    viewModel.Top = e.GetPosition(MainCanvas).Y;
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
                        MessageBox.Show("视频组件只能包含视频格式文件！");
                        return;
                    }

                    var uploadPath = System.IO.Path.Combine(localFolder, fileName);
                    if (!File.Exists(uploadPath))
                    {
                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        uploadPath = fileService.SaveFileContent(localFolder, fileName, fileStream);
                    }

                    var viewModel = currentComponent;
                    viewModel.Left = e.GetPosition(MainCanvas).X;
                    viewModel.Top = e.GetPosition(MainCanvas).Y;
                    viewModel.Width = 300;
                    viewModel.Height = 200;
                    viewModel.Source = uploadPath;
                    viewModel.FileName = fileName;
                    viewModel.IsShowInfo = true;

                    viewModel.DrawContentCommand.Execute(MainCanvas);
                    manageViewModel.SelectedElement = viewModel.FrameworkElement;
                }
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as MediaEditViewModel)!;

            if (viewModel.MediaConfig.Pages.Count == 0)
            {
                MessageBox.Show("请先创建页面!");
                return;
            }

            var configModel = viewModel.MediaConfig.ToModel();

            var configContent = JsonConvert.SerializeObject(configModel);

            var mediaResourcePath = System.IO.Path.Combine(Helpers.Constants.OutPath, configModel.Name);

            fileService.SaveFileContent(mediaResourcePath, Helpers.Constants.ConfigFileName, configContent);

            if (viewModel.CurrentMedia.Id == 0)
            {
                SQLite.InserTable(viewModel.CurrentMedia.ToModel());
            }
            else
            {
                SQLite.UpdateTable(viewModel.CurrentMedia.ToModel());
            }
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            //发布时将当前节目的所有资源打包，发布到机顶盒
            var manageViewModel = (DataContext as MediaEditViewModel)!;

            var sourceDic = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentMedia.Name);

            if(!File.Exists(System.IO.Path.Combine(sourceDic, Helpers.Constants.ConfigFileName)))
            {
                MessageBox.Show("请先保存节目!");
                return;
            }

            var desZipFilePath = System.IO.Path.Combine(Helpers.Constants.OutPath, manageViewModel.CurrentMedia.Name + ".zip");
            fileService.CreatZip(sourceDic, desZipFilePath);

            var fileSize = (double)new FileInfo(desZipFilePath).Length / 1024 /1024;

            manageViewModel.CurrentMedia.Size = fileSize.ToString("F2") + "MB";
            SQLite.UpdateTable(manageViewModel.CurrentMedia.ToModel());

            var devices = deviceService.GetDevices().GetAwaiter().GetResult().ToList();
            foreach (var device in devices)
            {
                if (device.MediaIds.Contains(manageViewModel.CurrentMedia.Id))
                {
                    device.IsSelected = true;
                }
            }

            var viewModel = new MediaDevicesViewModel(manageViewModel.CurrentMedia, devices);
            //viewModel.PublishCommand.Execute(null);
            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnPublishSave_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;

            var viewModel = ((sender as Button).DataContext as MediaDevicesViewModel)!;
            viewModel.PublishCommand.Execute(null);

            var screenCount = viewModel.Devices.Select(c => c.IsSelected).Count();
            manageViewModel.CurrentMedia.ScreensCount = screenCount;
            SQLite.UpdateTable(manageViewModel.CurrentMedia.ToModel());

            DialogHost.CloseDialogCommand.Execute(null, null);

            if (manageViewModel.ShowNavigation)
            {
                var content = new MediaManage(manageViewModel.CurrentUser, true);
                (App.Current.MainWindow as MainWindow).GoCotent(content, 2);
            }
            else
            {
                var userControllers = new UserControllers(manageViewModel.CurrentUser!);
                (App.Current.MainWindow as MainWindow).GoCotent(userControllers, 2);
            }
        }

        private void SelectDay_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var schedulers = manageViewModel.SelectedPage.Schedulers;

            var newScheduler = new SchedulerViewModel(schedulers.Count() + 1, string.Empty, string.Empty, new List<int>());
            schedulers.Add(newScheduler);
        }

        private void DeleteScheduler_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var schedulers = manageViewModel.SelectedPage.Schedulers;

            if (schedulers.Count > 1)
            {
                var viewModel = ((sender as PackIcon).DataContext as SchedulerViewModel)!;
                manageViewModel.SelectedPage.Schedulers.Remove(viewModel);
            }
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            if (manageViewModel.SelectedComponent != null && manageViewModel.SelectedElement != null)
            {
                Canvas.SetLeft(manageViewModel.SelectedElement, manageViewModel.SelectedComponent.Left);
                Canvas.SetTop(manageViewModel.SelectedElement, manageViewModel.SelectedComponent.Top);
            }
        }

        private void btnPageAdd_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var viewModel = new MediaPageViewModel(new MediaPage
            {
                Id = manageViewModel.MediaConfig.Pages.Count > 0 ? manageViewModel.MediaConfig.Pages.Select(c => c.Id).Max() + 1 : 1,
                Order = manageViewModel.MediaConfig.Pages.Count > 0 ? manageViewModel.MediaConfig.Pages.Select(c => c.Order).Max() + 1 : 1,
                Schedulers = new List<Scheduler> { new Scheduler { Id = 1, ScheduleDays = new List<int>() } },
                Components = new List<BaseComponent>()
            });

            manageViewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnPageSave_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var viewModel = ((sender as Button).DataContext as MediaPageViewModel)!;
            if (string.IsNullOrEmpty(viewModel.Name))
            {
                MessageBox.Show("请输入页面名称！");
                return;
            }

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

        private void btnPageDelete_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var viewModel = ((sender as StackPanel).DataContext as MediaPageViewModel)!;
            manageViewModel.MediaConfig.Pages.First(c => c.IsSelected).IsSelected = false;
            manageViewModel.SelectedPage = viewModel;
            manageViewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
            viewModel.IsSelected = true;
            LoadCanvasComponents(manageViewModel);
        }

        private void SelectComponent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var viewModel = ((sender as DockPanel).DataContext as BaseComponentViewModel)!;
            manageViewModel.SelectedElement = null;
            SwitchComponent(viewModel);
        }

        private void btnComponentDelete_Click(object sender, RoutedEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;

            var radioButton = sender as RadioButton;
            if (radioButton.IsChecked.HasValue && radioButton.IsChecked.Value)
            {
                switch(manageViewModel.SelectedComponent.Type)
                {
                    case "Video":
                        (manageViewModel.SelectedComponent as VideoComponentViewModel).PlayMode = radioButton.Content?.ToString();
                        break;
                    case "Text":
                        (manageViewModel.SelectedComponent as TextComponentViewModel).PlayMode = radioButton.Content?.ToString();
                        break;

                }
            }
        }

        private void CreateComponent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            var createBtn = (sender as StackPanel)!;

            if(manageViewModel.SelectedPage == null)
            {
                MessageBox.Show("请先创建一个页面");
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
                            Name = $"视频{maxId + 1}",
                            ZIndex = 1,
                            Type = (MediaType)type,
                        });
                        break;
                    case MediaType.Image:
                        manageViewModel.SelectedComponent = new ImageComponentViewModel(new ImageComponent
                        {
                            Id = maxId + 1,
                            Name = $"图片{maxId + 1}",
                            ZIndex = 1,
                            Type = (MediaType)type,
                        });
                        break;
                    case MediaType.Text:
                        manageViewModel.SelectedComponent = new TextComponentViewModel(new TextComponent
                        {
                            Id = maxId + 1,
                            Name = $"文本{maxId + 1}",
                            ZIndex = 1,
                            Type = (MediaType)type,
                            Source = "空文本"
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            if (manageViewModel.MediaConfig.Pages.Count == 0)
            {
                MessageBox.Show("请先创建页面!");
                return;
            }

            var previewWindow = new MediaPreview(manageViewModel);
            previewWindow.Show();
        }

        private void ClearContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is PackIcon packIcon)
            {
                var manageViewModel = (DataContext as MediaEditViewModel)!;
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
            var manageViewModel = (DataContext as MediaEditViewModel)!;
            if (manageViewModel.SelectedComponent != null && manageViewModel.SelectedElement != null)
            {
                Canvas.SetZIndex(manageViewModel.SelectedElement, manageViewModel.SelectedComponent.ZIndex);
            }

        }

        private void NameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as MediaEditViewModel)!;
            viewModel.IsEditingName = false;
        }
    }
}
