using System.Windows;
using System.Windows.Controls;

namespace GsdmlLinker.TemplateSelectors;

public class ProcessDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate ColumnDataTemplate { get; set; } = new DataTemplate();
    public DataTemplate RowDataTemplate { get; set; } = new DataTemplate();
    public DataTemplate ItemDataTemplate { get; set; } = new DataTemplate();

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            Models.ProcessDataColumn => ColumnDataTemplate,
            Models.ProcessDataRow => RowDataTemplate,
            Models.ProcessDataItem => ItemDataTemplate,
            _ => base.SelectTemplate(item, container),
        };
    }
}
