using System.Globalization;
using System.Windows.Data;

namespace GsdmlLinker.Converters;

public class ItemStateToTextIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Core.Models.ItemState itemState)
        {
            switch (itemState) 
            {
                case Core.Models.ItemState.None:
                    return string.Empty;
                case Core.Models.ItemState.Modified:
                    return "\uE70F";
                case Core.Models.ItemState.Deleted:
                    return "\uE711";
                case Core.Models.ItemState.Created:
                    return "\uE710";
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
