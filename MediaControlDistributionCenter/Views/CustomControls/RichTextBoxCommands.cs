using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class RichTextBoxCommands
    {
        private RichTextBox rtbEditor;
        public RichTextBoxCommands(RichTextBox richTextBox) 
        {
            this.rtbEditor = richTextBox;
        }
        public void SetupCommands()
        {
            // 绑定原生命令
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, Print_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, Undo_Executed, Undo_CanExecute));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, Redo_Executed, Redo_CanExecute));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_CanExecute));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
            rtbEditor.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_CanExecute));

            // 绑定编辑命令
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleBold, ToggleBold_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleItalic, ToggleItalic_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleUnderline, ToggleUnderline_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.AlignLeft, AlignLeft_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.AlignCenter, AlignCenter_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.AlignRight, AlignRight_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.AlignJustify, AlignJustify_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.IncreaseIndentation, IncreaseIndentation_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.DecreaseIndentation, DecreaseIndentation_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleBullets, ToggleBullets_Executed));
            rtbEditor.CommandBindings.Add(new CommandBinding(EditingCommands.ToggleNumbering, ToggleNumbering_Executed));
        }

        #region 文件操作
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "富文本文件 (*.rtf)|*.rtf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);

                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        range.Load(fs, DataFormats.Rtf);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "富文本文件 (*.rtf)|*.rtf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);

                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        range.Save(fs, DataFormats.Rtf);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument document = rtbEditor.Document;
                document.PageHeight = printDialog.PrintableAreaHeight;
                document.PageWidth = printDialog.PrintableAreaWidth;
                document.PagePadding = new Thickness(50);
                document.ColumnGap = 0;
                document.ColumnWidth = printDialog.PrintableAreaWidth;

                IDocumentPaginatorSource paginatorSource = document;
                printDialog.PrintDocument(paginatorSource.DocumentPaginator, "富文本打印");
            }
        }
        #endregion

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
    }
}
