﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using WPFFrameworkApp2;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Converters;

namespace WPFFrameworkApp
{
    public partial class RoutineLogics
    {
        public static string configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Configs.C_CONFIGS);
        private static string[] fontcolor = File.ReadAllLines(Path.Combine(configFolder, Configs.CFONT));

        public static string menuFontColor = GetFontColor(fontcolor, 2);
        public static string folderFontcolor = GetFontColor(fontcolor, 1);
        public static string desktopFontcolor = GetFontColor(fontcolor, 0);

        #region Style functions
        public static void SetWindowStyles(dynamic menu, dynamic[] menuitems)
        {
            string[] fontsettings = GetFontSettingsFromCfont();
            SolidColorBrush menucolor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetColorSettingsFromCcol()[3]));
            SetSettingsForAllMenu(menu, fontsettings);

            foreach (var item in menuitems)
            {
                item.Background = menucolor;
                item.BorderThickness = new Thickness(0);
            }
        }
        #endregion

        #region Search functions
        public static void AddEveryItemIntoSearch(ComboBox searchBox, string desktopPath)
        {
            searchBox.Items.Clear(); // clear the search box

            AddAllFoldersIntoSearch(searchBox, desktopPath);
            AddAllAppIntoSearch(searchBox, desktopPath);

            var items = searchBox.Items.Cast<object>().ToArray();
            SetWindowStyles(searchBox, items);
        }
        public static void AddAllFoldersIntoSearch(ComboBox searchBox, string folderPath)
        {
            IEnumerable<string> folders = Directory.EnumerateDirectories(folderPath);
            string[] hiddenfolders = { HiddenFolders.HAUD_FOL, HiddenFolders.HTRSH_FOL, HiddenFolders.HPV_FOL };
            foreach (string folder in folders)
            {
                string foldername = Path.GetFileName(folder);
                if (new DirectoryInfo(folder).GetDirectories().Length > 0) AddAllFoldersIntoSearch(searchBox, folder);
                if (hiddenfolders.Contains(foldername) == false)
                {
                    Image imageicon = new Image
                    {
                        Source = new BitmapImage(new Uri(ImagePaths.FOLDER_IMG, UriKind.RelativeOrAbsolute)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 20
                    };

                    TextBlock textblock = new TextBlock
                    {
                        Text = "   " + foldername,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    StackPanel stackpanel = new StackPanel { Orientation = Orientation.Horizontal };
                    stackpanel.Children.Add(imageicon);
                    stackpanel.Children.Add(textblock);

                    ComboBoxItem comboBoxItem = new ComboBoxItem
                    {
                        Content = stackpanel,
                        Tag = foldername
                    };

                    AddSearchBoxItemListeners(comboBoxItem, folder);
                    searchBox.Items.Add(comboBoxItem);
                }
            }
        }
        public static void AddAllAppIntoSearch(ComboBox searchBox, string desktopPath)
        {
            IEnumerable<string> files = Directory.EnumerateFileSystemEntries(desktopPath);
            foreach (string file in files)
            {
                if (file.EndsWith(HiddenFolders.HTRSH_FOL)) continue;

                string image;
                string filename = Path.GetFileName(file);
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                switch (Path.GetExtension(file))
                {
                    case SupportedFiles.TXT: image = ImagePaths.TXT_IMG; break;
                    case SupportedFiles.RTF: image = ImagePaths.RTF_IMG; break;
                    case SupportedFiles.WAV: image = ImagePaths.WAV_IMG; break;
                    case SupportedFiles.MP3: image = ImagePaths.MP3_IMG; break;
                    case SupportedFiles.EXE: image = ImagePaths.EXE_IMG; break;
                    case SupportedFiles.PNG: image = ImagePaths.PNG_IMG; break;
                    case SupportedFiles.JPG: image = ImagePaths.JPG_IMG; break;
                    case SupportedFiles.MP4: image = ImagePaths.MP4_IMG; break;
                    default: AddAllAppIntoSearch(searchBox, file); continue;
                }

                Image imageicon = new Image
                {
                    Source = new BitmapImage(new Uri(image, UriKind.RelativeOrAbsolute)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = 20
                };

                TextBlock textblock = new TextBlock
                {
                    Text = "   " + filename,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                StackPanel stackpanel = new StackPanel { Orientation = Orientation.Horizontal };
                stackpanel.Children.Add(imageicon);
                stackpanel.Children.Add(textblock);

                comboBoxItem.Content = stackpanel;
                comboBoxItem.Tag = filename;

                AddSearchBoxItemListeners(comboBoxItem, file);

                searchBox.Items.Add(comboBoxItem);
            }
        }
        private static void AddSearchBoxItemListeners(ComboBoxItem item, string filepath)
        {
            string filename = item.Tag.ToString();
            string desktopPath = Path.GetDirectoryName(filepath);
            MainWindow window = GetMainWindow(desktopPath);

            switch (Path.GetExtension(filename))
            {
                case SupportedFiles.TXT: item.PreviewMouseDown += (sender, e) => AddTextListener(window, desktopPath, filename); break;
                case SupportedFiles.RTF: item.PreviewMouseDown += (sender, e) => AddRTFListener(window, desktopPath, filename); break;
                case SupportedFiles.WAV: item.PreviewMouseDown += (sender, e) => AddAudioListener(filepath, filename); break;
                case SupportedFiles.MP3: item.PreviewMouseDown += (sender, e) => AddAudioListener(filepath, filename); break;
                case SupportedFiles.EXE: item.PreviewMouseDown += (sender, e) => AddEXEListener(filepath, filename); break;
                case SupportedFiles.PNG: item.PreviewMouseDown += (sender, e) => AddPictureListener(window, desktopPath, filename, filepath); break;
                case SupportedFiles.JPG: item.PreviewMouseDown += (sender, e) => AddPictureListener(window, desktopPath, filename, filepath); break;
                case SupportedFiles.MP4: item.PreviewMouseDown += (sender, e) => AddMP4Listener(desktopPath, filename, filepath); break;
                default: item.PreviewMouseDown += (sender, e) => AddFolderListener(desktopPath, filename); break;
            }
        }
        #endregion

        #region IsOpen functions
        public static bool IsMusicAppOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GenMusicApp)
                {
                    window.Activate(); // GenMusic is open
                    return true;
                }
            }
            return false; // GenMusic is close
        }
        public static bool IsTrashBacketOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is TrashBacket)
                {
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        public static bool IsMailAppOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GenMailApp)
                {
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        public static bool IsPicMovieOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is PicMovie)
                {
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        public static bool IsColorSettingsOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is ColorSettings)
                {
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        public static bool IsFontSettingsOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is FontSettings)
                {
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        public static bool IsCalendarAppOpen()
        {
            foreach(Window window in Application.Current.Windows)
            {
                if (window is CalendarApp)
                {
                    window.Activate();
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region ShortKeys functions
        private static ContextMenu SetShortKeyOptions(MainWindow window, string copyicon, string deleteicon, string filepath, string imageicon)
        {
            string[] fontsettings = GetFontSettingsFromCfont();
            string color = GetColorSettingsFromCcol()[3];
            string currentdesktop = Path.GetDirectoryName(filepath);
            string filename = Path.GetFileName(filepath);

            ContextMenu contextMenu = new ContextMenu();
            MenuItem renameItem = CreateMenuItemForContextMenu("Rename", color, ImagePaths.RENM_IMG);
            MenuItem moveitem = CreateMenuItemForContextMenu("Move", color, ImagePaths.NMOVE_IMG);
            MenuItem copyitem = CreateMenuItemForContextMenu("Copy", color, copyicon);
            MenuItem deleteitem = CreateMenuItemForContextMenu("Delete", color, deleteicon);
            MenuItem moreinfo = CreateMenuItemForContextMenu("Details", color, ImagePaths.INFO_IMG);

            renameItem.Click += (sender, e) =>
            {
                RenameFile_Wanted(filepath, imageicon);
                ReloadWindow(window, currentdesktop);
            };
            copyitem.Click += (sender, e) =>
            {
                CopyAnythingWithQuery("Copy File", "All Files (*.*)|*.*", filename, currentdesktop, currentdesktop);
                ReloadWindow(window, currentdesktop);
            };
            moveitem.Click += (sender, e) =>
            {
                MoveAnythingWithQuery("Move File", "All Files (*.*)|*.*", filename, currentdesktop, currentdesktop, 1);
                ReloadWindow(window, currentdesktop);
            };
            deleteitem.Click += (sender, e) =>
            {
                if (MessageBox.Show($"Are you sure to delete {filename}?", "Delete File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    MoveAnythingWithoutQuery(currentdesktop, filename, Path.Combine(MainWindow.TrashPath, filename));
                    ReloadWindow(window, currentdesktop);
                }
            };
            moreinfo.Click += (sender, e) =>
            {
                setInfoWindow(new InfoWindow(), new FileInfo(filepath), imageicon);
            };

            contextMenu.Items.Add(renameItem);
            contextMenu.Items.Add(moveitem);
            contextMenu.Items.Add(copyitem);
            if (imageicon == ImagePaths.WAV_IMG || imageicon == ImagePaths.MP3_IMG) AddIfSound(contextMenu, color, window, currentdesktop, filename);
            else if (imageicon == ImagePaths.PNG_IMG || imageicon == ImagePaths.JPG_IMG) AddIfPicVideo(contextMenu, color, window, currentdesktop, filename, ImagePaths.ADDPIC_IMG);
            else if (imageicon == ImagePaths.MP4_IMG) AddIfPicVideo(contextMenu, color, window, currentdesktop, filename, ImagePaths.ADDMP4_IMG);
            contextMenu.Items.Add(deleteitem);
            contextMenu.Items.Add(moreinfo);

            SetSettingsForAllMenu(contextMenu, fontsettings);

            return contextMenu;
        }
        private static ContextMenu SetShortKeyOptionsForFolders(MainWindow window, string desktopPath, string filename)
        {
            string color = GetColorSettingsFromCcol()[3];
            ContextMenu contextMenu = new ContextMenu();
            MenuItem renameItem = CreateMenuItemForContextMenu("Rename", color, ImagePaths.RENM_IMG);
            MenuItem deleteitem = CreateMenuItemForContextMenu("Delete", color, ImagePaths.FDEL_IMG);
            MenuItem folderInfo = CreateMenuItemForContextMenu("Details", color, ImagePaths.INFO_IMG);

            renameItem.Click += (sender, e) => AddRenameFolderListener(window, desktopPath, filename);
            deleteitem.Click += (sender, e) => AddDeleteFolderListener(window, desktopPath, filename);
            folderInfo.Click += (sender, e) => setFolderInfoWindow(Path.Combine(desktopPath, filename));

            contextMenu.Items.Add(renameItem);
            contextMenu.Items.Add(deleteitem);
            contextMenu.Items.Add(folderInfo);

            SetSettingsForAllMenu(contextMenu, GetFontSettingsFromCfont());

            return contextMenu;
        }
        private static MenuItem CreateMenuItemForContextMenu(string header, string color, string icon)
        {
            return new MenuItem
            {
                Header = header,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                BorderThickness = new Thickness(0),
                Icon = new Image
                {
                    Source = new BitmapImage(new Uri(icon, UriKind.RelativeOrAbsolute)),
                }
            };

        }
        private static string setExtensionString(string extension)
        {
            switch (extension)
            {
                case SupportedFiles.TXT: return $"Text File ({SupportedFiles.TXT})";
                case SupportedFiles.RTF: return $"Rich Text File ({SupportedFiles.RTF})";
                case SupportedFiles.WAV: return $"Waveform Audio File ({SupportedFiles.WAV})";
                case SupportedFiles.MP3: return $"MPEG-1 Audio Layer 3 Audio File ({SupportedFiles.MP3})";
                case SupportedFiles.EXE: return $"Executable File ({SupportedFiles.EXE})";
                case SupportedFiles.PNG: return $"Portable Network Graphics File ({SupportedFiles.PNG})";
                case SupportedFiles.JPG: return $"JPEG Image File ({SupportedFiles.JPG})";
                case SupportedFiles.MP4: return $"MPEG-4 Video File ({SupportedFiles.MP4})";
                default: return "Unknown File (?.?)";
            }
        }
        private static void setInfoWindow(InfoWindow infowindow, FileInfo fileInfo, string imageicon)
        {
            infowindow.nameInfo.Text = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4); // get only name
            infowindow.typeInfo.Text = setExtensionString(fileInfo.Extension);
            infowindow.sizeInfo.Text = $"{fileInfo.Length / 1024} KB";
            infowindow.locInfo.Text = fileInfo.FullName;
            infowindow.LastCreatedInfo.Text = fileInfo.CreationTime.ToString();
            infowindow.LastModifiedInfo.Text = fileInfo.LastWriteTime.ToString();
            infowindow.LastAccessInfo.Text = fileInfo.LastAccessTime.ToString();
            infowindow.SizeToContent = SizeToContent.Width;
            infowindow.logoimage.Source = new BitmapImage(new Uri(imageicon, UriKind.RelativeOrAbsolute));
        }
        private static void setFolderInfoWindow(string directoryPath)
        {

            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            InfoWindow infowindow = new InfoWindow { Title = "Directory Details" };

            infowindow.logoimage.Source = new BitmapImage(new Uri(ImagePaths.FOLDER_IMG, UriKind.RelativeOrAbsolute));
            infowindow.nameInfo.Text = directoryInfo.Name;
            infowindow.typeInfo.Text = "Directory";
            infowindow.sizeInfo.Text = $"{directoryInfo.GetFiles().Length} files";
            infowindow.locInfo.Text = directoryInfo.FullName;
            infowindow.LastCreatedInfo.Text = directoryInfo.CreationTime.ToString();
            infowindow.LastModifiedInfo.Text = directoryInfo.LastWriteTime.ToString();
            infowindow.LastAccessInfo.Text = directoryInfo.LastAccessTime.ToString();
            infowindow.SizeToContent = SizeToContent.Width;
        }
        #endregion

        #region GetWindow functions
        public static PicMovie GetPicMovieWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is PicMovie)
                {
                    return (PicMovie)window;
                }
            }
            return null;
        }
        private static GenMusicApp GetMusicAppWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GenMusicApp)
                {
                    return (GenMusicApp)window;
                }
            }
            return null;
        }
        public static MainWindow GetMainWindow(string directoryPath)
        {
            string title;
            if (directoryPath.EndsWith(Configs.CDESK)) title = MainItems.MAIN_WIN;
            else title = GetDirectoryName(directoryPath);
            foreach (Window window in Application.Current.Windows)
            {
                if ((window is MainWindow) && (title == window.Title))
                {
                    return (MainWindow)window;
                }
            }
            return null;
        }
        private static TrashBacket GetTrashBacketWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is TrashBacket)
                {
                    return (TrashBacket)window;
                }
            }
            return null;
        }
        #endregion

        #region GetSettings functions
        public static string[] GetFontSettingsFromCfont()
        {
            string[] fontsettings = File.ReadAllLines(Path.Combine(configFolder, Configs.CFONT));
            return fontsettings;
        }
        public static string[] GetColorSettingsFromCcol()
        {
            string[] colors = File.ReadAllLines(Path.Combine(configFolder, Configs.CCOL));
            return colors;
        }
        private static string GetFontColor(string[] fontcolor, short which)
        {
            switch (which)
            {
                case 0: return fontcolor[1]; // desktop font color
                case 1: return fontcolor[6]; // folder font color
                case 2: return fontcolor[11]; // menu font color
                default: return null;
            }
        }
        #endregion

        #region SetSettings functions
        public static void SetSettingsForAllMenu(dynamic menu, string[] fontsettings)
        {
            menu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetColorSettingsFromCcol()[3]));
            menu.BorderThickness = new Thickness(0);
            menu.FontFamily = new FontFamily(fontsettings[10]);
            menu.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontsettings[11]));
            menu.FontWeight = fontsettings[12] == "Bold" ? FontWeights.Bold : FontWeights.Regular;
            menu.FontStyle = fontsettings[13] == "Italic" ? FontStyles.Italic : FontStyles.Normal;
            menu.FontSize = Convert.ToDouble(fontsettings[14]);
        }
        private static void SetWindowSettings(MainWindow window)
        {
            string[] colors = File.ReadAllLines(Path.Combine(configFolder, Configs.CCOL));
            string[] fonts = File.ReadAllLines(Path.Combine(configFolder, Configs.CFONT));
            try
            {
                SetBackgroundSettings(window, colors);
                SetMenuFontSettings(window, fonts);

                window.clockTime.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fonts[11]));
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while configuring desktop. \nDefault settings will be used.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Information);

                SetDefaultsForBackgroundColor(window);
                SetDefaultForFonts(window);
                SetSettingsDefault();

                fontcolor[1] = Defaults.FONT_COL;
            }
        }
        private static void SetDefaultsForBackgroundColor(MainWindow window)
        {
            window.desktop.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Defaults.MAIN_DESK_COl));
            window.folderdesktop.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Defaults.FOL_DESK_COL));
            window.safari.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Defaults.SAFARI_COL));
            window.trashApp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Defaults.SAFARI_COL));
            window.menu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Defaults.MENU_COL));
        }
        private static void SetDefaultForFonts(MainWindow window)
        {
            window.FontFamily = new FontFamily(Defaults.FONT);
            window.menu.FontFamily = new FontFamily(Defaults.FONT);
            window.menu.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Defaults.FONT_COL));
            window.FontWeight = FontWeights.Regular;
            window.FontStyle = FontStyles.Normal;
            window.FontSize = float.Parse(Defaults.FONT_SIZE);
        }
        private static void SetSettingsDefault()
        {
            string[] colors = { Defaults.MAIN_DESK_COl, Defaults.FOL_DESK_COL, Defaults.SAFARI_COL, Defaults.MENU_COL };
            string[] fonts = { Defaults.FONT, Defaults.FONT_COL, Defaults.FONT_WEIGHT, Defaults.FONT_STYLE, Defaults.FONT_SIZE,
                               Defaults.FONT, Defaults.FONT_COL, Defaults.FONT_WEIGHT, Defaults.FONT_STYLE, Defaults.FONT_SIZE,
                               Defaults.FONT, Defaults.FONT_COL, Defaults.FONT_WEIGHT, Defaults.FONT_STYLE, Defaults.FONT_SIZE};

            File.WriteAllLines(Path.Combine(configFolder, Configs.CCOL), colors);
            File.WriteAllLines(Path.Combine(configFolder, Configs.CFONT), fonts);
        }
        private static void SetApplications(MainWindow window)
        {
            if (window.reloadNeed.IsVisible)
            {
                window.reloadNeed.Visibility = Visibility.Collapsed;
                window.NoteApp.Visibility = Visibility.Visible;
                window.MusicApp.Visibility = Visibility.Visible;
                window.MailApp.Visibility = Visibility.Visible;
                window.PicMovie.Visibility = Visibility.Visible;
            }
        }
        private static void SetBackgroundSettings(MainWindow window, string[] colors)
        {
            SolidColorBrush menucolor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[3]));
            SolidColorBrush safaricolor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[2]));
            SolidColorBrush folderdesktopcolor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[1]));

            window.desktop.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[0]));
            window.folderdesktop.Background = folderdesktopcolor;
            window.expander.Background = folderdesktopcolor;
            window.gridSplitter.Background = folderdesktopcolor;
            window.safari.Background = safaricolor;
            window.menu.Background = menucolor;
            window.trashApp.Background = safaricolor;
            window.trashContextMenu.Background = menucolor;
            window.trashContextMenu.BorderThickness = new Thickness(0);

            SetColorOfMenuItems(window, menucolor);
            SetBorderThicknessOfMenuItems(window, new Thickness(0));
        }
        private static void SetMenuFontSettings(MainWindow window, string[] fonts)
        {
            SetSettingsForAllMenu(window.menu, fonts);
            SetSettingsForAllMenu(window.mainContextMenu, fonts);
            SetSettingsForAllMenu(window.trashContextMenu, fonts);
        }
        private static void SetColorOfMenuItems(MainWindow window, SolidColorBrush menucolor)
        {
            //main desktop contexmenu's items
            window.itemmenu1.Background = menucolor;
            window.itemmenu2.Background = menucolor;
            window.itemmenu3.Background = menucolor;

            // trash backet contexmenu's items
            window.trashitem1.Background = menucolor;
            window.trashitem2.Background = menucolor;

            // main menubar's items
            window.menuitem1.Background = menucolor;
            window.menuitem2.Background = menucolor;
            window.menuitem3.Background = menucolor;
            window.menuitem4.Background = menucolor;
            window.menuitem5.Background = menucolor;
            window.menuitem6.Background = menucolor;
            window.menuitem7.Background = menucolor;
            window.menuitem8.Background = menucolor;
            window.menuitem9.Background = menucolor;
        }
        private static void SetBorderThicknessOfMenuItems(MainWindow window, Thickness borderthickness)
        {
            // main desktop contexmenu's items
            window.itemmenu1.BorderThickness = borderthickness;
            window.itemmenu2.BorderThickness = borderthickness;
            window.itemmenu3.BorderThickness = borderthickness;

            // trash backet contexmenu's items
            window.trashitem1.BorderThickness = borderthickness;
            window.trashitem2.BorderThickness = borderthickness;

            // main menubar's items
            window.menuitem1.BorderThickness = borderthickness;
            window.menuitem2.BorderThickness = borderthickness;
            window.menuitem3.BorderThickness = borderthickness;
            window.menuitem4.BorderThickness = borderthickness;
            window.menuitem5.BorderThickness = borderthickness;
            window.menuitem6.BorderThickness = borderthickness;
            window.menuitem7.BorderThickness = borderthickness;
            window.menuitem8.BorderThickness = borderthickness;
            window.menuitem9.BorderThickness = borderthickness;
        }
        #endregion

        #region Rename file functions
        public static void RenameFile_Wanted(string filepath, string ImagePath, string icon = ImagePaths.RENM_IMG)
        {
            string filename = Path.GetFileName(filepath);
            string currentDesktop = Path.GetDirectoryName(filepath);

            string input = InputDialog.ShowInputDialog("Enter the new name:", "Rename File", ImagePath, icon);

            if (InputDialog.Result == false) return;
            if (CheckInputIsNull(input)) return;

            string newfilename = input.ToLower();
            string pathToDirection = Path.Combine(currentDesktop, newfilename);

            if (CheckNewTXTFilenameIsAllowed(filename, newfilename) == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.RTF, "non-RTF") == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.WAV, "non-WAV") == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.MP3, "non-MP3") == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.EXE, "non-EXE") == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.PNG, "non-PNG") == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.JPG, "non-JPG") == false) return;
            if (CheckNewFilenameIsAllowed(filename, newfilename, SupportedFiles.MP4, "non-MP4") == false) return;

            MoveAnythingWithoutQuery(currentDesktop, filename, pathToDirection); // if this function works, then no error occured
        }
        private static bool CheckNewTXTFilenameIsAllowed(string filename, string newfilename)
        {
            if (filename.EndsWith(SupportedFiles.TXT))
            {
                if (newfilename.EndsWith(SupportedFiles.TXT) == false && newfilename.EndsWith(SupportedFiles.RTF) == false) // TXT to RTF is supported
                {
                    ErrorMessage(Errors.PRMS_ERR, "You can not rename", filename, " as a non-TXT file except RTF file");
                    return false; // not allowed
                }
                return true; // allowed
            }
            return true; // not a txt file
        }
        private static bool CheckNewFilenameIsAllowed(string filename, string newfilename, string checkFor, string non_file)
        {
            if (filename.EndsWith(checkFor))
            {
                if (IsRenameAllowed(filename, newfilename, checkFor) == false)
                {
                    ErrorMessage(Errors.PRMS_ERR, "You can not rename ", filename, $" as a {non_file} file");
                    return false; // not allowed
                }
                return true; // allowed
            }
            return true; // not a rtf file
        }
        private static bool CheckNewFolderNameIsAllowed(string newfoldername)
        {
            if (string.IsNullOrEmpty(newfoldername))
            {
                ErrorMessage(Errors.PRMS_ERR, "You can not rename folders as null"); // not allowed
                return false;
            }
            if (newfoldername == HiddenFolders.HTRSH_FOL || newfoldername == HiddenFolders.HAUD_FOL)
            {
                ErrorMessage(Errors.PRMS_ERR, "You can not rename folders as same as hidden folders.");
                return false; // not allowed
            }
            if (newfoldername == Configs.CDESK || newfoldername == Configs.C_CONFIGS || newfoldername == MainItems.MAIN_WIN)
            {
                ErrorMessage(Errors.PRMS_ERR, "You can not rename folders as ", Configs.CDESK, ", ", Configs.C_CONFIGS, " or ", MainItems.MAIN_WIN);
                return false;
            }
            return true; // allowed
        }
        private static bool IsRenameAllowed(string filename, string newfilename, string checkfor) => filename.EndsWith(checkfor) && newfilename.EndsWith(checkfor);
        #endregion

        #region App Creation functions
        public static Button CreateButton(string filename, byte extrasize = 0)
        {
            return new Button()
            {
                Width = 80 + extrasize,
                Height = 80 + extrasize,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                ToolTip = filename
            };
        }
        public static Image CreateImage(byte extrasize = 0)
        {
            return new Image
            {
                Width = 60 + extrasize, // Set desired width
                Height = 60 + extrasize, // Set desired height
            };
        }
        public static TextBlock CreateTextBlock(string filename, string[] fontsettings, short which)
        {
            switch (which)
            {
                case 0: return CreateTextBlockForDesktop(filename, fontsettings);
                case 1: return CreateTextBlockForFolder(filename, fontsettings);
                default: return null;
            }
        }
        public static void Appearence(Image image, StackPanel stackpanel, Button app, TextBlock appname, string logo)
        {
            BitmapImage bitmap = setBitmapImage(logo);
            bitmap.Freeze();
            image.Source = bitmap;

            stackpanel.Children.Add(image);
            stackpanel.Children.Add(appname);

            app.Content = stackpanel;
        }
        private static ImageSource InitTrashBacket()
        {
            try
            {
                IEnumerable<string> files = Directory.GetFiles(Path.Combine(MainWindow.CDesktopPath, HiddenFolders.HTRSH_FOL));
                if (files.Any()) return new BitmapImage(new Uri(ImagePaths.FULL_IMG, UriKind.RelativeOrAbsolute));
                else return new BitmapImage(new Uri(ImagePaths.EMPT_IMG, UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                ErrorMessage("Trashbacket" + Errors.OPEN_ERR, Errors.OPEN_ERR_MSG, HiddenFolders.HTRSH_FOL, "\n", ex.Message);
                return new BitmapImage(new Uri(ImagePaths.EMPT_IMG, UriKind.RelativeOrAbsolute));
            }
        }
        private static TextBlock CreateTextBlockForDesktop(string filename, string[] fontsettings)
        {
            return new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = filename,
                FontFamily = new FontFamily(fontsettings[0]),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontsettings[1])),
                FontWeight = fontsettings[2] == "Bold" ? FontWeights.Bold : FontWeights.Regular,
                FontStyle = fontsettings[3] == "Italic" ? FontStyles.Italic : FontStyles.Normal,
                FontSize = float.Parse(fontsettings[4])
            };
        }
        private static TextBlock CreateTextBlockForFolder(string filename, string[] fontsettings)
        {
            return new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = filename,
                FontFamily = new FontFamily(fontsettings[5]),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontsettings[6])),
                FontWeight = fontsettings[7] == "Bold" ? FontWeights.Bold : FontWeights.Regular,
                FontStyle = fontsettings[8] == "Italic" ? FontStyles.Italic : FontStyles.Normal,
                FontSize = float.Parse(fontsettings[9])
            };
        }
        private static void InitTextFile(MainWindow window, string desktopPath, string filename, string[] fontSettings)
        {
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontSettings, 0);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel { Orientation = Orientation.Vertical };
            Appearence(image, stackpanel, app, appname, ImagePaths.TXT_IMG);

            window.desktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptions(window, ImagePaths.NCOPY_IMG, ImagePaths.NDEL_IMG, Path.Combine(desktopPath, filename), ImagePaths.TXT_IMG);

            app.Click += (sender, e) => AddTextListener(window, desktopPath, filename);
        }
        private static string ReadTXTFile(string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                StringBuilder stringbuilder = new StringBuilder();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    stringbuilder.AppendLine(line);
                }
                return stringbuilder.ToString();
            }
        }
        private static void InitRTFFile(MainWindow window, string desktopPath, string filename, string[] fontsettings)
        {
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontsettings, 0);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            Appearence(image, stackpanel, app, appname, ImagePaths.RTF_IMG);

            window.desktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptions(window, ImagePaths.NCOPY_IMG, ImagePaths.NDEL_IMG, Path.Combine(desktopPath, filename), ImagePaths.RTF_IMG);

            app.Click += (sender, e) => AddRTFListener(window, desktopPath, filename);
        }
        private static void InitAudioFile(MainWindow window, string filepath, string fileimage, string[] fontsettings)
        {
            string filename = Path.GetFileName(filepath);
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontsettings, 0);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            Appearence(image, stackpanel, app, appname, fileimage);

            window.desktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptions(window, ImagePaths.SCOPY_IMG, ImagePaths.SDEL_IMG, filepath, fileimage);

            app.Click += (o, e) => AddAudioListener(filepath, filename);
        }
        private static void InitEXEFile(MainWindow window, string desktopPath, string filepath, string filename, string[] fontsettings)
        {
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontsettings, 0);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            Appearence(image, stackpanel, app, appname, ImagePaths.EXE_IMG);

            window.desktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptions(window, ImagePaths.EXE_IMG, ImagePaths.DELEXE_IMG, filepath, ImagePaths.EXE_IMG);

            app.Click += (s, e) => AddEXEListener(filepath, filename);
        }
        private static void InitPictureFile(MainWindow window, string desktopPath, string filepath, string fileimage, string filename, string[] fontsettings)
        {
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontsettings, 0);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            Appearence(image, stackpanel, app, appname, fileimage);

            window.desktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptions(window, ImagePaths.COPYPIC, ImagePaths.DELPNG_IMG, filepath, fileimage);

            app.Click += (s, e) => AddPictureListener(window, desktopPath, filename, filepath);
        }
        private static void InitMP4File(MainWindow window, string desktopPath, string filepath, string fileimage, string filename, string[] fontsettings)
        {
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontsettings, 0);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            Appearence(image, stackpanel, app, appname, fileimage);
            window.desktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptions(window, ImagePaths.COPYMP4_IMG, ImagePaths.DELMP4_IMG, filepath, fileimage);

            app.Click += (sender, e) => AddMP4Listener(desktopPath, filename, filepath);
        }
        public static BitmapImage setBitmapImage(string imagepath)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load the image into memory
            bitmap.UriSource = new Uri(imagepath, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            bitmap.Freeze(); // Make the image thread-safe

            return bitmap;
        }
        public static PicWindow OpenPicWindow(MainWindow window, string filename, string desktopPath)
        {
            BitmapImage icon = new BitmapImage(new Uri(filename.EndsWith(SupportedFiles.PNG) ? ImagePaths.PNG_IMG : ImagePaths.JPG_IMG, UriKind.RelativeOrAbsolute));
            icon.Freeze();

            PicWindow pictureApp = new PicWindow
            {
                Title = filename,
                Icon = icon,
                window = window,
                desktopPath = desktopPath,
                MaxHeight = SystemParameters.PrimaryScreenHeight * 0.8, // %80 of main screen height
                MaxWidth = SystemParameters.PrimaryScreenWidth * 0.9, // %90 of main screen width
                MinHeight = 200,
                MinWidth = 200,
            };

            return pictureApp;
        }
        private static void InitFolder(MainWindow window, string desktopPath, string filename, string[] fontsettings)
        {
            Button app = CreateButton(filename);
            TextBlock appname = CreateTextBlock(filename, fontsettings, 1);
            Image image = CreateImage();
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            Appearence(image, stackpanel, app, appname, ImagePaths.FOLDER_IMG);

            window.folderdesktop.Children.Add(app);

            app.ContextMenu = SetShortKeyOptionsForFolders(window, desktopPath, filename);

            app.Click += (sender, e) => AddFolderListener(desktopPath, filename);
        }
        #endregion

        #region App Listener functions
        public static void AddTextListener(MainWindow window, string desktopPath, string filename)
        {
            try
            {
                TXTNote noteapp = new TXTNote
                {
                    windowForNote = window,
                    currentDesktopForNote = desktopPath,
                    Title = filename
                };

                noteapp.note.Text = ReadTXTFile(Path.Combine(desktopPath, filename));
            }
            catch (Exception ex)
            {
                ErrorMessage("TXT" + Errors.READ_ERR, Errors.READ_ERR_MSG, filename ?? "null File", "\n", ex.Message);
            }
        }
        public static void AddFolderListener(string desktopPath, string filename)
        {
            MainWindow.TempPath = Path.Combine(desktopPath, filename);
            MainWindow newWindow = new MainWindow
            {
                Title = filename
            };
        }
        public static void AddRTFListener(MainWindow window, string desktopPath, string filename)
        {
            RTFNote noteapp = new RTFNote
            {
                windowForNote = window,
                currentDesktopForNote = desktopPath,
                Title = filename
            };
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(desktopPath, filename)))
                {
                    StringBuilder stringbuilder = new StringBuilder();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        stringbuilder.Append(line);
                    }
                    TextRange textRange = new TextRange(noteapp.RichNote.Document.ContentStart, noteapp.RichNote.Document.ContentEnd);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (StreamWriter writer = new StreamWriter(memoryStream))
                        {
                            writer.Write(stringbuilder);
                            writer.Flush();
                            memoryStream.Position = 0; // Reset the stream position

                            textRange.Load(memoryStream, DataFormats.Rtf);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage("RTF" + Errors.OPEN_ERR, Errors.READ_ERR_MSG, filename ?? "null File", "\n", ex.Message);
            }
        }
        public static void AddAudioListener(string filepath, string filename)
        {
            CloseAllGenMusicApps();
            GenMusicApp.mediaPlayer?.Close();
            GenMusicApp.mediaPlayer = null;
            GenMusicApp.isPaused = true;
            GenMusicApp musicapp = new GenMusicApp();
            musicapp.MusicAppButton_Clicked(filepath, filename);
        }
        public static void AddEXEListener(string filepath, string filename)
        {
            string[] options = { "Run", "Cancel" };
            if (QueryDialog.ShowQueryDialog($"Are you sure you want to run {filename}?", "Executable File Run", options, ImagePaths.EXE_IMG) == 0)
            {
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
                    ErrorMessage(Errors.RUN_ERR, Errors.RUN_ERR_MSG, filename ?? "null File", "\n", ex.Message);
                }
            }
        }
        public static void AddPictureListener(MainWindow window, string desktopPath, string filename, string filepath)
        {
            PicWindow pictureApp = OpenPicWindow(window, filename, desktopPath);

            pictureApp.PicMain.Source = setBitmapImage(filepath);
            pictureApp.SizeToContent = SizeToContent.WidthAndHeight;
        }
        public static void AddMP4Listener(string desktopPath, string filename, string filepath)
        {
            var videoApp = new VideoWindow
            {
                Title = filename,
                desktopPath = desktopPath
            };
            videoApp.videoPlayer.Source = new Uri(filepath);
        }
        #endregion

        #region Window Reload functions
        public static void ReloadWindow(MainWindow window, string desktopPath, ComboBox searchBox = null)
        {
            window.desktop.Children.Clear();
            window.folderdesktop.Children.Clear();
            SetWindowSettings(window);
            try
            {
                string[] fontsettings = GetFontSettingsFromCfont();
                string[] hiddenfolders = { HiddenFolders.HAUD_FOL, HiddenFolders.HTRSH_FOL, HiddenFolders.HPV_FOL };
                IEnumerable<string> files = Directory.EnumerateFileSystemEntries(desktopPath);
                foreach (string file in files)
                {
                    string filename = Path.GetFileName(file);
                    if (Directory.Exists(file))
                    {
                        if (hiddenfolders.Contains(filename) == false) InitFolder(window, desktopPath, filename, fontsettings);
                        else // then it is trashbacket
                        {
                            Grid.SetColumnSpan(window.safari, 1);
                            window.trashApp.Visibility = Visibility.Visible;
                            window.trashimage.Source = InitTrashBacket();
                        }
                        continue;
                    }
                    switch (Path.GetExtension(filename))
                    {
                        case SupportedFiles.TXT: InitTextFile(window, desktopPath, filename, fontsettings); break;
                        case SupportedFiles.RTF: InitRTFFile(window, desktopPath, filename, fontsettings); break;
                        case SupportedFiles.WAV: InitAudioFile(window, file, ImagePaths.WAV_IMG, fontsettings); break;
                        case SupportedFiles.MP3: InitAudioFile(window, file, ImagePaths.MP3_IMG, fontsettings); break;
                        case SupportedFiles.EXE: InitEXEFile(window, desktopPath, file, filename, fontsettings); break;
                        case SupportedFiles.PNG: InitPictureFile(window, desktopPath, file, ImagePaths.PNG_IMG, filename, fontsettings); break;
                        case SupportedFiles.JPG: InitPictureFile(window, desktopPath, file, ImagePaths.JPG_IMG, filename, fontsettings); break;
                        case SupportedFiles.MP4: InitMP4File(window, desktopPath, file, ImagePaths.MP4_IMG, filename, fontsettings); break;
                        default: ErrorMessage(Errors.UNSUPP_ERR, filename, " is not supported for ", Versions.GOS_VRS); File.Delete(file); break;
                    }
                }
                if (searchBox != null) AddEveryItemIntoSearch(searchBox, desktopPath);
                SetApplications(window);
                window.Show();
            }
            catch (Exception e)
            {
                ErrorMessage(Errors.REL_ERR, Errors.REL_ERR_MSG, "Main Window\n", e.Message);
                MainWindowManuallyReloadNeeded(window);
            }
        }
        public static void WindowReloadNeeded(string directoryName)
        {
            MainWindow directionFolder = GetMainWindow(directoryName);
            if (directionFolder != null && directoryName != null) ReloadWindow(directionFolder, directoryName);
        }
        public static void ReloadNeededForEveryWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    MainWindowManuallyReloadNeeded((MainWindow)window);
                }
            }
        }
        public static void MainWindowManuallyReloadNeeded(MainWindow mainfolder)
        {
            mainfolder.reloadNeed.Visibility = Visibility.Visible;
            mainfolder.NoteApp.Visibility = Visibility.Hidden;
            mainfolder.MusicApp.Visibility = Visibility.Collapsed;
            mainfolder.MailApp.Visibility = Visibility.Collapsed;
            mainfolder.PicMovie.Visibility = Visibility.Collapsed;
        }
        private static void MusicAppReloadNeeded()
        {
            GenMusicApp musicapp = GetMusicAppWindow();
            if (musicapp != null)
            {
                musicapp.IsReloadNeeded(true);
            }
        }
        private static void TrashBacketReloadNeeded()
        {
            TrashBacket trashapp = GetTrashBacketWindow();
            trashapp?.ReloadWindow();
        }
        #endregion

        #region File Movement functions
        public static void MoveAnythingWithQuery(string title, string filter, string selectedFileName, string initialDirectory, string currentDesktop, byte toWhere)
        {
            switch (toWhere)
            {
                case 1:
                    MoveSomeWhere(title, filter, selectedFileName, initialDirectory, currentDesktop);
                    break;
                case 2:
                    MoveCertainWindow(title, filter, initialDirectory, currentDesktop, MainWindow.MusicAppPath);
                    MusicAppReloadNeeded();
                    break;
                case 3:
                    MoveCertainWindow(title, filter, initialDirectory, currentDesktop, MainWindow.TrashPath);
                    TrashBacketReloadNeeded();
                    break;
                case 4:
                    MoveCertainWindow(title, filter, initialDirectory, currentDesktop, MainWindow.CDesktopPath);
                    WindowReloadNeeded(MainWindow.CDesktopPath);
                    break;
                case 5:
                    MoveCertainWindow(title, filter, initialDirectory, currentDesktop, MainWindow.PicVideoPath);
                    break;
            }
        }
        public static void MoveAnythingWithoutQuery(string currentDesktop, string filename, string pathToDirection) // path to the file will go
        {
            try
            {
                File.Move(Path.Combine(currentDesktop, filename), pathToDirection);
                string directoryName = Path.GetDirectoryName(pathToDirection);
                if (directoryName == MainWindow.TrashPath) TrashBacketReloadNeeded();
                else if (directoryName == MainWindow.MusicAppPath) MusicAppReloadNeeded();
                else WindowReloadNeeded(directoryName);
            }
            catch (Exception e)
            {
                ErrorMessage(Errors.MOVE_ERR, Errors.MOVE_ERR_MSG, filename ?? "null File", "\n", e.Message);
            }
        }
        public static void CopyAnythingWithQuery(string title, string filter, string selectedFileName, string initialDirectory, string currentDesktop)
        {
            SaveFileDialog copydialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = selectedFileName,
                InitialDirectory = initialDirectory,
            };

            if (copydialog.ShowDialog() == true)
            {
                string filepath = copydialog.FileName;
                string filename = Path.GetFileName(filepath); // filepath = the path where the file will go
                if (filepath.Contains(HiddenFolders.HAUD_FOL) == false && filepath.Contains(HiddenFolders.HTRSH_FOL) == false)
                {
                    try
                    {
                        File.Copy(Path.Combine(currentDesktop, filename), filepath);
                        WindowReloadNeeded(Path.GetDirectoryName(filepath));
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage(Errors.COPY_ERR, Errors.COPY_ERR_MSG, filename ?? "null File", "\n", ex.Message);
                    }
                }
                else
                {
                    ErrorMessage(Errors.PRMS_ERR, "You can not move or copy files into hidden folders manually");
                }
            }
        }
        private static void MoveSomeWhere(string title, string filter, string selectedFileName, string initialDirectory, string currentDesktop)
        {
            SaveFileDialog movedialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = selectedFileName,
                InitialDirectory = initialDirectory,
            };

            if (movedialog.ShowDialog() == true)
            {
                string filepath = movedialog.FileName;
                string filename = Path.GetFileName(filepath); // filepath = the path where the file will go
                if (filepath.Contains(HiddenFolders.HAUD_FOL) == false && filepath.Contains(HiddenFolders.HTRSH_FOL) == false)
                {
                    try
                    {
                        File.Move(Path.Combine(currentDesktop, filename), filepath);
                        WindowReloadNeeded(Path.GetDirectoryName(filepath));
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage(Errors.MOVE_ERR, Errors.MOVE_ERR_MSG, filename ?? "null File", "\n", ex.Message);
                    }
                }
                else // user tried to move into hidden folders
                {
                    ErrorMessage(Errors.PRMS_ERR, "You can not move or copy files into hidden folders manually");
                }
            }
        }
        private static void MoveCertainWindow(string title, string filter, string initialDirectory, string currentDesktop, string window)
        {
            OpenFileDialog movedialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                InitialDirectory = initialDirectory,
                Multiselect = true
            };

            if (movedialog.ShowDialog() == true)
            {
                foreach (string filepath in movedialog.FileNames)
                {
                    string filename = Path.GetFileName(filepath);
                    try
                    {
                        File.Move(filepath, Path.Combine(window, filename));
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage(Errors.MOVE_ERR, Errors.MOVE_ERR_MSG, filename ?? "null File", "\n", ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Unclassified public functions
        public static void ErrorMessage(string errtitle, params string[] errmessage)
        {
            StringBuilder stringbuilder = new StringBuilder();
            foreach (string str in errmessage)
            {
                stringbuilder.Append(str);
            }
            MessageBox.Show(stringbuilder.ToString(), errtitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void ShowAboutWindow(string title, string image, string icon, string version, string message)
        {
            AboutWindow aboutwindow = new AboutWindow
            {
                Title = title
            };
            aboutwindow.WhatAbout.Source = new BitmapImage(new Uri(image, UriKind.RelativeOrAbsolute)); ;
            aboutwindow.Icon = new BitmapImage(new Uri(icon, UriKind.RelativeOrAbsolute));
            aboutwindow.Version.Text = version;
            aboutwindow.AboutMessage.Text = message;
        }
        public static string TimeFormat(double totaltime)
        {
            int minutes = (int)(totaltime / 60);
            int seconds = (int)(totaltime % 60);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        #endregion

        #region Unclassified private functions
        private static string GetDirectoryName(string directoryPath)
        {
            string[] parts = directoryPath.Split(Path.DirectorySeparatorChar);
            if (parts.Length > 0)
            {
                return parts[parts.Length - 1];
            }
            return string.Empty;
        }
        private static bool CheckInputIsNull(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                ErrorMessage(Errors.PRMS_ERR, "You can not rename your file as null");
                return true;
            }
            return false;
        }
        private static void CloseAllGenMusicApps()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GenMusicApp musicapp)
                {
                    window.Close();
                }
            }
        }
        private static void AddIfSound(ContextMenu contextMenu, string color, MainWindow window, string currentdesktop, string filename)
        {
            MenuItem addtogenmuic = CreateMenuItemForContextMenu("Add to GenMusic", color, ImagePaths.SADD_IMG);

            addtogenmuic.Click += (sender, e) =>
            {
                MoveAnythingWithoutQuery(currentdesktop, filename, Path.Combine(MainWindow.MusicAppPath, filename));
                ReloadWindow(window, currentdesktop);
                GenMusicApp genmusicapp = GetMusicAppWindow();
                genmusicapp?.IsReloadNeeded(true);
            };

            contextMenu.Items.Add(addtogenmuic);
        }
        private static void AddIfPicVideo(ContextMenu contextMenu, string color, MainWindow window, string currentdesktop, string filename, string addfilemenuicon)
        {
            MenuItem addtopicmovie = CreateMenuItemForContextMenu("Add to PicMovie", color, addfilemenuicon);

            addtopicmovie.Click += (sender, e) =>
            {
                MoveAnythingWithoutQuery(currentdesktop, filename, Path.Combine(MainWindow.PicVideoPath, filename));
                ReloadWindow(window, currentdesktop);
                PicMovie picmovie = GetPicMovieWindow();
                picmovie?.ReloadWindow();
            };

            contextMenu.Items.Add(addtopicmovie);
        }
        private static void AddRenameFolderListener(MainWindow window, string desktopPath, string filename)
        {
            string newfilename = InputDialog.ShowInputDialog("Enter the new name:", "Rename Folder", ImagePaths.FOLDER_IMG, ImagePaths.RENM_IMG);
            if (InputDialog.Result == false) return;
            if (CheckNewFolderNameIsAllowed(newfilename))
            {
                try
                {
                    Directory.Move(Path.Combine(desktopPath, filename), Path.Combine(desktopPath, newfilename));
                    ReloadWindow(window, desktopPath);
                }
                catch (Exception ex)
                {
                    ErrorMessage(Errors.MOVE_ERR, Errors.MOVE_ERR_MSG, filename ?? "null Folder", "\n", ex.Message);
                }
            }
        }
        private static void AddDeleteFolderListener(MainWindow window, string desktopPath, string filename)
        {
            if (MessageBox.Show($"Are you sure to delete {filename} folder?", "Folder Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(Path.Combine(desktopPath, filename));
                    ReloadWindow(window, desktopPath);
                }
                catch (Exception ex)
                {
                    ErrorMessage(Errors.DEL_ERR, Errors.DEL_ERR_MSG, filename ?? "null Folder", "\n", ex.Message);
                }
            }
        }
        #endregion
    }
}
