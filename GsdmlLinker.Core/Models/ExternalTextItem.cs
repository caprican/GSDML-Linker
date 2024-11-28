namespace GsdmlLinker.Core.Models;

public record ExternalTextItem
{
    public string Item { get; init; }
    public string? ID { get; }

    public ItemState State { get; set; } = ItemState.Original;

    public ExternalTextItem(string id, string text)
    {
        ID = id;
        Item = text;
    }
}
