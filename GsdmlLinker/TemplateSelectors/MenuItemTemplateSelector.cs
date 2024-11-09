using System.Windows.Controls;
using System.Windows;

using MahApps.Metro.Controls;

namespace GsdmlLinker.TemplateSelectors;

public class MenuItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate GlyphDataTemplate { get; set; } = new DataTemplate();

    public DataTemplate ImageDataTemplate { get; set; } = new DataTemplate();

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is HamburgerMenuGlyphItem)
        {
            return GlyphDataTemplate;
        }

        if (item is HamburgerMenuImageItem)
        {
            return ImageDataTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}