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
    /// Логика взаимодействия для MoviePage.xaml
    /// </summary>
    public partial class MoviePage : Page
    {
        private Movies _movie;
        public MoviePage(Movies movie)
        {
            InitializeComponent();
            _movie = movie;
            DataContext = _movie;


            TxtTitle.Text = _movie.Title;
            TxtDesc.Text = _movie.Description;
            TxtRating.Text = $"Рейтинг: {_movie.Rating}";

            var db = Manager.GetContext();
            LViewSessions.ItemsSource = db.Sessions.Where(s => s.MovieId == _movie.Id).ToList();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Manager.MainFrame.GoBack();

        private void LViewSessions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Manager.CurrentUser == null)
            {
                MessageBox.Show("Войдите или зарегистрируйтесь для покупки!");
                return;
            }

            if (LViewSessions.SelectedItem is Sessions selectedSession)
                Manager.MainFrame.Navigate(new SessionPage(selectedSession));
        }
    }
}
