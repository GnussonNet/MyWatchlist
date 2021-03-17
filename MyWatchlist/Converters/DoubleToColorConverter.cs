using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MyWatchlist.Converters
{
    public class DoubleToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color header = Color.FromRgb(0, 93, 167);
            SolidColorBrush headerBrush = new SolidColorBrush(header);
            double num = (double)value;
            if (num < 0)
                return Brushes.Red;
            else if (num > 0)
                return headerBrush;
            else
                return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
