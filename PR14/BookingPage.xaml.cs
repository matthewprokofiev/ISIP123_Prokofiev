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
    /// Логика взаимодействия для BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private Sessions _session;
        private int _seat;
        public BookingPage(Sessions session, int seat)
        {
            InitializeComponent();
            _session = session;
            _seat = seat;

            TxtMovieTitle.Text = _session.Movies.Title;
            TxtHall.Text = _session.Halls.Name;
            TxtDateTime.Text = _session.SessionDate.ToString();
            TxtSeat.Text = _seat.ToString();
            TxtTotalPrice.Text = _session.Price.ToString();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var db = Manager.GetContext();
            Tickets newTicket = new Tickets
            {
                UserId = Manager.CurrentUser.Id,
                SessionId = _session.Id,
                SeatNumber = _seat,
                PurchaseDate = DateTime.Now
            };

            db.Tickets.Add(newTicket);
            db.SaveChanges();

            MessageBox.Show("Билет куплен!");
            Manager.MainFrame.Navigate(new MainPage());
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        { 
            if (Manager.MainFrame.CanGoBack)
            {
                Manager.MainFrame.GoBack();
            }
        }
        }
}
