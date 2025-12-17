using System.Windows;
using System.Windows.Controls;

namespace PR12
{
    public partial class Step4Page : Page
    {
        private CarConfiguration _config;

        public Step4Page(CarConfiguration config)
        {
            InitializeComponent();
            _config = config;
            this.DataContext = _config;

            ((MainWindow)Application.Current.MainWindow).UpdateProgress(4);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step5Page(_config));
        }
    }
}
