using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PR15
{
    public partial class MainWindow : Window
    {
        PCBuilderEntities db = new PCBuilderEntities();
        List<PartItem> allParts = new List<PartItem>();
        List<PartItem> currentAssembly = new List<PartItem>();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            ValidateAssembly();
        }
        private void LoadData()
        {
            try
            {
                ManufacturerFilter.ItemsSource = db.manufacturer_.ToList();
                var partsFromDb = db.basepart_.Include("manufacturer_").ToList();
                foreach (var p in partsFromDb)
                {
                    allParts.Add(new PartItem
                    {
                        Id = p.id,
                        Name = p.name,
                        Manufacturer = p.manufacturer_.name,
                        // Заполняем описание (можно брать из названия или характеристик)
                        Description = $"Комплектующее категории {p.parttype_.name}",
                        Price = (decimal)p.price,
                        ImagePath = p.image,
                        BasePart = p
                    });
                }
                PartsListView.ItemsSource = allParts;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void RefreshCart()
        {
            SelectedPartsList.ItemsSource = null;
            SelectedPartsList.ItemsSource = currentAssembly;
            TotalPriceText.Text = $"{currentAssembly.Sum(x => x.Price):N0} ₽";
            ValidateAssembly();
        }


        private void ValidateAssembly()
        {
            List<string> compatErrors = new List<string>();
            List<string> missingParts = new List<string>();

            // 1. Поиск всех компонентов по типам
            var cpu = currentAssembly.FirstOrDefault(x => x.BasePart.cpu_ != null)?.BasePart.cpu_;
            var mobo = currentAssembly.FirstOrDefault(x => x.BasePart.motherboard_ != null)?.BasePart.motherboard_;
            var gpu = currentAssembly.FirstOrDefault(x => x.BasePart.gpu_ != null)?.BasePart.gpu_;
            var ram = currentAssembly.FirstOrDefault(x => x.BasePart.ram_ != null)?.BasePart.ram_;
            var psu = currentAssembly.FirstOrDefault(x => x.BasePart.powersupply_ != null)?.BasePart.powersupply_;
            var pccase = currentAssembly.FirstOrDefault(x => x.BasePart.case_ != null)?.BasePart.case_;
            var storage = currentAssembly.FirstOrDefault(x => x.BasePart.storagedevice_ != null)?.BasePart.storagedevice_;
            var cooler = currentAssembly.FirstOrDefault(x => x.BasePart.processorcooler_ != null)?.BasePart.processorcooler_;

            // 2. Проверка наличия ОБЯЗАТЕЛЬНЫХ деталей (теперь их 8)
            if (cpu == null) missingParts.Add("Процессор");
            if (mobo == null) missingParts.Add("Мат. плата");
            if (gpu == null) missingParts.Add("Видеокарта");
            if (ram == null) missingParts.Add("ОЗУ");
            if (psu == null) missingParts.Add("Блок питания");
            if (pccase == null) missingParts.Add("Корпус");
            if (storage == null) missingParts.Add("Накопитель");
            if (cooler == null) missingParts.Add("Кулер"); // Добавлено!

            // 3. ПРОВЕРКИ СОВМЕСТИМОСТИ

            // Проверка сокета (Процессор + Мат.плата)
            if (cpu != null && mobo != null && cpu.socketid != mobo.socketid)
                compatErrors.Add("❌ Сокет процессора не совпадает с мат.платой.");

            // Проверка сокета КУЛЕРА (Кулер + Мат.плата)
            // В скрипте есть таблица socketprocessorcooler$, которая говорит, какие сокеты поддерживает кулер
            if (cooler != null && mobo != null)
            {
                bool isCompatible = db.socketprocessorcooler_.Any(x => x.processorcoolerid == cooler.id && x.socketid == mobo.socketid);
                if (!isCompatible)
                    compatErrors.Add("❌ Кулер не поддерживает сокет выбранной мат.платы.");
            }

            // Тип памяти (DDR)
            if (mobo != null && ram != null && mobo.memorytypeid != ram.memorytypeid)
                compatErrors.Add("❌ Тип памяти (DDR) не совпадает с материнской платой.");

            // Мощность БП
            if (psu != null && gpu != null && (gpu.recommendpower ?? 0) > psu.power)
                compatErrors.Add($"❌ Недостаточно мощности БП (нужно {(gpu.recommendpower ?? 0)}W).");

            // 4. ОТОБРАЖЕНИЕ СТАТУСОВ В ИНТЕРФЕЙСЕ

            // Список того, чего не хватает
            RequiredList.Text = missingParts.Count > 0 ? "Не хватает: " + string.Join(", ", missingParts) : "✅ Все основные компоненты выбраны";
            RequiredList.Foreground = missingParts.Count > 0 ? Brushes.DarkRed : Brushes.DarkGreen;

            // Ошибки совместимости
            if (compatErrors.Count > 0)
            {
                CompatibilityStatus.Text = string.Join("\n", compatErrors);
                CompatibilityStatus.Foreground = Brushes.Red;
            }
            else
            {
                CompatibilityStatus.Text = currentAssembly.Count > 0 ? "✅ Сборка совместима" : "Выберите комплектующие";
                CompatibilityStatus.Foreground = Brushes.Green;
            }

            // 5. АКТИВАЦИЯ КНОПКИ СОХРАНЕНИЯ
            bool isFullSet = missingParts.Count == 0;
            bool hasNoErrors = compatErrors.Count == 0;
            bool hasText = !string.IsNullOrWhiteSpace(AssemblyNameBox.Text) && !string.IsNullOrWhiteSpace(AuthorNameBox.Text);

            SaveBtn.IsEnabled = isFullSet && hasNoErrors && hasText;
        }

        // Обработчики событий
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag is PartItem newItem)
            {
                // ЛОГИКА ЗАМЕНЫ: Ищем, есть ли уже в сборке деталь такого же типа
                var existingItem = currentAssembly.FirstOrDefault(x =>
                    x.BasePart.parttypeid == newItem.BasePart.parttypeid);

                if (existingItem != null)
                {
                    currentAssembly.Remove(existingItem);
                }

                currentAssembly.Add(newItem);
                RefreshCart();
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag is PartItem item) { currentAssembly.Remove(item); RefreshCart(); }
        }

        private void InputFields_TextChanged(object sender, TextChangedEventArgs e) => ValidateAssembly();

        private void FilterChanged(object sender, EventArgs e)
        {
            var filtered = allParts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                filtered = filtered.Where(p => p.Name.ToLower().Contains(SearchBox.Text.ToLower()));
            if (ManufacturerFilter.SelectedItem is manufacturer_ m)
                filtered = filtered.Where(p => p.Manufacturer == m.name);
            PartsListView.ItemsSource = filtered.ToList();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = ""; ManufacturerFilter.SelectedItem = null; PartsListView.ItemsSource = allParts;
        }

        private void SaveAssembly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newAssembly = new assembly_ { name = AssemblyNameBox.Text, author = AuthorNameBox.Text };
                db.assembly_.Add(newAssembly);
                db.SaveChanges(); // Сохраняем, чтобы получить ID сборки

                foreach (var item in currentAssembly)
                {
                    db.partassembly_.Add(new partassembly_ { assemblyid = newAssembly.id, partid = item.Id });
                }
                db.SaveChanges();
                MessageBox.Show("Сборка успешно сохранена!");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка при сохранении: " + ex.Message); }
        }
    }
}