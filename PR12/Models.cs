using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PR12
{
    // Базовый класс для реализации уведомлений об изменениях (MVVM pattern)
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    // Простые модели данных
    public class CarModel
    {
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public List<EngineType> AvailableEngines { get; set; }
    }

    public class EngineType
    {
        public string Name { get; set; } // Например, "2.0L Turbo"
        public decimal ExtraPrice { get; set; }
    }

    public class CarColor
    {
        public string Name { get; set; }
        public decimal ExtraPrice { get; set; }
        public string HexCode { get; set; } // Для отображения цвета (опционально)
    }

    public class CarOption
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsSelected { get; set; } // Для привязки чекбокса
    }

    // ГЛАВНЫЙ КЛАСС КОНФИГУРАЦИИ
    public class CarConfiguration : ViewModelBase
    {
        private CarModel _selectedModel;
        private EngineType _selectedEngine;
        private CarColor _selectedColor;
        private int _creditTermMonths = 24; // Дефолт
        private double _downPaymentPercent = 20; // Дефолт %

        // Данные клиента
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientEmail { get; set; }

        public ObservableCollection<CarOption> SelectedOptions { get; set; } = new ObservableCollection<CarOption>();

        public CarModel SelectedModel
        {
            get => _selectedModel;
            set { _selectedModel = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); RecalculateCredit(); }
        }

        public EngineType SelectedEngine
        {
            get => _selectedEngine;
            set { _selectedEngine = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); RecalculateCredit(); }
        }

        public CarColor SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); RecalculateCredit(); }
        }

        // Параметры кредита
        public int CreditTermMonths
        {
            get => _creditTermMonths;
            set { _creditTermMonths = value; OnPropertyChanged(); RecalculateCredit(); }
        }

        public double DownPaymentPercent
        {
            get => _downPaymentPercent;
            set { _downPaymentPercent = value; OnPropertyChanged(); RecalculateCredit(); }
        }

        // Вычисляемые свойства для кредита
        private decimal _creditDownPaymentAmount;
        public decimal CreditDownPaymentAmount
        {
            get => _creditDownPaymentAmount;
            set { _creditDownPaymentAmount = value; OnPropertyChanged(); }
        }

        private decimal _creditAmount; // Тело кредита
        public decimal CreditAmount
        {
            get => _creditAmount;
            set { _creditAmount = value; OnPropertyChanged(); }
        }

        private decimal _monthlyPayment;
        public decimal MonthlyPayment
        {
            get => _monthlyPayment;
            set { _monthlyPayment = value; OnPropertyChanged(); }
        }

        public decimal TotalPrice
        {
            get
            {
                decimal total = 0;
                if (SelectedModel != null) total += SelectedModel.BasePrice;
                if (SelectedEngine != null) total += SelectedEngine.ExtraPrice;
                if (SelectedColor != null) total += SelectedColor.ExtraPrice;
                if (SelectedOptions != null) total += SelectedOptions.Sum(o => o.Price);
                return total;
            }
        }

        public void UpdateOptions(IEnumerable<CarOption> options)
        {
            SelectedOptions.Clear();
            foreach (var opt in options)
            {
                if (opt.IsSelected) SelectedOptions.Add(opt);
            }
            OnPropertyChanged(nameof(TotalPrice));
            RecalculateCredit();
        }

        private void RecalculateCredit()
        {
            // S = C - P
            decimal price = TotalPrice;
            CreditDownPaymentAmount = price * (decimal)(DownPaymentPercent / 100.0);
            CreditAmount = price - CreditDownPaymentAmount;

            // Расчет платежа
            // i = r / 100 / 12. Пусть ставка 15% годовых (захардкожена или можно вынести)
            double annualRate = 15.0;
            double i = (annualRate / 100.0) / 12.0;
            double n = CreditTermMonths;

            if (CreditAmount > 0)
            {
                // Формула: A = S * (i * (1+i)^n) / ((1+i)^n - 1)
                double coef = Math.Pow(1 + i, n);
                double payment = (double)CreditAmount * (i * coef) / (coef - 1);
                MonthlyPayment = (decimal)payment;
            }
            else
            {
                MonthlyPayment = 0;
            }
        }
    }

    // Статический репозиторий данных (Mock Data)
    public static class DataRepository
    {
        public static List<CarModel> GetModels()
        {
            return new List<CarModel>
            {
                new CarModel { Name = "Sedan Standard", BasePrice = 1500000, AvailableEngines = new List<EngineType> {
                    new EngineType { Name = "1.6L (100 л.с.)", ExtraPrice = 0 },
                    new EngineType { Name = "2.0L (150 л.с.)", ExtraPrice = 150000 }
                }},
                new CarModel { Name = "SUV Premium", BasePrice = 2500000, AvailableEngines = new List<EngineType> {
                    new EngineType { Name = "2.0L Turbo", ExtraPrice = 0 },
                    new EngineType { Name = "3.0L Diesel", ExtraPrice = 300000 }
                }},
                 new CarModel { Name = "Sport Coupe", BasePrice = 3200000, AvailableEngines = new List<EngineType> {
                    new EngineType { Name = "3.5L V6", ExtraPrice = 0 },
                    new EngineType { Name = "5.0L V8", ExtraPrice = 600000 }
                }}
            };
        }

        public static List<CarColor> GetColors()
        {
            return new List<CarColor>
            {
                new CarColor { Name = "Белый", ExtraPrice = 0, HexCode="#FFFFFF" },
                new CarColor { Name = "Черный Металлик", ExtraPrice = 20000, HexCode="#000000" },
                new CarColor { Name = "Красный Перламутр", ExtraPrice = 35000, HexCode="#FF0000" },
                new CarColor { Name = "Синий Космос", ExtraPrice = 25000, HexCode="#0000FF" }
            };
        }

        public static List<CarOption> GetOptions()
        {
            return new List<CarOption>
            {
                new CarOption { Name = "Кожаный салон", Price = 100000 },
                new CarOption { Name = "Панорамная крыша", Price = 80000 },
                new CarOption { Name = "Зимний пакет", Price = 45000 },
                new CarOption { Name = "Премиум аудиосистема", Price = 60000 }
            };
        }
    }
}
