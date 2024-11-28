using IODD = ISO15745.IODD;

using GsdmlLinker.Core.Models;

namespace GsdmlLinker.Core.IOL.Models;

public record DatatypeItem
{
    internal IODD.Datatypes.DatatypeT Item { get; init; }
    public string? ID => Item.Id;

    public ItemState State { get; set; } = Core.Models.ItemState.Original;

    public DatatypeItem(IODD.Datatypes.DatatypeT item)
    {
        Item = item;
    }
}
