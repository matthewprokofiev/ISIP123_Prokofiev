using PR12;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PR12.Converters
{
    public class OptionsPriceConverter : IValueConverter
    {
        // Вычисление суммарной стоимости выбранных опций
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' здесь, предположительно, TotalPrice, но мы будем использовать параметр
            // 'parameter' должен содержать ссылку на объект CarConfiguration

            if (parameter is CarConfiguration config)
            {
                // Суммируем цены только тех опций, у которых IsSelected = true
                decimal optionsTotal = config.SelectedOptions
                    .Where(o => o.IsSelected)
                    .Sum(o => o.Price);

                return optionsTotal.ToString("C0"); // Форматируем в валютный формат
            }

            return "N/A";
        }

        // Обратный метод не используется, оставляем его пустым
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}