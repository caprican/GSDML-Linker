
namespace GsdmlLinker.Core.Models;

public record GraphicItem
{
    public string Item { get; init; }

    public string? ID { get; }

    public ItemState State { get; set; } = Core.Models.ItemState.Original;

    public GraphicItem(string id, string graphic)
    {
        Item = graphic;
        ID = id;
    }
}
