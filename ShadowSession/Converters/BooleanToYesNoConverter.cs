using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ShadowSession.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = bool.Parse(value.ToString() ?? "");

            return boolean ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var yesOrNoText = (string)value;

            return yesOrNoText.ToLower() switch
            {
                "yes" => true,
                "no" => false,
                _ => false,
            };
        }
    }
}
