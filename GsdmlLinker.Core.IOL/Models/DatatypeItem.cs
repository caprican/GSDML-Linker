using GsdmlLinker.Core.Models;

using IO_Link.Models.Datatypes;

namespace GsdmlLinker.Core.IOL.Models;

public record DatatypeItem
{
    internal DatatypeT Item { get; init; }
    public string? ID => Item.Id;

    public ItemState State { get; set; } = Core.Models.ItemState.Original;

    public DatatypeItem(DatatypeT item)
    {
        Item = item;
    }
}
