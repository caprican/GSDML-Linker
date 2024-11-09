using System.Windows;
using System.Windows.Controls;

namespace GsdmlLinker.TemplateSelectors;

public class ParameterTemplateSelector : DataTemplateSelector
{
    public DataTemplate BoolTemplate { get; set; } = new();
    public DataTemplate StringTemplate { get; set; } = new();
    public DataTemplate ListTemplate { get; set; } = new();
    public DataTemplate ValueTemplate { get; set; } = new();
    public DataTemplate FloatTemplate { get; set; } = new();
    public DataTemplate RecordTemplate { get; set; } = new();

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            Core.Models.DeviceParameter parameter => parameter.DataType switch
            {
                Core.Models.DeviceDatatypes.BooleanT => parameter.Values is null ? BoolTemplate : ListTemplate,
                Core.Models.DeviceDatatypes.OctetStringT => parameter.Values is null ? ValueTemplate : ListTemplate,
                Core.Models.DeviceDatatypes.UIntegerT => parameter.Values is null ? ValueTemplate : ListTemplate,
                Core.Models.DeviceDatatypes.IntegerT => parameter.Values is null ? ValueTemplate : ListTemplate,
                Core.Models.DeviceDatatypes.Float32T => FloatTemplate,
                Core.Models.DeviceDatatypes.StringT => StringTemplate,
                Core.Models.DeviceDatatypes.RecordT => RecordTemplate,
                _ => base.SelectTemplate(item, container)
            },
            _ => base.SelectTemplate(item, container),
        };
    }
}
