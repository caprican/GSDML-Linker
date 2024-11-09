namespace GsdmlLinker.Models;

public class MasterModule
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public List<MasterModule>? Modules { get; set; }

    public ushort VendorId { get; set; }

    public uint DeviceId { get; set; }

    public string? ProfinetDeviceId { get; set; }
}
