using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace MonaJsonToMsp
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class MsgBox : UserControl
    {
        public MsgBox()
        {
            InitializeComponent();
        }

        public static Task<object?> Show(string text)
        {
            var dialog = new MsgBox();
            dialog.DataContext = new { DialogText = text };
            return DialogHost.Show(dialog);
        }
    }
}
