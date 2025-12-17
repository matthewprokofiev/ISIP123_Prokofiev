using System.Windows;
using System.Windows.Controls;

namespace PR12
{
    public partial class Step3Page : Page
    {
        private CarConfiguration _config;

        public Step3Page(CarConfiguration config)
        {
            InitializeComponent();
            _config = config;
            this.DataContext = _config; // Привязка данных для отображения

            ((MainWindow)Application.Current.MainWindow).UpdateProgress(3);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step4Page(_config));
        }
    }
}
