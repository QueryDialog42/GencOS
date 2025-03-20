using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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

        private void ReloadTrashBacket()
        {
            trashPanel.Children.Clear();
            string[] options = { "Shred", "Rescue" };
            string txtfileImage = "Images/textfile.png";
            string rtffileImage = "Images/rtffile.png";
            string exefileimage = "Images/exepenguin.png";
            string mp3fileimage = "Images/sound128x128.png";
            string wavfileimage = "Images/soundwav.png";
            IEnumerable<string> trashes = Directory.EnumerateFileSystemEntries(MainWindow.TrashPath);
            foreach (string trash in trashes)
            {
                string trashname = Path.GetFileName(trash);
                Button app = MainWindow.CreateButton(trashname);
                TextBlock appname = MainWindow.CreateTextBlock(trashname);
                Image image = MainWindow.CreateImage();
                StackPanel stackpanel = new StackPanel() { Orientation = Orientation.Vertical };

                if (trashname.EndsWith(".txt"))
                {
                    MainWindow.Appearence(image, stackpanel, app, appname, txtfileImage);
                    AddListener(app, trash, trashname, options, txtfileImage);
                }
                else if (trashname.EndsWith(".rtf"))
                {
                    MainWindow.Appearence(image, stackpanel, app, appname, rtffileImage);
                    AddListener(app, trash, trashname, options, rtffileImage);
                }
                else if (trashname.EndsWith(".exe"))
                {
                    MainWindow.Appearence(image, stackpanel, app, appname, exefileimage);
                    AddListener(app, trash, trashname, options, exefileimage);
                }
                else if (trashname.EndsWith(".mp3"))
                {
                    MainWindow.Appearence(image, stackpanel, app, appname, mp3fileimage);
                    AddListener(app, trash, trashname, options, mp3fileimage);
                }
                else if (trashname.EndsWith(".wav"))
                {
                    MainWindow.Appearence(image, stackpanel, app, appname, wavfileimage);
                    AddListener(app, trash, trashname, options, wavfileimage);
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
                    switch(QueryDialog.ShowQueryDialog(trashname, "Deleted Item", options, Image, "Images/trashfull.png")) 
                    {
                        case 0:
                            File.Delete(trash); // delete selected
                            ReloadTrashBacket();
                            break;
                        case 1:
                            File.Move(trash, Path.Combine(MainWindow.CDesktopPath, trashname)); // rescue selected
                            ReloadTrashBacket();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.ErrorMessage($"An error occured while deleting {trashname}.\n" + ex.Message, NoteAppLogics.deleteerror);
                }
            };
        }

        private void TrashBacketReload_Wanted(object sender, RoutedEventArgs e)
        {
            ReloadTrashBacket();
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
