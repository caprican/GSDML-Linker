using GsdmlLinker.Core.Models;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models;

public record ValueItem
{
    internal IEnumerable<GSDML.DeviceProfile.Assign> Assigments { get; init; }

    public string? ID { get; }

    public ItemState State { get; set; } = Core.Models.ItemState.Original;

    public ValueItem(string id, IEnumerable<GSDML.DeviceProfile.Assign> assigments)
    {
        Assigments = assigments;
        ID = id;
    }
}
