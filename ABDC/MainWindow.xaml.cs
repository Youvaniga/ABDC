using System.Windows;

namespace ABDC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnNUBEAccount_Click(object sender, RoutedEventArgs e)
        {
            frmNubeAccount frm = new frmNubeAccount();
            frm.ShowDialog();
        }

        private void btnFMCG_Click(object sender, RoutedEventArgs e)
        {
            frmFMCG frm = new frmFMCG();
            frm.ShowDialog();
        }
    }
}
