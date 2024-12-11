using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GsdmlLinker.Converters;

public class LockVisibiltyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Core.Models.DeviceParameter deviceParameter)
        {
            return !deviceParameter.IsSelected || deviceParameter?.Subindex > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
