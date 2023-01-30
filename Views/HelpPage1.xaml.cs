using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonaJsonToMsp.Views
{
    /// <summary>
    /// HelpPage1.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpPage1 : Page
    {
        public HelpPage1()
        {
            InitializeComponent();
        }

        private void helpOntology_Click(object sender, RoutedEventArgs e)
        {
            var page2 = new HelpPage2();
            NavigationService.Navigate(page2);
        }
    }
}
