using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            CDesktopPath = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "CDesktop.txt")).Trim();
            MusicAppPath = Path.Combine(CDesktopPath, ".audios$");
            TrashPath = Path.Combine(CDesktopPath, ".trash$");
            if (currentDesktop != null) ReloadDesktop(this, currentDesktop);
        }
        public static void ReloadDesktop(MainWindow window, string desktopPath)
        {
            window.desktop.Children.Clear();
            window.folderdesktop.Children.Clear();
            try
            {
                string[] hiddenfolders = {".audios$", ".trash$"};
                IEnumerable<string> files = Directory.EnumerateFileSystemEntries(desktopPath);
                foreach (string file in files)
                {
                    string filename = Path.GetFileName(file);
                    Button app = CreateButton(filename);
                    TextBlock appname = CreateTextBlock(filename);
                    Image image = CreateImage();
                    StackPanel stackpanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical
                    };

                    if (Directory.Exists(file))
                    {
                        if (hiddenfolders.Contains(filename) == false) InitFolder(window, image, stackpanel, app, appname, desktopPath, filename);
                        else
                        {
                            Grid.SetColumnSpan(window.safari, 1);
                            window.trashApp.Visibility = Visibility.Visible;
                            window.trashimage.Source = InitTrashBacket();
                        }
                    }
                    else if (filename.EndsWith(".txt")) InitTextFile(window, image, stackpanel, app, appname, desktopPath, filename);
                    else if (filename.EndsWith(".rtf")) InitRTFFile(window, image, stackpanel, app, appname, desktopPath, filename);
                    else if (filename.EndsWith(".wav")) InitAudioFile(window, image, stackpanel, app, appname, file, filename, "Images/soundwav.png");
                    else if (filename.EndsWith(".mp3")) InitAudioFile(window, image, stackpanel, app, appname, file, filename, "Images/sound128x128.png");
                    else if (filename.EndsWith(".exe")) InitEXEFile(window, image, stackpanel, app, appname, desktopPath, file, filename);
                    else
                    {
                        ErrorMessage($"{filename} is not supported for GencOS now.", "Unsupported File Error");
                        File.Delete(file);
                    }
                }
                window.Show();
            }
            catch(Exception e)
            {
                ErrorMessage("An error occured while reloading the desktop.\n" + e.Message, "Desktop Error");
            }
        }

        private static void InitTextFile(MainWindow window, Image image, dynamic stackpanel, Button app, TextBlock appname, string desktopPath, string filename)
        {
            Appearence(image, stackpanel, app, appname, "Images/textfile.png");

            window.desktop.Children.Add(app);

            app.Click += (sender, e) =>
            {
                try
                {
                    TXTNote noteapp = new TXTNote
                    {
                        windowForNote = window,
                        currentDesktopForNote = desktopPath,
                        Title = filename
                    };
                    noteapp.note.Text = File.ReadAllText(Path.Combine(desktopPath, filename));
                }
                catch(Exception ex)
                {
                    ErrorMessage("An error occured while reading the file.\n" + ex.Message, "Text Opening Error");
                }
            };
        }

        private static void InitRTFFile(MainWindow window, Image image, dynamic stackpanel, Button app, TextBlock appname, string desktopPath, string filename)
        {
            Appearence(image, stackpanel, app, appname, "Images/rtffile.png");

            window.desktop.Children.Add(app);

            app.Click += (sender, e) =>
            {
                RTFNote noteapp = new RTFNote
                {
                    windowForNote = window,
                    currentDesktopForNote = desktopPath,
                    Title = filename
                };
                try
                {
                    string rtfContent = File.ReadAllText(Path.Combine(desktopPath, filename));
                    TextRange textRange = new TextRange(noteapp.RichNote.Document.ContentStart, noteapp.RichNote.Document.ContentEnd);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (StreamWriter writer = new StreamWriter(memoryStream))
                        {
                            writer.Write(rtfContent);
                            writer.Flush();
                            memoryStream.Position = 0; // Reset the stream position

                            textRange.Load(memoryStream, DataFormats.Rtf);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage("An error occured while reading the file.\n" + ex.Message, "RTF Opening Error");
                }
            };
        }

        private static void InitAudioFile(MainWindow window, Image image, dynamic stackpanel, Button app, TextBlock appname, string filepath, string filename, string fileimage)
        {
            Appearence(image, stackpanel, app, appname, fileimage);

            window.desktop.Children.Add(app);

            app.Click += (o, e) =>
            {
                GenMusicApp.mediaPlayer.Close();
                GenMusicApp.currentAudio = filename;
                GenMusicApp.mediaPlayer = new MediaPlayer();
                new GenMusicApp();
                try
                {
                    GenMusicApp.mediaPlayer.Open(new Uri(filepath, UriKind.Relative));
                    GenMusicApp.mediaPlayer.Play();
                }
                catch (Exception ex)
                {
                    ErrorMessage($"An error occured while opening {filename}" + ex.Message, "Audio Opening Error");
                }
            };
        }

        private static void InitEXEFile(MainWindow window, Image image, dynamic stackpanel, Button app, TextBlock appname, string desktopPath, string filepath, string filename)
        {
            Appearence(image, stackpanel, app, appname, "Images/exepenguin.png");

            window.desktop.Children.Add(app);

            app.Click += (s, e) =>
            {
                string[] options = {"Run", "Delete"};
                switch(QueryDialog.ShowQueryDialog(filename, "Executable File Options", options, "Images/exepenguin.png"))
                {
                    case 0:
                        Process process = new Process();
                        process.StartInfo.FileName = filepath;
                        process.StartInfo.CreateNoWindow = true;
                        try
                        {
                            process.Start();
                            process.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage($"An error occured while running {filename}.\n" + ex.Message, "Exe running Error");
                        }
                        break;

                    case 1:
                        File.Move(filepath, Path.Combine(TrashPath, filename));
                        ReloadDesktop(window, desktopPath);
                        break;
                }
            };
        }
        private static void InitFolder(MainWindow window, Image image, StackPanel stackpanel, Button app, TextBlock appname, string desktopPath, string filename)
        {
            Appearence(image, stackpanel, app, appname, "Images/folder2.png");

            window.folderdesktop.Children.Add(app);

            app.Click += (sender, e) =>
            {
                TempPath = Path.Combine(desktopPath, filename);
                MainWindow newWindow = new MainWindow
                {
                    Title = filename
                };
            };
        }

        private static ImageSource InitTrashBacket()
        {
            try
            {
                IEnumerable<string> files = Directory.GetFiles(Path.Combine(CDesktopPath, ".trash$"));
                if (files.Any()) return new BitmapImage(new Uri("Images/trashfull.png", UriKind.RelativeOrAbsolute));
                else return new BitmapImage(new Uri("Images/trashempty.png", UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex) 
            {
                ErrorMessage("An error occured while opening .trash$\n" + ex.Message, "Trashbacket Opening Error");
                return new BitmapImage(new Uri("Images/trashempty.png", UriKind.RelativeOrAbsolute));
            }
        }
        private void CheckConfig()
        {
            string ConfigFileText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "CDesktop.txt"); //Home Directory\CDesktop.txt
            if (File.Exists(ConfigFileText) == false)
            {
                if (MessageBox.Show("CDesktop.txt could not find. Resetting path needed.", "Path could not find", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
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
                        if (MessageBox.Show("Path to C_DESKTOP is corrupted or does not exists.\nPlease reset the desktop path", "Incorrect path", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                            TempPath = ConfigurePath(ConfigFileText);
                        else Environment.Exit(0);
                    }
                }
                catch (Exception e)
                {
                    ErrorMessage("An error occured while reading the path from file.\n" + e.Message, NoteAppLogics.readerror);
                    Environment.Exit(0);
                }
            }
        }

        private void NewFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                string foldername = InputDialog.ShowInputDialog("Please enter the name of the folder:", "New Folder", "Images/newfolder.png", "Images/paperplane2.png");
                if (InputDialog.Result == true)
                {
                    string folderpath = Path.Combine(currentDesktop, foldername);
                    if (Directory.Exists(folderpath) == false)
                    {
                        Directory.CreateDirectory(folderpath);
                        ReloadDesktop(this, currentDesktop);
                    }
                    else ErrorMessage($"{foldername} already exists.", "Creation Error");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("An error occured while creating the folder.\n" + ex.Message, "Creation Error");
            }
        }

        private void DeleteFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(currentDesktop) && Path.GetFileName(currentDesktop) != "C_DESKTOP")
                {
                    new DirectoryInfo(currentDesktop) { Attributes = FileAttributes.Normal};
                    Directory.Delete(currentDesktop);
                    Close();
                }
                else
                {
                    ErrorMessage("You cannot delete C_DESKTOP folder.", "Permission Error");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("An error occured while deleting the folder.\n" + ex.Message, NoteAppLogics.deleteerror);
            }
            
        }

        private string ConfigurePath(string ConfigFileText)
        {
            string input = InputDialog.ShowInputDialog("Please enter C_DESKTOP path", "Path Needed");
            //Resumes until valid path is entered
            while (true)
            {
                if (InputDialog.Result == true)
                {
                    if (input.EndsWith("C_DESKTOP") == false) input = InputDialog.ShowInputDialog("Path must end with C_DESKTOP.\nIf C_DESKTOP folder does not exists, create one\nand enter the path of the folder", "Invalid path", "Images/warning1.png");
                    else if (Directory.Exists(input) == false) input = InputDialog.ShowInputDialog("Path to C_DESKTOP does not exists.\nPlease check if the path is correct.", "Incorrect path", "Images/warning1.png");
                    else
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(File.Create(ConfigFileText)))
                            {
                                writer.WriteLine(input);
                                Directory.CreateDirectory(Path.Combine(input, ".audios$"));
                                return input;
                            }
                        } catch(Exception e)
                        {
                            ErrorMessage("An error occured while writing the path to file.\n" + e.Message, "Write Error");
                            Environment.Exit(0);
                        }
                    }
                }
                else
                {
                    switch(MessageBox.Show("If you continue without valid path, your files will not be loaded.\nDo you want to continue?", "OS Without Desktop Path", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning)){
                        case MessageBoxResult.Yes: return null;
                        case MessageBoxResult.No:
                            input = InputDialog.ShowInputDialog("Please enter C_DESKTOP path", "Path Needed");
                            continue;
                        default: Environment.Exit(0); break;

                    }
                }
            }
        }
        private void NoteApp_Clicked(object sender, RoutedEventArgs e)
        {
            NoteAppLogics.NewNote_Wanted(this, currentDesktop);
        }

        private void MusicAppStart(object sender, RoutedEventArgs e)
        {
            if (IsWindowOpen() == false)
            {
               new GenMusicApp();
            }
            else
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GenMusicApp)
                    {
                        window.Activate();
                    }
                }
            }
        }

        private void ReloadDesktop_Wanted(object sender, RoutedEventArgs e)
        {
            ReloadDesktop(this, currentDesktop);
        }

        private void ImportFile_Wanted(object sender, RoutedEventArgs e)
        {
            ImportFile(this, currentDesktop);
        }

        private void OpenTrashBacket(object sender, RoutedEventArgs e)
        {
            new TrashBacket();
        }

        public static Button CreateButton(string filename)
        {
            return new Button()
            {
                Width = 80,
                Height = 80,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                ToolTip = filename
            };  
        }

        public static TextBlock CreateTextBlock(string filename)
        {
            return new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = filename
            };
        }

        public static Image CreateImage()
        {
            return new Image
            {
                Width = 60, // Set desired width
                Height = 60, // Set desired height
            };
        }

        public static void Appearence(Image image, dynamic stackpanel, Button app, TextBlock appname, string logo)
        {
            BitmapImage bitmap = new BitmapImage(new Uri(logo, UriKind.RelativeOrAbsolute));
            bitmap.Freeze();
            image.Source = bitmap;

            stackpanel.Children.Add(image);
            stackpanel.Children.Add(appname);

            app.Content = stackpanel;
        }

        public static void ErrorMessage(string errmessage, string errtitle)
        {
            MessageBox.Show(errmessage, errtitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void ImportFile(MainWindow window, string desktopPath)
        {
            Microsoft.Win32.OpenFileDialog filedialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|RTF Files (*.rtf)|*.rtf|WAV Files (*.wav)|*.wav|MP3 Files (*.mp3)|*.mp3|EXE Files (*.exe)|*.exe",
                Title = "Import File" 
            };

            if (filedialog.ShowDialog() == true) {
                string filepath = filedialog.FileName;
                string filename = Path.GetFileName(filepath);
                try
                {
                    File.Move(filepath, Path.Combine(desktopPath, filename));
                    ReloadDesktop(window, desktopPath);
                } catch(Exception ex)
                {
                    ErrorMessage($"An error occured while importing {filename}\n" + ex.Message, "Import Error");
                }
            }
        }

        private bool IsWindowOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GenMusicApp)
                {
                    return true; // GenMusic is open
                }
            }
            return false; // GenMusic is close
        }
    }
}
