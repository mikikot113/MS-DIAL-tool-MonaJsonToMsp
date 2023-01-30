using System;
using System.Windows;
using System.Windows.Input;

namespace MonaJsonToMsp.Views
{
    /// <summary>
    /// helpWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            Uri uri = new Uri("/Views/HelpPage1.xaml", UriKind.Relative);
            frame.Source = uri;

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
