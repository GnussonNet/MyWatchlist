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
            Color blue = Color.FromRgb(0, 93, 167);
            Color red = Color.FromRgb(167, 0, 0);
            SolidColorBrush blueBrush = new SolidColorBrush(blue);
            SolidColorBrush redBrush = new SolidColorBrush(red);
            double num = (double)value;
            if (num < 0)
                return redBrush;
            else if (num > 0)
                return blueBrush;
            else
                return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
