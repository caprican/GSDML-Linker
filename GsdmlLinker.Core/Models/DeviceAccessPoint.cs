
namespace GsdmlLinker.Core.Models;

public record DeviceAccessPoint
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; set; }

    public string? ProductId { get; init; }

    public string? DNS {  get; init; }
    public string? PhysicalSlots { get; init; }
    public string? FixedInSlots { get; init; }

    public string? Symbol { get; init; }
    public string? Icon { get; init; }

    public DateTime? Version { get; init; }

    public Version? SoftwareRelease { get; set; }

    public List<Module>? Modules { get; init; }
}
