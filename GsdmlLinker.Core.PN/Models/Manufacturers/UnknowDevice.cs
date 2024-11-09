using System.Text.RegularExpressions;

using ISO15745.GSDML.DeviceProfile;

namespace GsdmlLinker.Core.PN.Models.Manufacturers;

public record UnknowDevice(string filePath, Match? match) : Device(filePath, match)
{
    private const string ManufactuereId = "";

    public override string ProfileParameterIndex => string.Empty;

    public override uint VendorIdSubIndex => 0;

    public override uint DeviceIdSubIndex => 0;

    public override string GetLastIndentNumber(List<string> identNumberList)
    {
        throw new NotImplementedException();
    }

    public override uint GetModuleDeviceId(ParameterRecordDataT[]? recordDatas) => 0;

    public override ushort GetModuleVendorId(ParameterRecordDataT[]? recordDatas) => 0;

    public override string SetModuleDeviceId(uint Id)
    {
        throw new NotImplementedException();
    }

    public override string SetModuleVendorId(ushort Id)
    {
        throw new NotImplementedException();
    }
}
