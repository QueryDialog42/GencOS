﻿using System.ComponentModel;
using System.IO;
using System.Windows;

namespace WPFFrameworkApp
{
    /// <summary>
    /// NoteApp.xaml etkileşim mantığı
    /// </summary>
    public partial class TXTNote : Window
    {
        public string currentDesktopForNote;
        public MainWindow windowForNote;
        private string filter = $"Text Files(*{SupportedFiles.TXT})|*{SupportedFiles.TXT}";
        public TXTNote()
        {
            InitializeComponent();
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
            NoteAppLogics.TXTSaveNote_Wanted(currentDesktopForNote, this);
        }

        private void SaveAsNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.TXTSaveAsNote_Wanted(windowForNote, currentDesktopForNote, this);
        }

        private void CopyNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.CopyNote_Wanted(windowForNote, currentDesktopForNote, filter, this);
        }

        private void MoveNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.MoveNote_Wanted(windowForNote, currentDesktopForNote, filter, this);
        }
        private void RenameNote(object sender, RoutedEventArgs e)
        {
            RoutineLogics.RenameFile_Wanted(Path.Combine(currentDesktopForNote, Title), ImagePaths.NADD_IMG);
            RoutineLogics.ReloadDesktop(windowForNote, currentDesktopForNote);
            Close();
        }
        private void DeleteNote(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.DeleteNote_Wanted(windowForNote, currentDesktopForNote, this);
        }

        private void AboutGennotePage_Wanted(object sender, RoutedEventArgs e)
        {
            RoutineLogics.ShowAboutWindow("GenNote", ImagePaths.NOTE_IMG, ImagePaths.NOTE_IMG, Versions.NOTE_VRS, Messages.ABT_DFLT_MSG);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            currentDesktopForNote = null;
            windowForNote = null;
            filter = null;
        }
    }
}
