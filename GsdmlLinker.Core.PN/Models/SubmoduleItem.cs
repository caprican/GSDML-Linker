using System.Diagnostics;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models;

[DebuggerDisplay("{Name}")]
public record SubmoduleItem : Core.Models.Module
{
    internal GSDML.DeviceProfile.SubmoduleItemT Item { get; init; }
    
    public string? ID => Item.ID;
    internal GSDML.DeviceProfile.SubmoduleItemBaseTRecordDataList? RecordDataList => Item.RecordDataList;
    internal GSDML.DeviceProfile.ModuleInfoT? ModuleInfo => Item.ModuleInfo;

    public SubmoduleItem(GSDML.DeviceProfile.SubmoduleItemT item)
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
