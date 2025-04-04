﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFFrameworkApp
{
    /// <summary>
    /// TrashBacket.xaml etkileşim mantığı
    /// </summary>
    public partial class TrashBacket : Window
    {
        public TrashBacket()
        {
            InitializeComponent();
            ReloadTrashBacket();
            Show();
        }

        public void ReloadTrashBacket()
        {
            trashPanel.Children.Clear();
            string[] options = { "Shred", "Rescue" };
            IEnumerable<string> trashes = Directory.EnumerateFileSystemEntries(MainWindow.TrashPath);
            foreach (string trash in trashes)
            {
                string[] color = RoutineLogics.GetFontSettingsFromCfont();
                string trashname = Path.GetFileName(trash);
                Button app = CreateButtonForTrashBacket(trashname);
                TextBlock appname = RoutineLogics.CreateTextBlock(trashname, color, 0);
                Image image = RoutineLogics.CreateImage();
                StackPanel stackpanel = new StackPanel() { Orientation = Orientation.Vertical };

                if (trashname.EndsWith(SupportedFiles.TXT))
                {
                    RoutineLogics.Appearence(image, stackpanel, app, appname, ImagePaths.TXT_IMG);
                    AddListener(app, trash, trashname, options, ImagePaths.TXT_IMG);
                }
                else if (trashname.EndsWith(SupportedFiles.RTF))
                {
                    RoutineLogics.Appearence(image, stackpanel, app, appname, ImagePaths.RTF_IMG);
                    AddListener(app, trash, trashname, options, ImagePaths.RTF_IMG);
                }
                else if (trashname.EndsWith(SupportedFiles.EXE))
                {
                    RoutineLogics.Appearence(image, stackpanel, app, appname, ImagePaths.EXE_IMG);
                    AddListener(app, trash, trashname, options, ImagePaths.EXE_IMG);
                }
                else if (trashname.EndsWith(SupportedFiles.MP3))
                {
                    RoutineLogics.Appearence(image, stackpanel, app, appname, ImagePaths.MP3_IMG);
                    AddListener(app, trash, trashname, options, ImagePaths.MP3_IMG);
                }
                else if (trashname.EndsWith(SupportedFiles.WAV))
                {
                    RoutineLogics.Appearence(image, stackpanel, app, appname, ImagePaths.WAV_IMG);
                    AddListener(app, trash, trashname, options, ImagePaths.WAV_IMG);
                }
                trashPanel.Children.Add(app);
            }
        }
        private void AddListener(Button app, string trash, string trashname, string[] options, string Image)
        {
            app.Click += (sender, e) =>
            {
                try
                {
                    switch(QueryDialog.ShowQueryDialog(trashname, "Deleted Item Options", options, Image, ImagePaths.FULL_IMG)) 
                    {
                        case 0:
                            File.Delete(trash); // delete selected
                            ReloadTrashBacket();
                            break;
                        case 1:
                            RoutineLogics.MoveAnythingWithoutQuery(MainWindow.TrashPath, trashname, Path.Combine(MainWindow.CDesktopPath, trashname)); // rescue selected
                            ReloadTrashBacket();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    RoutineLogics.ErrorMessage(Errors.DEL_ERR, Errors.DEL_ERR_MSG, trashname, "\n", ex.Message);
                }
            };
        }

        private void TrashBacketReload_Wanted(object sender, RoutedEventArgs e)
        {
            ReloadTrashBacket();
        }

        private void RescueAll(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to rescue all files from TrashBacket?", "Empty Trash", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                IEnumerable<string> trashes = Directory.EnumerateFileSystemEntries(MainWindow.TrashPath);
                foreach (string trash in trashes)
                {
                    try
                    {
                        File.Move(trash, Path.Combine(MainWindow.CDesktopPath, Path.GetFileName(trash)));
                    } catch(Exception ex)
                    {
                        RoutineLogics.ErrorMessage(Errors.REL_ERR, Errors.REL_ERR_MSG, $"Trash Backet, deleting {Path.GetFileName(trash)}", "\n", ex.Message);
                        File.Delete(trash); // delete selected
                    }
                }
                RoutineLogics.WindowReloadNeeded(MainWindow.CDesktopPath);
                trashPanel.Children.Clear();
            }
        }

        private Button CreateButtonForTrashBacket(string filename)
        {
            return new Button()
            {
                Width = 80,
                Height = 80,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                ToolTip = filename,
            };
        }

        private void EmptyTrash(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to empty the TrashBacket?\nYou can not take back again.", "Empty Trash", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                IEnumerable<string> trashes = Directory.EnumerateFileSystemEntries(MainWindow.TrashPath);
                foreach(string trash in trashes)
                {
                    File.Delete(trash);
                }
                trashPanel.Children.Clear();
            }
        }
    }
}
