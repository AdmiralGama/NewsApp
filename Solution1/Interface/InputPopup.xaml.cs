using System.Windows;

namespace Interface
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class InputPopup : Window
    {
        public MainWindow parent;

        public InputPopup()
        {
            InitializeComponent();
        }

        private void CreateClicked(object sender, RoutedEventArgs e)
        {
            parent.AddFeedReturn(this, nameBox.Text, uRLBox.Text);
        }
    }
}
