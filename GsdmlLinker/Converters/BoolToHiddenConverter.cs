using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GsdmlLinker.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool val && val ? Visibility.Hidden : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
