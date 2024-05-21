using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Pharmcheck.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                (byte)0 => Brushes.Gray,
                (byte)1 => new SolidColorBrush(Color.FromRgb(0, 205, 20)), //Зелёный
                (byte)2 => new SolidColorBrush(Color.FromRgb(205, 0, 0)), //Красный
                (byte)3 => new SolidColorBrush(Color.FromRgb(255, 248, 75)), //Жёлтый
                _ => Brushes.Blue
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
