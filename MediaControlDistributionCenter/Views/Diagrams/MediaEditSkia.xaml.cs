using MaterialDesignThemes.Wpf;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using Serilog;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using SqlSugar;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    public partial class MediaEditSkia : UserControl
    {
        private SkiaEditorSurface? _surface;
        private MediaEditViewModel? _viewModel;
        private IServiceProvider? _serviceProvider;
        private IFileService? _fileService;

        public MediaEditSkia()
        {
            InitializeComponent();
            this.Unloaded += MediaEditSkia_Unloaded;
            InitializeEngine();
        }

        public MediaEditSkia(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            this.Unloaded += MediaEditSkia_Unloaded;
            InitializeEngine();
        }

        private void InitializeEngine()
        {
            CompositionTarget.Rendering += OnRendering;
            this.PreviewKeyDown += OnPreviewKeyDown;
            SkCanvas.PaintSurface += SkCanvas_PaintSurface;
        }

        public void SetViewModel(MediaEditViewModel viewModel)
        {
            _viewModel = viewModel;
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            var key = connectionMode.Mode == "Local" || string.IsNullOrEmpty(connectionMode.ServiceUri) ? "Local" : "Remote";
            _fileService = App.ServicesProvider.GetRequiredKeyedService<IFileService>(key);

            _surface = new SkiaEditorSurface(_serviceProvider);
            _surface.SetViewModel(viewModel);
            viewModel.Surface = _surface;
            viewModel.CanvasRatio = 1;
            viewModel.SelectedComponent = null;
            DataContext = viewModel;

            this.Loaded += (s, e) =>
            {
                if (_viewModel?.CurrentMedia != null)
                {
                    Dispatcher.Invoke(async () =>
                    {
                        try
                        {
                            await _viewModel.LoadData();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "LoadData failed");
                        }
                        LoadComponents();
                    });
                }
            };
        }

        private void LoadComponents()
        {
            try
            {
                if (_viewModel?.SelectedPage == null) return;
                _viewModel.Surface?.LoadComponents(_viewModel.SelectedPage.Components);
                SkCanvas.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load components");
            }
        }

        public byte[]? CaptureSnapshot()
        {
            return _viewModel?.Surface?.CaptureSnapshot();
        }

        private void MediaEditSkia_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.Surface != null)
            {
                _viewModel.Surface.Clear();
                _viewModel.Surface.Dispose();
                _viewModel.Surface = null;
            }
            _viewModel?.DisposeCommand.Execute(null);
            CompositionTarget.Rendering -= OnRendering;
            this.PreviewKeyDown -= OnPreviewKeyDown;
            SkCanvas.PaintSurface -= SkCanvas_PaintSurface;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11 && _surface != null)
            {
                _surface.Controller.ToggleFps();
                e.Handled = true;
            }
        }

        private void SkCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (_surface == null) return;
            _surface.Render(e.Surface.Canvas, e.Info.Width, e.Info.Height);
            _surface.Controller.FpsCounter.Draw(e.Surface.Canvas, (float)SkCanvas.ActualWidth, _surface.Controller.RenderEngine.Statistics);
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_surface == null) return;
            _surface.UpdateDeltaTime();
            _surface.Controller.FpsCounter.Update(_surface.Controller.LastDeltaSeconds);

            if (_surface.NeedsRedraw)
            {
                SkCanvas.InvalidateVisual();
            }
        }

        private SKPoint GetCanvasPosition(MouseEventArgs e)
        {
            var pos = e.GetPosition(SkCanvas);
            return new SKPoint((float)pos.X, (float)pos.Y);
        }

        private void SkCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_surface == null) return;
            var pos = GetCanvasPosition(e);
            _surface.MouseHandler.OnMouseDown(pos);
            _surface.UpdateSelection();
            _surface.Controller.RenderEngine.IsInteracting = true;
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_surface == null) return;
            var pos = GetCanvasPosition(e);
            _surface.MouseHandler.OnMouseMove(pos);

            var cursor = _surface.MouseHandler.GetCursor(pos);
            SkCanvas.Cursor = cursor switch
            {
                Rendering.CursorType.SizeAll => Cursors.SizeAll,
                Rendering.CursorType.SizeWE => Cursors.SizeWE,
                Rendering.CursorType.SizeNS => Cursors.SizeNS,
                Rendering.CursorType.SizeNWSE => Cursors.SizeNWSE,
                Rendering.CursorType.SizeNESW => Cursors.SizeNESW,
                _ => Cursors.Arrow,
            };

            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_surface == null) return;
            _surface.MouseHandler.OnMouseUp();
            _surface.Controller.RenderEngine.IsInteracting = false;
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_surface == null) return;
            _surface.MouseHandler.OnMouseWheel(e.Delta);
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void SkCanvas_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void SkCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && _viewModel != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string filePath = files.FirstOrDefault()!;
                var pos = e.GetPosition(SkCanvas);
                UpdateFileComponent(filePath, pos.X, pos.Y);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void btnBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            if (_viewModel == null || _serviceProvider == null) return;
            var mainWindow = App.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            if (_viewModel.ShowNavigation)
            {
                var content = _serviceProvider.GetRequiredService<MediaManage>();
                mainWindow.GoContent(content, 2);
            }
            else
            {
                var content = _serviceProvider.GetRequiredService<UserControllers>();
                mainWindow.GoContent(content, 2);
            }
        }

        private bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            string ext = Path.GetExtension(filePath);
            return imageExtensions.Contains(ext);
        }

        private bool IsVideoFile(string filePath)
        {
            string[] imageExtensions = { ".mp4", ".mp3", ".avi", ".wmv", ".mkv" };
            string ext = Path.GetExtension(filePath);
            return imageExtensions.Contains(ext);
        }

        private bool IsWordFile(string filePath)
        {
            string[] extensions = { ".pdf", ".doc", ".docx", ".pptx" };
            string ext = Path.GetExtension(filePath);
            return extensions.Contains(ext);
        }

        private void UpdateFileComponent(string filePath, double left = 0, double top = 0)
        {
            if (_viewModel == null || _fileService == null) return;
            var pageViewModel = _viewModel.SelectedPage;
            var currentComponent = pageViewModel!.Components.FirstOrDefault(c => c!.IsSelected);
            if (currentComponent == null)
            {
                _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_147");
                _viewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            string fileName = Path.GetFileName(filePath);
            string localFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, _viewModel.CurrentUser.Account, _viewModel.CurrentMedia.Name, pageViewModel.Name, currentComponent.Name);

            if (IsImageFile(filePath))
            {
                if (currentComponent.Type != MediaType.Image.ToString())
                {
                    _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_148");
                    _viewModel.ShowConfirmDialogCommand.Execute(null);
                    return;
                }

                var uploadPath = Path.Combine(localFolder, fileName);
                if (!File.Exists(uploadPath))
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    uploadPath = _fileService.SaveFileContent(localFolder, fileName, fileStream);
                }

                var viewModel = (currentComponent as ImageComponentViewModel)!;
                viewModel.Left = left;
                viewModel.Top = top;
                viewModel.Width = Math.Min(double.Parse(_viewModel.CurrentMedia.Width), 300 / viewModel.Ratio / _viewModel.CanvasRatio);
                viewModel.Height = Math.Min(double.Parse(_viewModel.CurrentMedia.Height), 200 / viewModel.Ratio / _viewModel.CanvasRatio);
                viewModel.Source = uploadPath;
                viewModel.FileName = fileName;
                viewModel.IsShowInfo = true;

                _surface?.AddComponent(viewModel);
            }
            else if (IsVideoFile(filePath))
            {
                if (currentComponent.Type != MediaType.Video.ToString())
                {
                    _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_149");
                    _viewModel.ShowConfirmDialogCommand.Execute(null);
                    return;
                }

                var uploadPath = Path.Combine(localFolder, fileName);
                if (!File.Exists(uploadPath))
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    uploadPath = _fileService.SaveFileContent(localFolder, fileName, fileStream);
                }

                var viewModel = currentComponent;
                viewModel.Left = left;
                viewModel.Top = top;
                viewModel.Width = Math.Min(double.Parse(_viewModel.CurrentMedia.Width), 300 / viewModel.Ratio / _viewModel.CanvasRatio);
                viewModel.Height = Math.Min(double.Parse(_viewModel.CurrentMedia.Height), 200 / viewModel.Ratio / _viewModel.CanvasRatio);
                viewModel.Source = uploadPath;
                viewModel.FileName = fileName;
                viewModel.IsShowInfo = true;

                _surface?.AddComponent(viewModel);
            }
            else if (IsWordFile(filePath))
            {
                if (currentComponent.Type != MediaType.Word.ToString())
                {
                    _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_195");
                    _viewModel.ShowConfirmDialogCommand.Execute(null);
                    return;
                }

                var uploadPath = Path.Combine(localFolder, fileName);
                if (!File.Exists(uploadPath))
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    uploadPath = _fileService.SaveFileContent(localFolder, fileName, fileStream);
                }

                var viewModel = currentComponent;
                viewModel.Left = left;
                viewModel.Top = top;
                viewModel.Width = Math.Min(double.Parse(_viewModel.CurrentMedia.Width), 229 / viewModel.Ratio / _viewModel.CanvasRatio);
                viewModel.Height = Math.Min(double.Parse(_viewModel.CurrentMedia.Height), 329 / viewModel.Ratio / _viewModel.CanvasRatio);
                viewModel.Source = uploadPath;
                viewModel.FileName = fileName;
                viewModel.IsShowInfo = true;

                _surface?.AddComponent(viewModel);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_viewModel.MediaConfig.Pages.Count == 0)
            {
                _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_150");
                _viewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            SaveContent();
            _viewModel.CurrentMedia.ShowConfirmDialogCommand.Execute(null);
        }

        private void SaveContent()
        {
            if (_viewModel == null || _fileService == null) return;
            _viewModel.CaptureCommand.Execute(null);

            var configModel = _viewModel.MediaConfig.ToModel();
            var configContent = JsonConvert.SerializeObject(configModel);
            var mediaResourcePath = Path.Combine(Helpers.Constants.OutPath, _viewModel.CurrentUser.Account, configModel.Program.Name);

            _fileService.SaveFileContent(mediaResourcePath, Helpers.Constants.ConfigFileName, configContent);

            foreach (var deletePage in _viewModel.MediaConfig.Pages.Where(c => c.IsDeleted))
            {
                _fileService.DeleteResourcePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, _viewModel.CurrentUser.Account, _viewModel.CurrentMedia.Name, deletePage.Name));
            }

            foreach (var deletePage in _viewModel.MediaConfig.Pages.Where(c => c.Components.Any(c => c.IsDeleted)))
            {
                var deleteCompos = deletePage.Components.Where(c => c.IsDeleted);
                foreach (var compo in deleteCompos)
                {
                    var componentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, _viewModel.CurrentUser.Account, _viewModel.CurrentMedia.Name, deletePage.Name, compo.Name);
                    _fileService.DeleteResourcePath(componentPath);
                }
            }
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null || _fileService == null) return;
            SaveContent();
            var sourceDic = Path.Combine(Helpers.Constants.OutPath, _viewModel.CurrentUser.Account, _viewModel.CurrentMedia.Name);

            var desZipFilePath = Path.Combine(Helpers.Constants.OutPath, _viewModel.CurrentUser.Account, _viewModel.CurrentMedia.Name + ".zip");
            _fileService.CreateZip(sourceDic, desZipFilePath);

            _viewModel.CurrentMedia.Size = new FileInfo(desZipFilePath).Length;
            _viewModel.CurrentMedia.SizeText = Utility.GetSizeText(_viewModel.CurrentMedia.Size);
            _viewModel.SaveCommand.Execute(null);

            if (_serviceProvider == null) return;
            var vm = _serviceProvider.GetRequiredService<MediaDevicesViewModel>();
            vm.CurrentMedia = _viewModel.CurrentMedia;
            var dialogBox = _serviceProvider.GetRequiredService<MediaPublishDialog>();
            _viewModel.ShowDialogContentCommand.Execute(dialogBox);
        }

        private void SelectDay_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var dayViewModel = ((sender as Button).DataContext as SchedulerDayViewModel)!;
            if (dayViewModel.IsSelected)
            {
                dayViewModel.IsSelected = false;
                _viewModel.SelectedPage.Schedulers.First(c => c.Id == dayViewModel.SchedulerId).ScheduleDays.Remove(dayViewModel.Id);
            }
            else
            {
                dayViewModel.IsSelected = true;
                _viewModel.SelectedPage.Schedulers.First(c => c.Id == dayViewModel.SchedulerId).ScheduleDays.Add(dayViewModel.Id);
            }
        }

        private void AddScheduler_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var schedulers = _viewModel.SelectedPage.Schedulers;
            var newScheduler = new SchedulerViewModel(schedulers.Count() + 1, string.Empty, string.Empty, new List<int>());
            schedulers.Add(newScheduler);
        }

        private void DeleteScheduler_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var schedulers = _viewModel.SelectedPage.Schedulers;
            if (schedulers.Count > 1)
            {
                var viewModel = ((sender as PackIcon).DataContext as SchedulerViewModel)!;
                _viewModel.SelectedPage.Schedulers.Remove(viewModel);
            }
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (_viewModel?.SelectedComponent == null) return;
        }

        private void btnPageSave_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var maxId = _viewModel.MediaConfig.Pages.Count > 0 ? _viewModel.MediaConfig.Pages.Select(c => c.Id).Max() : 0;
            var viewModel = new MediaPageViewModel(new MediaPage
            {
                Id = maxId + 1,
                Order = maxId + 1,
                Type = "normal",
                PlayCount = 1,
                PlayGap = 10,
                AdPlayMode = "perday",
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Page")}{maxId + 1}",
                Schedulers = new List<Scheduler> { new Scheduler { Id = 1, ScheduleDays = new List<int>() { 1, 2, 3, 4, 5, 6, 7 } } },
                Components = new List<BaseComponent>()
            }, _viewModel.CurrentUser.Account);

            if (_viewModel.SelectedPage != null)
            {
                _viewModel.SelectedPage.IsSelected = false;
            }
            _viewModel.SelectedPage = viewModel;
            viewModel.IsSelected = true;
            _viewModel.MediaConfig.Pages.Add(viewModel);
            LoadComponents();
            _viewModel.CaptureCommand.Execute(null);
        }

        private void btnPageDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var currentPage = _viewModel.SelectedPage;
            currentPage.IsSelected = false;
            currentPage.IsDeleted = true;
            _viewModel.SelectedPage = _viewModel.MediaConfig.Pages.FirstOrDefault();
            if (_viewModel.SelectedPage != null)
            {
                _viewModel.SelectedPage.IsSelected = true;
            }
            LoadComponents();
        }

        private void btnPageUpOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_viewModel.MediaConfig.Pages.Where(c => !c.IsDeleted).Count() > 1)
            {
                var prePage = _viewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order < _viewModel.SelectedPage.Order);
                if (prePage != null)
                {
                    var currentOrder = _viewModel.SelectedPage.Order;
                    _viewModel.SelectedPage.Order = prePage.Order;
                    prePage.Order = currentOrder;
                }
                _viewModel.MediaConfig.Pages = new ObservableCollection<MediaPageViewModel>(_viewModel.MediaConfig.Pages.OrderBy(c => c.Order).ToList());
            }
        }

        private void btnPageDownOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_viewModel.MediaConfig.Pages.Where(c => !c.IsDeleted).Count() > 1)
            {
                var nextPage = _viewModel.MediaConfig.Pages.FirstOrDefault(c => c.Order > _viewModel.SelectedPage.Order && !c.IsDeleted);
                if (nextPage != null)
                {
                    var currentOrder = _viewModel.SelectedPage.Order;
                    _viewModel.SelectedPage.Order = nextPage.Order;
                    nextPage.Order = currentOrder;
                }
                _viewModel.MediaConfig.Pages = new ObservableCollection<MediaPageViewModel>(_viewModel.MediaConfig.Pages.OrderBy(c => c.Order).ToList());
            }
        }

        private void SelectPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var viewModel = ((sender as StackPanel).DataContext as MediaPageViewModel)!;
            if (_viewModel.SelectedPage != null)
            {
                _viewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
                _viewModel.SelectedComponent = null;
            }
            _viewModel.MediaConfig.Pages.First(c => c.IsSelected).IsSelected = false;
            _viewModel.SelectedPage = viewModel;
            _viewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
            viewModel.IsSelected = true;
            LoadComponents();
        }

        private void SelectComponent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var viewModel = ((sender as DockPanel).DataContext as BaseComponentViewModel)!;
            SwitchComponent(viewModel);
        }

        private void btnComponentDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_viewModel.SelectedComponent != null)
            {
                var currentCom = _viewModel.SelectedComponent;
                currentCom.IsSelected = false;
                currentCom.IsDeleted = true;
                _viewModel.SelectedComponent = null;
                LoadComponents();
            }
            else
            {
                _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_147");
                _viewModel.ShowConfirmDialogCommand.Execute(null);
            }
        }

        private void btnMediaUpOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var currentMedia = _viewModel.SelectedPage.Components.First(c => c.IsSelected);
            if (_viewModel.SelectedPage.Components.Count > 1)
            {
                var prePage = _viewModel.SelectedPage.Components.OrderByDescending(c => c.ZIndex).FirstOrDefault(c => c.ZIndex < currentMedia.ZIndex);
                if (prePage != null)
                {
                    var currentOrder = currentMedia.ZIndex;
                    currentMedia.ZIndex = prePage.ZIndex;
                    prePage.ZIndex = currentOrder;
                }
                _viewModel.SelectedPage.Components = new ObservableCollection<BaseComponentViewModel>(_viewModel.SelectedPage.Components.OrderBy(c => c.ZIndex).ToList());
                _viewModel.DisposeCommand.Execute(null);
                LoadComponents();
            }
        }

        private void btnMediaDownOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var currentMedia = _viewModel.SelectedPage.Components.First(c => c.IsSelected);
            if (_viewModel.SelectedPage.Components.Count > 1)
            {
                var nextPage = _viewModel.SelectedPage.Components.FirstOrDefault(c => c.ZIndex > currentMedia.ZIndex);
                if (nextPage != null)
                {
                    var currentOrder = currentMedia.ZIndex;
                    currentMedia.ZIndex = nextPage.ZIndex;
                    nextPage.ZIndex = currentOrder;
                }
                _viewModel.SelectedPage.Components = new ObservableCollection<BaseComponentViewModel>(_viewModel.SelectedPage.Components.OrderBy(c => c.ZIndex).ToList());
                _viewModel.DisposeCommand.Execute(null);
                LoadComponents();
            }
        }

        private void SwitchComponent(BaseComponentViewModel viewModel)
        {
            if (_viewModel == null) return;
            if (_viewModel.SelectedComponent != null)
            {
                _viewModel.SelectedComponent.IsSelected = false;
                _viewModel.SelectedComponent = null;
            }

            viewModel.MaxLeft = double.Parse(_viewModel.CurrentMedia.Width) - viewModel.Width;
            viewModel.MaxTop = double.Parse(_viewModel.CurrentMedia.Height) - viewModel.Height;
            _viewModel.SelectedComponent = viewModel;
            _viewModel.SelectedComponent.IsSelected = true;
            _surface?.SelectComponent(viewModel);
            SkCanvas.InvalidateVisual();
        }

        private void PlayModeChanged_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SelectedComponent == null) return;
            var radioButton = sender as RadioButton;
            if (radioButton.IsChecked.HasValue && radioButton.IsChecked.Value)
            {
                switch (_viewModel.SelectedComponent.Type)
                {
                    case "Video":
                        (_viewModel.SelectedComponent as VideoComponentViewModel).PlayMode = radioButton.Tag?.ToString();
                        break;
                    case "Text":
                        (_viewModel.SelectedComponent as TextComponentViewModel).PlayMode = radioButton.Tag?.ToString();
                        break;
                    case "Rss":
                        (_viewModel.SelectedComponent as RssComponentViewModel).PlayMode = radioButton.Tag?.ToString();
                        break;
                }
            }
        }

        private void PageTypeChanged_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var radioButton = sender as RadioButton;
            if (radioButton.IsChecked.HasValue && radioButton.IsChecked.Value)
            {
                _viewModel.SelectedPage.Type = radioButton.Tag?.ToString();
            }
        }

        private void AdPlayModeChanged_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var radioButton = sender as RadioButton;
            if (radioButton.IsChecked.HasValue && radioButton.IsChecked.Value)
            {
                _viewModel.SelectedPage.AdPlayMode = radioButton.Tag?.ToString();
            }
        }

        private void CreateComponent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var createBtn = (sender as StackPanel)!;

            if (_viewModel.SelectedPage == null)
            {
                _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_150");
                _viewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            var maxId = _viewModel.SelectedPage.Components.Count == 0 ? 0 : _viewModel.SelectedPage.Components.Select(c => c!.Id).Max() + 0;

            if (Enum.TryParse(typeof(MediaType), createBtn.Tag.ToString()!, out var type))
            {
                if (_viewModel.SelectedComponent != null)
                {
                    _viewModel.SelectedComponent.IsSelected = false;
                }

                _viewModel.SelectedComponent = _viewModel.CreateComponent((MediaType)type, maxId + 1);

                if (_viewModel.SelectedComponent != null)
                {
                    _viewModel.SelectedComponent.Ratio = _viewModel.MediaConfig.Ratio;
                    _viewModel.SelectedComponent.IsSelected = true;
                    if (!_viewModel.SelectedComponent.IsFile)
                    {
                        _viewModel.PrepareComponentDefaults(_viewModel.SelectedComponent);
                        _surface?.AddComponent(_viewModel.SelectedComponent);
                    }

                    _viewModel.SelectedPage.Components.Add(_viewModel.SelectedComponent);
                }
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_viewModel.MediaConfig.Pages.Count == 0)
            {
                _viewModel.ErrorMessage = (string)FindResource("LanguageKey_Code_ProgramEdit_Tooltip_150");
                _viewModel.ShowConfirmDialogCommand.Execute(null);
                return;
            }

            if (_viewModel.IsPreviewing) return;

            var previewWindow = new MediaPreview(_viewModel);
            previewWindow.Show();
        }

        private void ClearContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            if (sender is CustomIcon packIcon)
            {
                var viewModel = (BaseComponentViewModel)packIcon.DataContext;
                if (viewModel.IsFile)
                {
                    viewModel.Source = null;
                    viewModel.FileName = null;
                    viewModel.IsShowInfo = false;
                    LoadComponents();
                }
                else
                {
                    viewModel.Source = null;
                }
            }
        }

        private void Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (_viewModel?.SelectedComponent == null) return;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SelectedComponent == null) return;
            if (sender is TextBox textBox)
            {
                ((TextComponentViewModel)_viewModel.SelectedComponent).Foreground = (Color)ColorConverter.ConvertFromString(textBox.Text);
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var viewModel = (TextComponentViewModel)_viewModel.SelectedComponent;
            _viewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void btnUpload_Click(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.SelectedComponent == null) return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (_viewModel.SelectedComponent.Type == "Image")
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            }
            else if (_viewModel.SelectedComponent.Type == "Video")
            {
                openFileDialog.Filter = "Video Files|*.mp4;*.mp3;*.avi;*.wmv;*.mkv";
            }
            else if (_viewModel.SelectedComponent.Type == "Word")
            {
                openFileDialog.Filter = "DOC Files|*.pdf; *.doc; *.docx;*.pptx";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                UpdateFileComponent(filePath);
            }
        }

        private void btnPageCapture_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.CaptureCommand.Execute(null);
        }

        private void LeftTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tab && tab.SelectedIndex == 1)
            {
                _viewModel?.CaptureCommand.Execute(null);
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
            _viewModel?.ShowDialogCommand.Execute(viewModel);
        }

        private void CreateComponentFromStore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var element = (Image)sender;
            var viewModel = element.DataContext as MediaViewModel;
            UpdateFileComponent(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Helpers.Constants.OutPath, viewModel.Src));
        }

        private void SelectColorTextColor_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var viewModel = (sender as Button).DataContext as ColorTextComponentViewModel;
            _viewModel.ShowDialogCommand.Execute(viewModel);
        }

        private void SetColorTexFontStyle_Click(object sender, RoutedEventArgs e)
        {
            var element = (Button)sender;
            var viewModel = element.DataContext as ColorTextComponentViewModel;
            switch (element.Tag.ToString())
            {
                case "Bold":
                    viewModel.IsBold = !viewModel.IsBold;
                    viewModel.FontWeight = FontWeights.Bold;
                    break;
                case "Italic":
                    viewModel.IsItalic = !viewModel.IsItalic;
                    viewModel.FontStyle = FontStyles.Italic;
                    break;
                case "Underline":
                    viewModel.IsUnderline = !viewModel.IsUnderline;
                    viewModel.TextDecoration = TextDecorations.Underline;
                    break;
            }
        }

        private void RefreshData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var tag = ((sender as Border).Tag as string)!;
            _viewModel.SelectedType = tag;
            _viewModel.SearchString = null;
            Dispatcher.Invoke(async () =>
            {
                await _viewModel.RefreshMedias();
            });
        }

        private void Screen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (_viewModel?.SelectedComponent == null) return;
        }

        private void btnChangeContentVerticalAlign_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SelectedComponent is not TextComponentViewModel viewModel) return;
            var button = sender as Button;
            viewModel.VerticalContentAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), button.Tag?.ToString());
        }

        private void canvasRatio_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (sender is TextBox textBox && _viewModel.CanvasRatio != Convert.ToDouble(textBox.Text) / 100)
            {
                _viewModel.CanvasRatio = Math.Max(0.2, Math.Min(4, Convert.ToDouble(textBox.Text) / 100));
                _viewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
                LoadComponents();
            }
        }

        private void MinusRatio_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.CanvasRatio = Math.Max(0.2, _viewModel.CanvasRatio - 0.1);
            _viewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
            LoadComponents();
        }

        private void PlusRatio_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.CanvasRatio = Math.Min(4, _viewModel.CanvasRatio + 0.1);
            _viewModel.SelectedPage.Components.ToList().ForEach(c => c!.FrameworkElement = null);
            LoadComponents();
        }

        private void SelectPresetColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            if (_viewModel.SelectedComponent is TextComponentViewModel textComponentViewModel)
            {
                var border = sender as Border;
                var color = border?.Background is SolidColorBrush brush ? brush.Color : Colors.Transparent;
                textComponentViewModel.Background = color;
                _viewModel.CloseDialogCommand.Execute(null);
            }
        }
    }

    public class ComponentDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dialogBox = FindDialog(container);
            if (item is BaseComponentViewModel viewModel)
            {
                var resourceKey = $"{viewModel.Type}Component";
                return dialogBox.FindResource(resourceKey) as DataTemplate;
            }
            return null;
        }

        private MediaEditSkia FindDialog(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            while (parentObject != null)
            {
                if (parentObject is MediaEditSkia)
                {
                    return parentObject as MediaEditSkia;
                }
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }
            return null;
        }
    }
}