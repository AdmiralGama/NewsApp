using System.Windows;

namespace Interface
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UpdatePopup : Window
    {
        public MainWindow parent;

        public UpdatePopup()
        {
            InitializeComponent();
        }

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            parent.UpdateReturn(this, periodBox.Text);
        }
    }
}
