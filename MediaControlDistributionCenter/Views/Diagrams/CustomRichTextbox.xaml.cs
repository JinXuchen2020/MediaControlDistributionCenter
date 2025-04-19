using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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

namespace MediaControlDistributionCenter.Views.Diagrams
{
    /// <summary>
    /// CustomRichTextbox.xaml 的交互逻辑
    /// </summary>
    public partial class CustomRichTextbox : UserControl
    {
        public static readonly RoutedUICommand ToggleStrikethroughCommand = new RoutedUICommand(
            "strikethrough", "ToggleStrikethrough", typeof(CustomRichTextbox));

        public static readonly RoutedUICommand ToggleSuperscriptCommand = new RoutedUICommand(
            "superscript", "ToggleSuperscript", typeof(CustomRichTextbox));

        public static readonly RoutedUICommand ToggleSubscriptCommand = new RoutedUICommand(
            "subscript", "ToggleSubscript", typeof(CustomRichTextbox));

        public ObservableCollection<FontFamily> FontFamilies { get; } = new ObservableCollection<FontFamily>();

        public CustomRichTextbox()
        {
            InitializeComponent();
            SetupCommands();

            foreach (var font in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            {
                FontFamilies.Add(font);
            }

            FontFamilyComboBox.ItemsSource = FontFamilies;
            FontFamilyComboBox.SelectedItem = FontFamilies.FirstOrDefault(f => f.Source == "微软雅黑") ?? FontFamilies.First();
        }

        private void SetupCommands()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (sender, e) => SaveData()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, Undo_Executed, Undo_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, Redo_Executed, Redo_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_CanExecute));

            // 绑定自定义命令
            CommandBindings.Add(new CommandBinding(ToggleStrikethroughCommand, ToggleStrikethrough_Executed));
            CommandBindings.Add(new CommandBinding(ToggleSuperscriptCommand, ToggleSuperscript_Executed));
            CommandBindings.Add(new CommandBinding(ToggleSubscriptCommand, ToggleSubscript_Executed));

            // 绑定编辑命令
            CommandBindings.Add(new CommandBinding(EditingCommands.ToggleBold, ToggleBold_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.ToggleItalic, ToggleItalic_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.ToggleUnderline, ToggleUnderline_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.AlignLeft, AlignLeft_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.AlignCenter, AlignCenter_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.AlignRight, AlignRight_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.AlignJustify, AlignJustify_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.IncreaseIndentation, IncreaseIndentation_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.DecreaseIndentation, DecreaseIndentation_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.ToggleBullets, ToggleBullets_Executed));
            CommandBindings.Add(new CommandBinding(EditingCommands.ToggleNumbering, ToggleNumbering_Executed));
        }

        public void LoadData(string filePath)
        {
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                range.Load(fs, DataFormats.Rtf);
            }
        }

        public void LoadDataContent(string content)
        {
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            range.Text = content;
        }

        public void SaveData()
        {
            var manageViewModel = App.ServicesProvider.GetRequiredService<MediaEditViewModel>();
            var fileDic = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, manageViewModel.CurrentUser.Account, manageViewModel.CurrentMedia.Name, manageViewModel.SelectedPage.Name, manageViewModel.SelectedComponent.Name);
            if (!Directory.Exists(fileDic))
            {
                Directory.CreateDirectory(fileDic);
            }

            var filePath = System.IO.Path.Combine(fileDic, manageViewModel.SelectedComponent.Name + ".rtf");
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                range.Save(fs, DataFormats.Rtf);
            }

            (manageViewModel.SelectedComponent as TextComponentViewModel).RtfFilePath = filePath;

             MaterialDesignThemes.Wpf.DialogHost.Close(Helpers.Constants.DialogHostId);
        }

        private void FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem is FontFamily selectedFont)
            {
                rtbEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, selectedFont);
            }
        }

        // 字号选择
        private void FontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (double.TryParse(FontSizeComboBox.SelectedValue.ToString(), out double size))
            {
                rtbEditor?.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
            }
        }

        // 行间距设置
        private void LineSpacing_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem item && double.TryParse(item.Content.ToString(), out var spacing))
            {
                var paragraph = rtbEditor.Selection.Start.Paragraph;
                if (paragraph != null)
                {
                    paragraph.LineHeight = spacing * paragraph.FontSize;
                }
            }

            //if (sender is Button && e.Source == sender)
            //{
            //    var paragraph = rtbEditor.Selection.Start.Paragraph;
            //    if (paragraph != null)
            //    {
            //        spacing = double.Parse(this.tbLineSpacing.Text);
            //        paragraph.LineHeight = spacing * paragraph.FontSize;
            //    }
            //}
        }

        private void CustomLineSpacing_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            var dialogHost = MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageboxId).GetAwaiter().GetResult();
            if (double.TryParse(dialog.Result, out var spacing))
            {
                var paragraph = rtbEditor.Selection.Start.Paragraph;
                if (paragraph != null)
                {
                    paragraph.LineHeight = spacing * paragraph.FontSize;
                }
            }
        }

        // 字符间距设置
        private void CharacterSpacing_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && int.TryParse(item.Tag.ToString(), out var spacing))
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, new TextDecorationCollection(spacing * 100));
            }
        }

        private void CustomCharacterSpacing_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            Dispatcher.Invoke(async () =>
            {
                var dialogHost = await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageboxId);
                if (dialogHost != null)
                {

                }
                if (int.TryParse(dialog.Result, out var spacing))
                {
                    rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, spacing);
                }
            });
        }

        private void ApplyCharacterSpacing(int spacing)
        {
            // 获取当前选择范围
            rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, spacing);
        }

        // 更新UI显示当前选择文本的格式
        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // 更新字体显示
            var currentFont = rtbEditor.Selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
            FontFamilyComboBox.SelectedItem = currentFont;

            // 更新字号显示
            var currentSize = rtbEditor.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (currentSize is double size)
            {
                FontSizeComboBox.SelectedValue = size.ToString();
            }
        }

        #region 编辑操作
        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (rtbEditor.CanUndo)
            {
                rtbEditor.Undo();
            }
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rtbEditor.CanUndo;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (rtbEditor.CanRedo)
            {
                rtbEditor.Redo();
            }
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rtbEditor.CanRedo;
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Cut();
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !rtbEditor.Selection.IsEmpty;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Copy();
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !rtbEditor.Selection.IsEmpty;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Paste();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }
        #endregion

        #region 文本格式
        private void ToggleBold_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleBold.Execute(null, rtbEditor);
        }

        private void ToggleItalic_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleItalic.Execute(null, rtbEditor);
        }

        private void ToggleUnderline_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleUnderline.Execute(null, rtbEditor);
        }

        private void ToggleStrikethrough_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextDecorationCollection decorations = TextDecorations.Strikethrough;
            if (rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) == decorations)
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
            else
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, decorations);
            }
        }

        private void ChangeFontColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(
                    colorDialog.Color.A,
                    colorDialog.Color.R,
                    colorDialog.Color.G,
                    colorDialog.Color.B);

                rtbEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
        }

        private void ChangeHighlightColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(
                    colorDialog.Color.A,
                    colorDialog.Color.R,
                    colorDialog.Color.G,
                    colorDialog.Color.B);

                rtbEditor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));
            }
        }
        #endregion

        #region 段落格式
        private void AlignLeft_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.AlignLeft.Execute(null, rtbEditor);
        }

        private void AlignCenter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.AlignCenter.Execute(null, rtbEditor);
        }

        private void AlignRight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.AlignRight.Execute(null, rtbEditor);
        }

        private void AlignJustify_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.AlignJustify.Execute(null, rtbEditor);
        }

        private void IncreaseIndentation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.IncreaseIndentation.Execute(null, rtbEditor);
        }

        private void DecreaseIndentation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.DecreaseIndentation.Execute(null, rtbEditor);
        }

        private void ToggleBullets_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleBullets.Execute(null, rtbEditor);
        }

        private void ToggleNumbering_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleNumbering.Execute(null, rtbEditor);
        }
        #endregion

        #region 上下标
        private void ToggleSuperscript_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Superscript);
        }

        private void ToggleSubscript_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Subscript);
        }
        #endregion

        #region 插入功能
        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "图片文件 (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                    Image image = new Image { Source = bitmap };

                    // 限制图片大小
                    if (bitmap.PixelWidth > 500 || bitmap.PixelHeight > 500)
                    {
                        image.Width = 500;
                        image.Height = 500;
                    }

                    // 将图片插入到当前光标位置
                    InlineUIContainer container = new InlineUIContainer(image, rtbEditor.CaretPosition);
                    (rtbEditor.Document.Blocks.FirstBlock as Paragraph).Inlines.Add(container);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"插入图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void InsertHyperlink_Click(object sender, RoutedEventArgs e)
        {
            TextRange selection = rtbEditor.Selection;
            if (!selection.IsEmpty)
            {
                string url = Microsoft.VisualBasic.Interaction.InputBox("请输入链接地址:", "插入超链接", "https://");
                if (!string.IsNullOrEmpty(url))
                {
                    Hyperlink hyperlink = new Hyperlink(selection.Start, selection.End)
                    {
                        NavigateUri = new Uri(url),
                        ToolTip = url
                    };

                    // 设置Material Design样式
                    hyperlink.Style = (Style)FindResource("MaterialDesignHyperlink");
                }
            }
            else
            {
                MessageBox.Show("请先选择要添加链接的文本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void InsertTable_Click(object sender, RoutedEventArgs e)
        {
            Table table = new Table();
            table.CellSpacing = 10;
            table.Style = (Style)FindResource("MaterialDesignDataGrid");

            // 添加3行3列表格
            for (int i = 0; i < 3; i++)
            {
                table.RowGroups.Add(new TableRowGroup());
                table.RowGroups[0].Rows.Add(new TableRow());

                for (int j = 0; j < 3; j++)
                {
                    table.RowGroups[0].Rows[i].Cells.Add(new TableCell(new Paragraph(new Run($"单元格 {i + 1}-{j + 1}"))));
                }
            }

            // 插入到文档
            rtbEditor.Document.Blocks.Add(table);
        }
        #endregion
    }
}

