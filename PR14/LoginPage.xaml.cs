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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var db = Manager.GetContext();
            // Ищем пользователя в БД
            var user = db.Users.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == TxtPassword.Password);

            if (user != null)
            {
                Manager.CurrentUser = user; // Сохраняем вошедшего пользователя
                MessageBox.Show("Успешный вход!");
                Manager.MainFrame.Navigate(new MainPage());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }

        private void BtnReg_Click(object sender, RoutedEventArgs e) => Manager.MainFrame.Navigate(new RegisterPage());
        private void BtnBack_Click(object sender, RoutedEventArgs e) => Manager.MainFrame.GoBack();
    }
}
