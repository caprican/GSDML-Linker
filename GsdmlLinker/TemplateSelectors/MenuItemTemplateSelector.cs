using System.Windows.Controls;
using System.Windows;

using MahApps.Metro.Controls;

namespace GsdmlLinker.TemplateSelectors;

public class MenuItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate GlyphDataTemplate { get; set; } = new DataTemplate();

    public DataTemplate ImageDataTemplate { get; set; } = new DataTemplate();
    public DataTemplate IconDataTemplate { get; set; } = new DataTemplate();

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        switch(item)
        {
            case HamburgerMenuGlyphItem:
                return GlyphDataTemplate;
            case HamburgerMenuImageItem:
                return ImageDataTemplate;
            case HamburgerMenuIconItem:
                return IconDataTemplate;
            default:
                return base.SelectTemplate(item, container);
        }
    }
}