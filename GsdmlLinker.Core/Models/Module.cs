
using System.Collections.ObjectModel;

namespace GsdmlLinker.Core.Models;

public record Module
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ProfinetDeviceId { get; init; }

    public string? VendorName { get; set; }

    public string? OrderNumber { get; set; }

    public string? HardwareRelease { get; set; }

    public string? SoftwareRelease { get; set; }

    public string? CategoryRef { get; set; }
    public string? CategoryRefText { get; set; }

    public string? SubCategoryRef { get; set; }
    public string? SubCategoryRefText { get; set; }

    public ushort VendorId { get; init; }
    public uint DeviceId { get; init; }
    public ObservableCollection<Module>? Submodules { get; set; }

    public ObservableCollection<Module>? SubmodulesCaterogies { get; set; }
}
