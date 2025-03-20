using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WPFFrameworkApp
{
    public static class NoteAppLogics
    {
        public static readonly string saveerror = "Save Error";
        public static readonly string readerror = "Read Error";
        public static readonly string copyerror = "Copy Error";
        public static readonly string deleteerror = "Delete Error";
        public static readonly string moveerror = "Move Error";
        public static void NewNote_Wanted(MainWindow windowForNote, string currentDesktopForNote) 
        {
            string[] options = { "TXT", "RTF" };
            switch (QueryDialog.ShowQueryDialog("Choose Note Type", "NoteApp", options, "NoteImages/noteadd.png", "NoteImages/noteadd.png"))
            {
                case 0: CreateNoteAppEnvironment(windowForNote, currentDesktopForNote, new TXTNote()); break;
                case 1: CreateNoteAppEnvironment(windowForNote, currentDesktopForNote, new RTFNote()); break;
            }
        }

        public static void OpenNote_Wanted(MainWindow windowForNote, string currentDesktopForNote)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                InitialDirectory = currentDesktopForNote,
                Filter = "Text Files (*.txt)|*.txt|RTF Files (*.rtf)|*.rtf", // TXT files and RTF files are supported
                Title = "Open Note"
            };

            if (fileDialog.ShowDialog() == true)
            {
                string file = fileDialog.FileName;
                string folder = Path.GetDirectoryName(file);
                if (file.EndsWith(".txt"))
                {
                    TXTNote TXTnoteapp = new TXTNote
                    {
                        currentDesktopForNote = folder,
                        windowForNote = windowForNote // something different needed
                    };
                    try
                    {
                        TXTnoteapp.note.Text = File.ReadAllText(fileDialog.FileName);
                        TXTnoteapp.Title = Path.GetFileName(fileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MainWindow.ErrorMessage("An error occured while reading the file.\n" + ex.Message, readerror);
                    }
                }
                else if (file.EndsWith(".rtf"))
                {
                    RTFNote RTFnoteapp = new RTFNote
                    {
                        currentDesktopForNote = folder,
                        windowForNote = windowForNote
                    };
                    try
                    {
                        RTFnoteapp.Title = Path.GetFileName(fileDialog.FileName);   
                        // Read the RTF file content
                        string rtfContent = File.ReadAllText(fileDialog.FileName);

                        // Create a TextRange to load the RTF content into the RichTextBox
                        TextRange textRange = new TextRange(RTFnoteapp.RichNote.Document.ContentStart, RTFnoteapp.RichNote.Document.ContentEnd);

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            // Write the RTF content to the memory stream
                            using (StreamWriter writer = new StreamWriter(memoryStream))
                            {
                                writer.Write(rtfContent);
                                writer.Flush();
                                memoryStream.Position = 0; // Reset the stream position

                                // Load the RTF content into the RichTextBox
                                textRange.Load(memoryStream, DataFormats.Rtf);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MainWindow.ErrorMessage($"An error occured while reading {file}.\n" + ex.Message, readerror);
                    }
                }
                else
                {
                    MainWindow.ErrorMessage($"{file} is not supported for GencOS.\n.txt\nis supported for now.", "Unsupported File Error");
                }
            }
        }
        
        public static void TXTSaveNote_Wanted(string currentDesktopForNote, TXTNote noteapp)
        {
            string filename = noteapp.Title;
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(currentDesktopForNote, filename)))
                {
                    writer.WriteLine(noteapp.note.Text);
                }
            }
            catch (Exception ex)
            {
                MainWindow.ErrorMessage($"An error occured while saving {filename}\n" + ex.Message, saveerror);
            }
        }

        public static void TXTSaveAsNote_Wanted(MainWindow windowForNote, string currentDesktopForNote, TXTNote noteapp)
        {
            SaveFileDialog filedialog = new SaveFileDialog
            {
                InitialDirectory = currentDesktopForNote,
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Save As"
            };
            if (filedialog.ShowDialog() == true)
            {
                string file = filedialog.FileName;
                try
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        writer.WriteLine(noteapp.note.Text);
                        MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.ErrorMessage($"An error occured while saving {Path.GetFileName(file)}\n" + ex.Message, saveerror);
                }
            }
        }

        public static void RTFSaveNote_Wanted(string currentDesktopForNote, RTFNote noteapp)
        {
            string filename = noteapp.Title;
            try 
            {
                TextRange textrange = new TextRange(noteapp.RichNote.Document.ContentStart, noteapp.RichNote.Document.ContentEnd);
                using (FileStream filestream = new FileStream(Path.Combine(currentDesktopForNote, filename), FileMode.Create))
                {
                    textrange.Save(filestream, DataFormats.Rtf);
                }
            }
            catch (Exception ex)
            {
                MainWindow.ErrorMessage($"An error occured while saving {filename}.\n" + ex.Message, saveerror);
            }
        }

        public static void RTFSaveAsNote_Wanted(MainWindow windowForNote, string currentDesktopForNote, RTFNote noteapp)
        {
            SaveFileDialog filedialog = new SaveFileDialog
            {
                InitialDirectory = currentDesktopForNote,
                Filter = "RTF Files (*.rtf)|*.rtf",
                Title = "Save As"
            };
            if (filedialog.ShowDialog() == true)
            {
                string file = filedialog.FileName;
                try
                {
                    TextRange textrange = new TextRange(noteapp.RichNote.Document.ContentStart, noteapp.RichNote.Document.ContentEnd);
                    using (FileStream filestream = new FileStream(file, FileMode.Create))
                    {
                        textrange.Save(filestream, DataFormats.Rtf);
                    }
                }
                catch(Exception ex)
                {
                    MainWindow.ErrorMessage($"An error occured while saving {Path.GetFileName(file)}.\n" + ex.Message, saveerror);
                }
            }
        }

        public static void CopyNote_Wanted(MainWindow windowForNote, string currentDesktopForNote, string filter, dynamic noteapp)
        {
            string filename = noteapp.Title;
            SaveFileDialog filedialog = new SaveFileDialog
            {
                InitialDirectory = currentDesktopForNote,
                Filter = filter,
                FileName = filename,
                Title = "Copy Note"
            };
            if (filedialog.ShowDialog() == true)
            {
                try
                {
                    File.Copy(Path.Combine(currentDesktopForNote, filename), filedialog.FileName);
                    MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
                    noteapp.Close();
                }
                catch (Exception ex)
                {
                    MainWindow.ErrorMessage($"An error occured while copying the {filename}.\n" + ex.Message, copyerror);
                }
            }
        }

        public static void MoveNote_Wanted(MainWindow windowForNote, string currentDesktopForNote, string filter, dynamic noteapp)
        {
            string filename = noteapp.Title;
            SaveFileDialog filedialog = new SaveFileDialog
            {
                InitialDirectory = currentDesktopForNote,
                Filter = filter,
                FileName = filename,
                Title = "Move Note"
            };
            if (filedialog.ShowDialog() == true)
            {
                try
                {
                    File.Move(Path.Combine(currentDesktopForNote, filename), filedialog.FileName);
                    MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
                    noteapp.Close();
                }
                catch (Exception ex)
                {
                    MainWindow.ErrorMessage($"An error occured while moving the {filename}.\n" + ex.Message, moveerror);
                }
            }
        }

        public static void DeleteNote_Wanted(MainWindow windowForNote, string currentDesktopForNote, dynamic noteapp)
        {
            string filename = noteapp.Title;
            if (MessageBox.Show($"Are you sure you want to delete {filename}?", "Delete Note", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    File.Move(Path.Combine(currentDesktopForNote, filename), Path.Combine(MainWindow.TrashPath, filename));
                    MainWindow.ReloadDesktop(windowForNote, currentDesktopForNote);
                    noteapp.Close();
                }
                catch (Exception ex)
                {
                    MainWindow.ErrorMessage($"An error occured while deleting {filename}.\n" + ex.Message, deleteerror);
                }
            }
        }

        public static void CreateNoteAppEnvironment(MainWindow windowForNote, string currentDesktopForNote, dynamic noteapp)
        {
            noteapp.currentDesktopForNote = currentDesktopForNote;
            noteapp.windowForNote = windowForNote;
            noteapp.save.IsEnabled = false;
            noteapp.copy.IsEnabled = false;
            noteapp.move.IsEnabled = false;
            noteapp.delete.IsEnabled = false;
        }

        public static void FontChange_Wanted(RichTextBox RichNote, double value)
        {
            TextSelection selection = RichNote.Selection;
            if (selection.IsEmpty == false)
            {
                TextRange textrange = new TextRange(selection.Start, selection.End);

                // Get the current font size
                var currentFontSize = textrange.GetPropertyValue(TextElement.FontSizeProperty);

                // Check if the current font size is a valid double
                if (currentFontSize is double currentSize)
                {
                    // Apply the new font size to the selected text
                    textrange.ApplyPropertyValue(TextElement.FontSizeProperty, currentSize + value);
                }
                else
                {
                    // Apply the new font size to the selected text
                    textrange.ApplyPropertyValue(TextElement.FontSizeProperty, 12.0);
                }
            }
        }

        public static void ColorChange_Wanted(RichTextBox RichNote)
        {
            System.Windows.Forms.ColorDialog colordialog = new System.Windows.Forms.ColorDialog();
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