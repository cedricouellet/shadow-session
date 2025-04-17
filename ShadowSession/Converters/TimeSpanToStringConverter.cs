using Humanizer;
using System.Globalization;
using System.Windows.Data;

namespace ShadowSession.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = value as TimeSpan?;

            if (timeSpan == null)
            {
                return string.Empty;
            }

            return timeSpan.Value.Humanize();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
