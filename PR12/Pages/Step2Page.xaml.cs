using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PR12
{
    public partial class Step2Page : Page
    {
        private CarConfiguration _config;
        private List<CarOption> _uiOptions;

        public Step2Page(CarConfiguration config)
        {
            InitializeComponent();
            _config = config;

            // Загружаем цвета
            cmbColor.ItemsSource = DataRepository.GetColors();

            // Создаем копию опций для UI, чтобы привязать чекбоксы
            _uiOptions = DataRepository.GetOptions();

            // Восстановление выбранных опций
            foreach (var opt in _uiOptions)
            {
                foreach (var selected in _config.SelectedOptions)
                {
                    if (selected.Name == opt.Name) opt.IsSelected = true;
                }
            }
            lstOptions.ItemsSource = _uiOptions;

            // Восстановление цвета
            if (_config.SelectedColor != null)
            {
                foreach (var c in cmbColor.Items)
                {
                    if (((CarColor)c).Name == _config.SelectedColor.Name)
                        cmbColor.SelectedItem = c;
                }
            }

            ((MainWindow)Application.Current.MainWindow).UpdateProgress(2);
        }

        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbColor.SelectedItem is CarColor color)
            {
                _config.SelectedColor = color;
                btnNext.IsEnabled = true;
            }
        }

        private void Option_CheckChanged(object sender, RoutedEventArgs e)
        {
            // Обработчик просто для возможной логики, основные данные обновляются при переходе Next
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            SaveOptions(); // Сохраняем перед уходом назад
            NavigationService.GoBack();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            SaveOptions();
            NavigationService.Navigate(new Step3Page(_config));
        }

        private void SaveOptions()
        {
            _config.UpdateOptions(_uiOptions);
        }
    }
}
