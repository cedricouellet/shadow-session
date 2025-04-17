using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShadowSession.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = (bool)value;

            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;

            return visibility switch
            {
                Visibility.Visible => true,
                _ => false,
            };
        }
    }
}
