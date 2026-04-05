namespace GsdmlLinker.Core.Models;

public record DeviceDataStructure
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DeviceDatatypes? DataType { get; init; } =  DeviceDatatypes.BooleanT;
    public int? BitOffset { get; init; }
    public int? BitLength { get; init; }

    public double? Order { get; set; }
}
