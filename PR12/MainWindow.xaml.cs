using PR12;
using System.Windows;

namespace PR12
{
    public partial class MainWindow : Window
    {
        // Глобальный объект конфигурации
        public CarConfiguration Config { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Config = new CarConfiguration();

            // Запуск первого шага
            MainFrame.Navigate(new Step1Page(Config));
        }

        // Метод для обновления прогресс-бара, вызываемый из страниц
        public void UpdateProgress(int stepNumber)
        {
            pbProgress.Value = stepNumber;
            txtStepInfo.Text = $"Шаг {stepNumber} из 5";
        }
    }
}
