using System;
using System.Linq;
using System.Windows;

namespace PR15
{
    public partial class SavedAssembliesWindow : Window
    {
        public SavedAssembliesWindow()
        {
            InitializeComponent();
            LoadAssemblies();
        }

        private void LoadAssemblies()
        {
            try
            {
                using (var db = new PCBuilderEntities())
                {
                    // Подгружаем сборки вместе со связующей таблицей и самими деталями
                    var assembliesFromDb = db.assembly_
                        .Include("partassembly_")
                        .Include("partassembly_.basepart_")
                        .ToList();

                    // Формируем список для отображения в таблице
                    var displayList = assembliesFromDb.Select(a => new AssemblyDisplayItem
                    {
                        Author = a.author,
                        Name = a.name,
                        // Вытаскиваем имена всех деталей из сборки и склеиваем их через запятую
                        Contents = string.Join(", ", a.partassembly_.Select(pa => pa.basepart_.name))
                    }).ToList();

                    AssembliesListView.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке сборок: " + ex.Message);
            }
        }

        // Кнопка возврата просто закрывает текущее окно
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Вспомогательный класс для удобной привязки к ListView
    public class AssemblyDisplayItem
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public string Contents { get; set; }
    }
}