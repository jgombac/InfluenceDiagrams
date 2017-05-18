
using System.Windows;

namespace InfluenceDiagrams
{
    /// <summary>
    /// Interaction logic for FilenameDialog.xaml
    /// </summary>
    public partial class FilenameDialog : Window
    {
        public FilenameDialog()
        {
            InitializeComponent();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Filename
        {
            get { return FilenameTb.Text; }
        }

    }
}
