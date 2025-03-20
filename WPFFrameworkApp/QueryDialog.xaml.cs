using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFrameworkApp
{
    /// <summary>
    /// QueryDialog.xaml etkileşim mantığı
    /// </summary>
    public partial class QueryDialog : Window
    {
        public short SelectedOption = -1; // -1 = not selected
        public QueryDialog()
        {
            InitializeComponent();
        }
        public static short ShowQueryDialog(
            string message,
            string title,
            string[] options,
            string Image = "Images/question2.png",
            string windowIcon = "Images/paperplane2.png")
        {
            QueryDialog dialog = new QueryDialog
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            foreach (string option in options) 
            {
                Button button = new Button
                {
                    Content = option,
                    Width = 75,
                    Margin = new Thickness(10, 0, 5, 0),
                    Background = Brushes.Transparent
                };
                dialog.QueryMessage.Text = message;
                dialog.ButtonPanel.Children.Add(button);
                dialog.ImageIcon.Source = new BitmapImage(new Uri(Image, UriKind.RelativeOrAbsolute));
                dialog.Icon = new BitmapImage(new Uri(windowIcon, UriKind.RelativeOrAbsolute));
                button.Click += (s, e) =>
                {
                    dialog.SelectedOption = (short)Array.IndexOf(options, option); // selected index will be SelectedOption
                    dialog.Close();
                };
            }
            dialog.ShowDialog();
            return dialog.SelectedOption;
        }
    }
}
