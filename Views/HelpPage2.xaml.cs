using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MonaJsonToMsp.Views
{
    /// <summary>
    /// helpOntology.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpPage2 : Page
    {
        public HelpPage2()
        {
            InitializeComponent();
        }

        private void helpOntology_Click(object sender, RoutedEventArgs e)
        {
            var page1 = new HelpPage1();
            NavigationService.Navigate(page1);

        }
    }
}
