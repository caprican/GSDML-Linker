
using GsdmlLinker.Core.Models;

namespace GsdmlLinker.Models;

public struct ProcessDataItem : ProcessDataBase
{
    public string Name { get; set; }
    public DeviceDatatypes? Datatype { get; set; }
    public string Color { get; set; }
}
