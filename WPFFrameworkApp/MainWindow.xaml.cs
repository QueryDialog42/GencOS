﻿using System;
using System.IO;
using System.Windows;

namespace WPFFrameworkApp
{
    public partial class MainWindow : Window
    {
        public static string CDesktopPath; // C_DESKTOP folder path
        public static string TempPath; // Temporary path to store currentDesktop path
        public static string MusicAppPath;
        public static string TrashPath;
        public string currentDesktop; // path to unique desktop
        public MainWindow()
        {
            InitializeComponent();
            if (TempPath == null) CheckConfig();
            currentDesktop = TempPath.Trim();
            CDesktopPath = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Configs.CPATH)).Trim();
            MusicAppPath = Path.Combine(CDesktopPath, HiddenFolders.HAUD_FOL);
            TrashPath = Path.Combine(CDesktopPath, HiddenFolders.HTRSH_FOL);
            if (currentDesktop != null) RoutineLogics.ReloadDesktop(this, currentDesktop);
        }
        private void CheckConfig()
        {
            string ConfigFileText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Configs.CPATH); //Home Directory\CDesktop.txt
            if (File.Exists(ConfigFileText) == false)
            {
                if (MessageBox.Show($"{Configs.CPATH} could not find. Resetting path needed.", "Path could not find", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    TempPath = ConfigurePath(ConfigFileText);
                else Environment.Exit(0);
            }
            else
            {
                try
                {
                    TempPath = File.ReadAllText(ConfigFileText);
                    if (Directory.Exists(TempPath) == false)
                    {
                        if (MessageBox.Show($"Path to {Configs.CDESK} is corrupted or does not exists.\nPlease reset the desktop path", "Incorrect path", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                            TempPath = ConfigurePath(ConfigFileText);
                        else Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    RoutineLogics.ErrorMessage(Errors.READ_ERR, Errors.READ_ERR_MSG, "the path from ", Configs.CPATH, "\n", ex.Message);
                    Environment.Exit(0);
                }
            }
        }

        private static void ImportFile(MainWindow window, string desktopPath)
        {
            RoutineLogics.MoveAnythingWithQuery("Import File", $"Text Files (*{SupportedFiles.TXT})|*{SupportedFiles.TXT}|RTF Files (*{SupportedFiles.RTF})|*{SupportedFiles.RTF}|WAV Files (*{SupportedFiles.WAV})|*{SupportedFiles.WAV}|MP3 Files (*{SupportedFiles.MP3})|*{SupportedFiles.MP3}|EXE Files (*{SupportedFiles.EXE})|*{SupportedFiles.EXE}",
                null, desktopPath, desktopPath, 4);
            RoutineLogics.ReloadDesktop(window, desktopPath);
        }

        private void NewFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                string foldername = InputDialog.ShowInputDialog("Please enter the name of the folder:", "New Folder", ImagePaths.NEWFOL_IMG, ImagePaths.LOGO_IMG);
                if (InputDialog.Result == true)
                {
                    string folderpath = Path.Combine(currentDesktop, foldername);
                    if (Directory.Exists(folderpath) == false)
                    {
                        Directory.CreateDirectory(folderpath);
                        RoutineLogics.ReloadDesktop(this, currentDesktop);
                    }
                    else RoutineLogics.ErrorMessage(Errors.CRT_ERR, foldername, " already exists.");
                }
            }
            catch (Exception ex)
            {
                RoutineLogics.ErrorMessage(Errors.CRT_ERR, Errors.READ_ERR_MSG, "the folder\n", ex.Message);
            }
        }

        private void DeleteFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(currentDesktop) && Path.GetFileName(currentDesktop) != Configs.CDESK)
                {
                    new DirectoryInfo(currentDesktop) { Attributes = FileAttributes.Normal };
                    Directory.Delete(currentDesktop);
                    Close();
                }
                else
                {
                    RoutineLogics.ErrorMessage(Errors.PRMS_ERR, "You cannot delete ", Configs.CDESK, " folder.");
                }
            }
            catch (Exception ex)
            {
                RoutineLogics.ErrorMessage(Errors.DEL_ERR, Errors.DEL_ERR_MSG, "the folder.\n", ex.Message);
            }
        }
        private void RenameFolderName(object sender, RoutedEventArgs e)
        {
            if (Title != MainItems.MAIN_WIN)
            {
                string newfoldername = InputDialog.ShowInputDialog("Enter the new name:", "Rename Folder", ImagePaths.FOLDER_IMG, ImagePaths.RENM_IMG);
                if (string.IsNullOrEmpty(newfoldername) == false && newfoldername != Configs.CDESK && newfoldername != MainItems.MAIN_WIN)
                {
                    Title = newfoldername;
                    try
                    {
                        Directory.Move(currentDesktop, Path.Combine(Path.GetDirectoryName(currentDesktop), newfoldername));
                    }
                    catch (Exception ex) 
                    {
                        RoutineLogics.ErrorMessage(Errors.MOVE_ERR, Errors.MOVE_ERR_MSG, newfoldername, ex.Message);
                    }
                }
                else RoutineLogics.ErrorMessage(Errors.PRMS_ERR, "You can not rename folders as null, ", Configs.CDESK, " or ", MainItems.MAIN_WIN);
            }
            else RoutineLogics.ErrorMessage(Errors.PRMS_ERR, "You can not rename ", MainItems.MAIN_WIN, " (", Configs.CDESK, ") folder.");
            
        }
        private void AboutGencosPage_Wanted(object sender, RoutedEventArgs e)
        {
            RoutineLogics.ShowAboutWindow("About GencOS", ImagePaths.HLOGO_IMG, ImagePaths.LOGO_IMG, Versions.GOS_VRS, Messages.ABT_DFLT_MSG);
        }
        private string ConfigurePath(string ConfigFileText)
        {
            string input = InputDialog.ShowInputDialog("Please enter " + Configs.CDESK + " path", "Path Needed");
            //Resumes until valid path is entered
            while (true)
            {
                if (InputDialog.Result == true)
                {
                    if (input.EndsWith(Configs.CDESK) == false) input = InputDialog.ShowInputDialog($"Path must end with {Configs.CDESK}.\nIf {Configs.CDESK} folder does not exists, create one\nand enter the path of the folder", "Invalid path", ImagePaths.WRNG_IMG);
                    else if (Directory.Exists(input) == false) input = InputDialog.ShowInputDialog($"Path to {Configs.CDESK} does not exists.\nPlease check if the path is correct.", "Incorrect path", ImagePaths.WRNG_IMG);
                    else
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(File.Create(ConfigFileText)))
                            {
                                writer.WriteLineAsync(input);
                                string trashpath = Path.Combine(input, HiddenFolders.HTRSH_FOL);
                                string musicpath = Path.Combine(input, HiddenFolders.HAUD_FOL);
                                Directory.CreateDirectory(musicpath);
                                Directory.CreateDirectory(trashpath);
                                new DirectoryInfo(musicpath) { Attributes = FileAttributes.Hidden }; // set the .audio$ folder hidden
                                new DirectoryInfo(trashpath) { Attributes = FileAttributes.Hidden }; // set the .trash$ folder hidden
                                return input;
                            }
                        } catch(Exception ex)
                        {
                            RoutineLogics.ErrorMessage(Errors.WRT_ERR, Errors.WRT_ERR_MSG, "the path to file.\n", ex.Message);
                            Environment.Exit(0);
                        }
                    }
                }
                else
                {
                    switch(MessageBox.Show("If you continue without valid path, your files will not be loaded.\nDo you want to continue?", "OS Without Desktop Path", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning)){
                        case MessageBoxResult.Yes: return null;
                        case MessageBoxResult.No:
                            input = InputDialog.ShowInputDialog($"Please enter {Configs.CDESK} path", "Path Needed");
                            continue;
                        default: Environment.Exit(0); break;

                    }
                }
            }
        }

        #region Subroutines
        private void NoteApp_Clicked(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.NewNote_Wanted(this, currentDesktop);
        }

        private void GenMailApp_Clicked(object sender, RoutedEventArgs e)
        {
            if (RoutineLogics.IsMailAppOpen() == false)
            {
                new GenMailApp();
            }
        }

        private void OpenTrashBacket(object sender, RoutedEventArgs e)
        {
            if (RoutineLogics.IsTrashBacketOpen() == false)
            {
                new TrashBacket();
            }
        }

        private void MusicAppStart(object sender, RoutedEventArgs e)
        {
            if (RoutineLogics.IsMusicAppOpen() == false)
            {
                GenMusicApp.allowToInitialize = true;
                new GenMusicApp();
            }
        }

        private void ReloadDesktop_Wanted(object sender, RoutedEventArgs e)
        {
            RoutineLogics.ReloadDesktop(this, currentDesktop);
        }

        private void ImportFile_Wanted(object sender, RoutedEventArgs e)
        {
            ImportFile(this, currentDesktop);
        }
        #endregion
    }
}
