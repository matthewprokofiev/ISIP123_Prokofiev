using PR12;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PR12
{
    public partial class Step5Page : Page
    {
        private CarConfiguration _config;
        private bool _isDataEntered = false;

        public Step5Page(CarConfiguration config)
        {
            InitializeComponent();
            _config = config;

            // Восстановление данных, если были введены ранее
            txtName.Text = _config.ClientName;
            txtPhone.Text = _config.ClientPhone;
            txtEmail.Text = _config.ClientEmail;

            UpdateSummary();
            ((MainWindow)Application.Current.MainWindow).UpdateProgress(5);
        }

        private void UpdateSummary()
        {
            txtSummary.Text = $"{_config.SelectedModel?.Name}, {_config.SelectedEngine?.Name}, Цвет: {_config.SelectedColor?.Name}\n" +
                              $"Стоимость: {_config.TotalPrice:C}\n" +
                              $"Кредит: {_config.MonthlyPayment:C}/мес на {_config.CreditTermMonths} мес.";
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isDataEntered = !string.IsNullOrWhiteSpace(txtName.Text) ||
                             !string.IsNullOrWhiteSpace(txtPhone.Text) ||
                             !string.IsNullOrWhiteSpace(txtEmail.Text);

            ValidateInputs();
        }

        private void ValidateInputs()
        {
            bool isNameValid = !string.IsNullOrWhiteSpace(txtName.Text) && txtName.Text.Length > 2;

            // Телефон только цифры
            bool isPhoneValid = !string.IsNullOrWhiteSpace(txtPhone.Text) && txtPhone.Text.All(char.IsDigit) && txtPhone.Text.Length >= 10;

            // Простая проверка email
            bool isEmailValid = !string.IsNullOrWhiteSpace(txtEmail.Text) && txtEmail.Text.Contains("@") && txtEmail.Text.Contains(".");

            btnSubmit.IsEnabled = isNameValid && isPhoneValid && isEmailValid;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_isDataEntered)
            {
                var result = MessageBox.Show("Вы ввели данные. При возврате назад они могут быть потеряны. Продолжить?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            // Сохраняем черновик
            _config.ClientName = txtName.Text;
            _config.ClientPhone = txtPhone.Text;
            _config.ClientEmail = txtEmail.Text;

            NavigationService.GoBack();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            _config.ClientName = txtName.Text;
            _config.ClientPhone = txtPhone.Text;
            _config.ClientEmail = txtEmail.Text;

            MessageBox.Show($"Спасибо, {_config.ClientName}!\nВаша заявка на {_config.SelectedModel.Name} успешно оформлена.\n" +
                            $"Менеджер свяжется с вами по номеру {_config.ClientPhone}.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            Application.Current.Shutdown();
        }
    }
}
