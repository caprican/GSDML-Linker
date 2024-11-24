using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GsdmlLinker.Core.Models;

[DebuggerDisplay("{Name}")]
public record Module
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ushort VendorId { get; set; }
    public uint DeviceId { get; set; }
    public string? ProfinetDeviceId { get; init; }

    public string? VendorName { get; set; }

    public string? OrderNumber { get; set; }

    public string? HardwareRelease { get; set; }
    public string? SoftwareRelease { get; set; }

    public string? CategoryRef { get; set; }
    public string? CategoryRefText { get; set; }

    public string? SubCategoryRef { get; set; }
    public string? SubCategoryRefText { get; set; }

    public ItemState State { get; set; }

    public ObservableCollection<Module>? Submodules { get; set; }
}
