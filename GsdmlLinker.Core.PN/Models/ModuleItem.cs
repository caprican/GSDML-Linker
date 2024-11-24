using System.Diagnostics;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models;

[DebuggerDisplay("{Name}")]
public record ModuleItem : Core.Models.Module
{
    internal GSDML.DeviceProfile.ModuleItemT Item { get; init; }

    public string? ID => Item.ID;
    internal GSDML.DeviceProfile.ModuleInfoT? ModuleInfo => Item.ModuleInfo;
    internal GSDML.DeviceProfile.UseableSubmodulesTSubmoduleItemRef[]? UseableSubmodules => Item.UseableSubmodules;
    internal GSDML.DeviceProfile.BuiltInSubmoduleItemT[]? VirtualSubmoduleList => Item.VirtualSubmoduleList;
    internal string? PhysicalSubslots => Item.PhysicalSubslots;

    public ModuleItem(GSDML.DeviceProfile.ModuleItemT item)
    {
        Item = item;
        State = Core.Models.ItemState.Original;

        ProfinetDeviceId = Item.ID;

        VendorName = Item.ModuleInfo?.VendorName?.Value ?? string.Empty;
        OrderNumber = Item.ModuleInfo?.OrderNumber?.Value ?? string.Empty;
        HardwareRelease = Item.ModuleInfo?.HardwareRelease?.Value ?? string.Empty;
        SoftwareRelease = Item.ModuleInfo?.SoftwareRelease?.Value ?? string.Empty;
    }
}
