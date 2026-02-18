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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            UpdateMovies();

            if (Manager.CurrentUser != null)
                BtnLogin.Content = "Выход";
        }

        private void UpdateMovies()
        {
            if (LViewMovies == null || TxtSearch == null || ComboSort == null) return;

            var db = Manager.GetContext();
            var currentMovies = db.Movies.ToList();

            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
                currentMovies = currentMovies.Where(p => p.Title.ToLower().Contains(TxtSearch.Text.ToLower())).ToList();

            if (ComboSort.SelectedIndex == 1)
                currentMovies = currentMovies.OrderBy(p => p.Title).ToList();
            if (ComboSort.SelectedIndex == 2)
                currentMovies = currentMovies.OrderByDescending(p => p.Rating).ToList();

            LViewMovies.ItemsSource = currentMovies;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateMovies();
        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateMovies();

        private void LViewMovies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LViewMovies.SelectedItem is Movies selectedMovie)
                Manager.MainFrame.Navigate(new MoviePage(selectedMovie));
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentUser != null) Manager.MainFrame.Navigate(new ProfilePage());
            else MessageBox.Show("Сначала войдите в аккаунт");
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentUser != null)
            {
                Manager.CurrentUser = null;
                Manager.MainFrame.Navigate(new MainPage());
            }
            else
            {
                Manager.MainFrame.Navigate(new LoginPage());
            }
        }
    }
}
