namespace GsdmlLinker.Core.IOL.Models;

public record DeviceVariantDescription
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? ProductId { get; init; }
    public string? Symbol { get; init; }
    public string? Icon { get; init; }
}
