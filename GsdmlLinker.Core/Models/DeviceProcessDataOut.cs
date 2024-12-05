namespace GsdmlLinker.Core.Models;

public class DeviceProcessDataOut
{
    public string Name { get; set; } = string.Empty;

    public ushort BitLength { get; set; }
    public List<DeviceParameter>? ProcessData { get; set; }
}
