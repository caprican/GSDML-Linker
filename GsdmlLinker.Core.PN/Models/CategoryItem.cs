using GsdmlLinker.Core.Models;

namespace GsdmlLinker.Core.PN.Models;

public record CategoryItem
{
    public string Item { get;init;}
   
    public string? ID { get; }

    public ItemState State { get; set; } = Core.Models.ItemState.Original;

    public CategoryItem(string id, string categoryName)
    {
        Item = categoryName;
        ID = id;
    }
}
