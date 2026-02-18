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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Простая валидация
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (TxtPassword.Password != TxtPasswordConfirm.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            var db = Manager.GetContext();

            // Проверка на уникальность логина
            if (db.Users.Any(u => u.Login == TxtLogin.Text))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!");
                return;
            }

            // Создаем нового пользователя
            Users newUser = new Users
            {
                Login = TxtLogin.Text,
                Password = TxtPassword.Password,
                Email = TxtEmail.Text
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            MessageBox.Show("Регистрация завершена!");
            Manager.MainFrame.Navigate(new LoginPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Manager.MainFrame.GoBack();
    }
}
