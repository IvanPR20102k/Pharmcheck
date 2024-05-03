using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Pharmcheck.Converters
{
    public class StatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                (byte)1 => new SolidColorBrush(Color.FromRgb(0, 205, 20)), //Зелёный
                (byte)2 => new SolidColorBrush(Color.FromRgb(205, 0, 0)), //Красный)
                _ => Brushes.Black,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
