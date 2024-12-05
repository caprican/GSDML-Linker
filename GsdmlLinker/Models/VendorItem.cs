using System.Collections.ObjectModel;

namespace GsdmlLinker.Models;

public record VendorItem
{
    public string Name { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;

    public bool CanEdit { get; init; } = false;

    public string Icon { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public ObservableCollection<DeviceItem>? Devices { get; set; }
}
