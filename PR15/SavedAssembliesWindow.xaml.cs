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
using System.Windows.Shapes;

namespace PR15
{
    /// <summary>
    /// Логика взаимодействия для SavedAssembliesWindow.xaml
    /// </summary>
    // SavedAssembliesWindow.xaml.cs
    public partial class SavedAssembliesWindow : Window
    {
        public SavedAssembliesWindow()
        {
            InitializeComponent();
            using (var db = new PCBuilderEntities())
            {
                AssembliesList.ItemsSource = db.assembly_.ToList();
            }
        }
    }
}

