using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WPFFrameworkApp
{
    /// <summary>
    /// InputDialog.xaml etkileşim mantığı
    /// </summary>
    public partial class InputDialog : Window
    {
        public string UserInput { get; private set; }
        public static bool Result { get; set; }
        public InputDialog()
        {
            InitializeComponent();
        }

        public static string ShowInputDialog(
            string message,
            string title,
            string ImagePath = "Images/question2.png",
            string WindowIconPath = "Images/paperplane2.png",
            string OKOption = "OK",
            string CancelOption = "Cancel")

        {
            InputDialog inputdialog = new InputDialog
            {
                Title = title,
                Icon = new BitmapImage(new Uri(WindowIconPath, UriKind.RelativeOrAbsolute)),
                SizeToContent = SizeToContent.WidthAndHeight, // frame.pack() in java
            };
            inputdialog.Message.Text = message;
            inputdialog.ImageIcon.Source = new BitmapImage(new Uri(ImagePath, UriKind.RelativeOrAbsolute));
            inputdialog.OKButton.Content = OKOption;
            inputdialog.CancelButton.Content = CancelOption;
            inputdialog.ShowDialog();
            return inputdialog.UserInput;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            UserInput = Input.Text.Replace("\"", "");
            Result = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UserInput = null;
            Result = false;
            Close();
        }
    }
}
