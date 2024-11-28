using System.ComponentModel;

using GsdmlLinker.Core.Contracts;

namespace GsdmlLinker.Core.Models;

public abstract record Device : /*IEditableObject,*/ IMementoable
{
    private Device? Original;

    //public bool Editing { get; set; } = false;

    public string VendorId { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string? FilePath { get; }
    public string? ManufacturerName { get; init; }
    public string? SchematicVersion { get; init; }
    public string? DeviceFamily { get; init; }
    public string? Name { get; init; }

    public string? FileHistory { get; set; }

    public string VendorName { get; init; } = string.Empty;

    public DateTime? Version { get; init; }

    public abstract string ProfileParameterIndex { get; }
    public abstract uint VendorIdSubIndex { get; }
    public abstract uint DeviceIdSubIndex { get; }

    public List<DeviceAccessPoint> DeviceAccessPoints { get; init; } = [];

    public string? Description { get; set; }

    public Dictionary<string, GraphicItem>? GraphicsList { get; init; }
    public Dictionary<string, ExternalTextItem>? ExternalTextList { get; init; }

    public List<Module> Modules { get; init; } = [];

    public Device(string filePath)
    {
        FilePath = filePath;
    }

    //public void BeginEdit()
    //{
    //    Original = this;
    //    Editing = true;
    //}

    //public void CancelEdit()
    //{
    //    Editing = false;

    //}

    //public void EndEdit()
    //{
    //    Editing = false;
    //}
}
