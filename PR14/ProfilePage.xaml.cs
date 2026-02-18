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

namespace PR14
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            TxtUser.Text = $"Пользователь: {Manager.CurrentUser.Login}";

            var db = Manager.GetContext();

            DGridTickets.ItemsSource = db.Tickets
                .Include("Sessions.Movies")
                .Where(t => t.UserId == Manager.CurrentUser.Id)
                .ToList();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e) => Manager.MainFrame.Navigate(new MainPage());

    }
}
