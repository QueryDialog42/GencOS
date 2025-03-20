using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Forms;

namespace WPFFrameworkApp
{
    /// <summary>
    /// RTFNote.xaml etkileşim mantığı
    /// </summary>
    public partial class RTFNote : Window
    {
        public string currentDesktopForNote;
        public MainWindow windowForNote;
        private string filter = "RTF Files(*.rtf) | *.rtf";

        public RTFNote()
        {
            InitializeComponent();;
            Show();
        }

        private void NewNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.NewNote_Wanted(windowForNote, currentDesktopForNote);
        }

        private void OpenNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.OpenNote_Wanted(windowForNote, currentDesktopForNote);
        }

        private void SaveNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.RTFSaveNote_Wanted(currentDesktopForNote, this);
        }

        private void SaveAsNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.RTFSaveAsNote_Wanted(windowForNote, currentDesktopForNote, this);
            MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
        }

        private void CopyNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.CopyNote_Wanted(windowForNote, currentDesktopForNote, filter, this);
        }

        private void MoveNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.MoveNote_Wanted(windowForNote, currentDesktopForNote, filter, this);
            MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
        }

        private void DeleteNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.DeleteNote_Wanted(windowForNote, currentDesktopForNote, this);
            MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
        }

        private void MakeBold(object sender, RoutedEventArgs e)
        {
            // Get the current selection
            TextSelection selection = RichNote.Selection;

            // Check if there is any text selected
            if (selection.IsEmpty == false)
            {
                // Create a TextRange from the selection
                TextRange textRange = new TextRange(selection.Start, selection.End);

                // Apply bold formatting
                textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
        }

        private void MakeItalic(object sender, RoutedEventArgs e)
        {
            TextSelection selection = RichNote.Selection;
            if (selection.IsEmpty == false) 
            {
                TextRange textrange = new TextRange(selection.Start, selection.End);
                textrange.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            }
        }

        private void MakeUnderline(object sender, RoutedEventArgs e)
        {
            TextSelection selection = RichNote.Selection;
            if (selection.IsEmpty == false)
            {
                TextRange textrange = new TextRange(selection.Start, selection.End);
                textrange.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
        }
        private void FontDown(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.FontChange_Wanted(RichNote, -1.0);
        }

        private void FontUp(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.FontChange_Wanted(RichNote, 1.0);
        }

        private void ColorChange(object sender, RoutedEventArgs e)
        {
            ColorDialog colordialog = new ColorDialog();
            if (colordialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color selectedcolor = colordialog.Color;

                // Convert to WPF Color
                Color wpfcolor = Color.FromArgb(selectedcolor.A, selectedcolor.R, selectedcolor.G, selectedcolor.B);

                TextSelection selection = RichNote.Selection;
                if (selection.IsEmpty == false)
                {
                    TextRange textRange = new TextRange(selection.Start, selection.End);

                    // Apply the new foreground color to the selected text
                    textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(wpfcolor));
                }
            }
        }
    }
}
