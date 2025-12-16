using PR12;
using PR12.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PR12
{
    public class MainViewModel : ObservableObject
    {
        // Данные для заполнения списков
        public ObservableCollection<CarModel> AvailableModels { get; set; }
        public ObservableCollection<EngineType> AvailableEngines { get; set; }
        public ObservableCollection<CarColor> AvailableColors { get; set; }

        // Основной объект конфигурации
        public CarConfiguration Config { get; set; }

        // Управление навигацией
        private int _currentStepIndex; // 0 to 4
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                _currentStepIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentStepDisplay)); // Для ProgressBar (1-5)
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
                OnPropertyChanged(nameof(IsStep3));
                OnPropertyChanged(nameof(IsStep4));
                OnPropertyChanged(nameof(IsStep5));
            }
        }
        public int CurrentStepDisplay => CurrentStepIndex + 1;

        // Свойства видимости для переключения View (простой способ без Frame)
        public bool IsStep1 => CurrentStepIndex == 0;
        public bool IsStep2 => CurrentStepIndex == 1;
        public bool IsStep3 => CurrentStepIndex == 2;
        public bool IsStep4 => CurrentStepIndex == 3;
        public bool IsStep5 => CurrentStepIndex == 4;

        // Команды
        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand OptionChangedCommand { get; }

        public MainViewModel()
        {
            LoadData();
            Config = new CarConfiguration();

            // Заполняем опции в конфиг, чтобы привязать чекбоксы
            var optionsData = GetOptionsData();
            foreach (var opt in optionsData) Config.SelectedOptions.Add(opt);

            CurrentStepIndex = 0;
            Config.Recalculate(); // Начальный расчет

            NextCommand = new RelayCommand(GoNext, CanGoNext);
            BackCommand = new RelayCommand(GoBack, CanGoBack);
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
            OptionChangedCommand = new RelayCommand(o => Config.Recalculate());
        }

        private void LoadData()
        {
            AvailableModels = new ObservableCollection<CarModel>
            {
                new CarModel { Name = "Sedan Standard", BasePrice = 1500000 },
                new CarModel { Name = "SUV Family", BasePrice = 2200000 },
                new CarModel { Name = "Sport GT", BasePrice = 3500000 }
            };

            AvailableEngines = new ObservableCollection<EngineType>
            {
                new EngineType { Name = "1.6L Эко (110 л.с.)", PriceModifier = 0 },
                new EngineType { Name = "2.0L Турбо (180 л.с.)", PriceModifier = 150000 },
                new EngineType { Name = "3.5L V6 (249 л.с.)", PriceModifier = 400000 }
            };

            AvailableColors = new ObservableCollection<CarColor>
            {
                new CarColor { Name = "Белый", HexCode = "#FFFFFF", PriceModifier = 0 },
                new CarColor { Name = "Черный Металлик", HexCode = "#000000", PriceModifier = 25000 },
                new CarColor { Name = "Красный Перламутр", HexCode = "#FF0000", PriceModifier = 45000 },
                new CarColor { Name = "Синий Космос", HexCode = "#0000FF", PriceModifier = 35000 }
            };
        }

        private ObservableCollection<CarOption> GetOptionsData()
        {
            return new ObservableCollection<CarOption>
            {
                new CarOption { Name = "Зимний пакет (подогрев руля/стекол)", Price = 50000 },
                new CarOption { Name = "Кожаный салон", Price = 120000 },
                new CarOption { Name = "Панорамная крыша", Price = 90000 },
                new CarOption { Name = "Аудиосистема Premium", Price = 75000 },
                new CarOption { Name = "Система автопарковки", Price = 40000 }
            };
        }

        private bool CanGoNext(object obj)
        {
            // Валидация перед переходом
            if (CurrentStepIndex == 0)
                return Config.SelectedModel != null && Config.SelectedEngine != null;

            if (CurrentStepIndex == 1)
                return Config.SelectedColor != null;

            return CurrentStepIndex < 4;
        }

        private void GoNext(object obj)
        {
            if (CurrentStepIndex < 4) CurrentStepIndex++;
        }

        private bool CanGoBack(object obj)
        {
            // На шаге 5 проверяем, если пользователь ввел данные, предупреждаем
            return CurrentStepIndex > 0;
        }

        private void GoBack(object obj)
        {
            if (CurrentStepIndex == 4)
            {
                // Проверка на потерю данных
                if (!string.IsNullOrWhiteSpace(Config.CustomerName) ||
                    !string.IsNullOrWhiteSpace(Config.CustomerPhone))
                {
                    var res = MessageBox.Show("Вы ввели данные заявки. При возврате назад они могут быть не сохранены (логически). Вернуться?",
                        "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No) return;
                }
            }
            if (CurrentStepIndex > 0) CurrentStepIndex--;
        }

        private bool CanSubmit(object obj)
        {
            if (string.IsNullOrWhiteSpace(Config.CustomerName) || Config.CustomerName.Length < 2) return false;

            if (string.IsNullOrWhiteSpace(Config.CustomerPhone)) return false;
            // Простейшая проверка телефона (только цифры)
            if (!Regex.IsMatch(Config.CustomerPhone, @"^\d+$")) return false;

            if (string.IsNullOrWhiteSpace(Config.CustomerEmail) || !Config.CustomerEmail.Contains("@")) return false;

            return true;
        }

        private void Submit(object obj)
        {
            MessageBox.Show($"Заявка успешно оформлена!\n\nКлиент: {Config.CustomerName}\nАвто: {Config.SelectedModel.Name}\nИтого: {Config.TotalPrice:C0}\n\nСпасибо за выбор нашего салона.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            Application.Current.Shutdown();
        }
    }
}
