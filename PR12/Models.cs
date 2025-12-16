using PR12;
using PR12.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR12
{
    // --- Справочные данные ---
    public class CarModel
    {
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public string ImagePath { get; set; } // Для расширения, если нужно
        public override string ToString() => $"{Name} (База: {BasePrice:C0})";
    }

    public class EngineType
    {
        public string Name { get; set; }
        public decimal PriceModifier { get; set; }
        public override string ToString() => $"{Name} (+{PriceModifier:C0})";
    }

    public class CarColor
    {
        public string Name { get; set; }
        public string HexCode { get; set; } // Для отображения цвета
        public decimal PriceModifier { get; set; }
    }

    public class CarOption : ObservableObject
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        // Поле для UI (выбрана опция или нет)
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    // --- Единый объект конфигурации ---
    public class CarConfiguration : ObservableObject
    {
        private CarModel _selectedModel;
        private EngineType _selectedEngine;
        private CarColor _selectedColor;
        private int _creditTermMonths = 36; // Default
        private double _initialPaymentPercent = 20; // Default %

        // Контактные данные
        private string _customerName;
        private string _customerPhone;
        private string _customerEmail;

        public CarConfiguration()
        {
            // Инициализация коллекций опций будет в VM
            SelectedOptions = new ObservableCollection<CarOption>();
        }

        public CarModel SelectedModel
        {
            get => _selectedModel;
            set { _selectedModel = value; OnPropertyChanged(); Recalculate(); }
        }

        public EngineType SelectedEngine
        {
            get => _selectedEngine;
            set { _selectedEngine = value; OnPropertyChanged(); Recalculate(); }
        }

        public CarColor SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(); Recalculate(); }
        }

        // Коллекция всех доступных опций, где мы будем менять флаг IsSelected
        public ObservableCollection<CarOption> SelectedOptions { get; set; }

        // --- Финансы ---
        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            private set { _totalPrice = value; OnPropertyChanged(); RecalculateCredit(); }
        }

        // Кредитные параметры
        public int CreditTermMonths
        {
            get => _creditTermMonths;
            set { _creditTermMonths = value; OnPropertyChanged(); RecalculateCredit(); }
        }

        public double InitialPaymentPercent
        {
            get => _initialPaymentPercent;
            set { _initialPaymentPercent = value; OnPropertyChanged(); RecalculateCredit(); }
        }

        // Результаты расчета кредита
        private decimal _downPaymentAmount;
        public decimal DownPaymentAmount
        {
            get => _downPaymentAmount;
            set { _downPaymentAmount = value; OnPropertyChanged(); }
        }

        private decimal _creditAmount;
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

        // --- Контакты ---
        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; OnPropertyChanged(); }
        }
        public string CustomerPhone
        {
            get => _customerPhone;
            set { _customerPhone = value; OnPropertyChanged(); }
        }
        public string CustomerEmail
        {
            get => _customerEmail;
            set { _customerEmail = value; OnPropertyChanged(); }
        }

        // --- Логика расчетов ---

        public void Recalculate()
        {
            decimal price = 0;
            if (SelectedModel != null) price += SelectedModel.BasePrice;
            if (SelectedEngine != null) price += SelectedEngine.PriceModifier;
            if (SelectedColor != null) price += SelectedColor.PriceModifier;

            if (SelectedOptions != null)
            {
                foreach (var opt in SelectedOptions)
                {
                    if (opt.IsSelected) price += opt.Price;
                }
            }

            TotalPrice = price;
        }

        private void RecalculateCredit()
        {
            if (TotalPrice == 0) return;

            // 1. Первоначальный взнос
            DownPaymentAmount = TotalPrice * (decimal)(InitialPaymentPercent / 100.0);

            // 2. Сумма кредита (S = C - P)
            CreditAmount = TotalPrice - DownPaymentAmount;

            // 3. Ежемесячный платеж
            // Формула: A = S * (i * (1+i)^n) / ((1+i)^n - 1)
            // r - годовая ставка. Допустим 15% (0.15)
            double annualRate = 15.0;
            double r = annualRate;

            // i = r / 100 / 12
            double i = r / 100.0 / 12.0;
            double n = CreditTermMonths;
            double S = (double)CreditAmount;

            if (S <= 0)
            {
                MonthlyPayment = 0;
                return;
            }

            double numerator = i * Math.Pow(1 + i, n);
            double denominator = Math.Pow(1 + i, n) - 1;

            double A = S * (numerator / denominator);

            MonthlyPayment = (decimal)A;
        }
    }
}
