using PR12;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PR12
{
    public partial class Step1Page : Page
    {
        private CarConfiguration _config;
        private List<CarModel> _models;

        public Step1Page(CarConfiguration config)
        {
            InitializeComponent();
            _config = config;
            _models = DataRepository.GetModels();

            cmbModel.ItemsSource = _models;

            // Восстановление состояния при возврате назад
            if (_config.SelectedModel != null)
            {
                // Находим модель в списке, совпадающую по имени
                foreach (var m in cmbModel.Items)
                {
                    if (((CarModel)m).Name == _config.SelectedModel.Name)
                        cmbModel.SelectedItem = m;
                }
            }

            // Обновляем прогресс в Main Window
            ((MainWindow)Application.Current.MainWindow).UpdateProgress(1);
        }

        private void CmbModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbModel.SelectedItem is CarModel selectedModel)
            {
                _config.SelectedModel = selectedModel;
                cmbEngine.ItemsSource = selectedModel.AvailableEngines;
                cmbEngine.IsEnabled = true;

                // Сброс двигателя при смене модели
                cmbEngine.SelectedItem = null;
                btnNext.IsEnabled = false;
            }
        }

        private void CmbEngine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEngine.SelectedItem is EngineType selectedEngine)
            {
                _config.SelectedEngine = selectedEngine;
                btnNext.IsEnabled = true;
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Step2Page(_config));
        }
    }
}