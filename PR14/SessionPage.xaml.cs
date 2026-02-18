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
    /// Логика взаимодействия для SessionPage.xaml
    /// </summary>
    public partial class SessionPage : Page
    {
        private Sessions _session;
        private int? _selectedSeat = null;
        public SessionPage(Sessions session)
        {
            InitializeComponent();
            _session = session;
            TxtHeader.Text = $"Сеанс: {_session.Movies.Title} | {_session.SessionDate}";
            LoadSeats();
        }

        private void LoadSeats()
        {
            var db = Manager.GetContext();

            var takenSeats = db.Tickets.Where(t => t.SessionId == _session.Id).Select(t => t.SeatNumber).ToList();

            for (int i = 1; i <= 20; i++)
            {
                Button btn = new Button();
                btn.Content = i.ToString();
                btn.Margin = new Thickness(5);
                btn.Tag = i;

                if (takenSeats.Contains(i))
                {
                    btn.IsEnabled = false;
                    btn.Background = Brushes.White;
                }
                else
                {
                    btn.Background = Brushes.Green;
                    btn.Click += SeatBtn_Click;
                }
                SeatContainer.Children.Add(btn);
            }
        }

        private void SeatBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            _selectedSeat = (int)btn.Tag;

            foreach (var child in SeatContainer.Children)
            {
                if (child is Button b && b.IsEnabled) b.Background = Brushes.Green;
            }

            btn.Background = Brushes.Gray;
            TxtPrice.Text = $"Цена: {_session.Price} руб.";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
            {
                Manager.MainFrame.GoBack();
            }
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSeat == null)
            {
                MessageBox.Show("Выберите место!");
                return;
            }
            Manager.MainFrame.Navigate(new BookingPage(_session, _selectedSeat.Value));
        }
    }
}
