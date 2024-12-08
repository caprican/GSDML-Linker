using System.Text.RegularExpressions;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models.Manufacturers;

public record IfmDeviceV2(string filePath, Match? match) : IfmDevice(filePath, match)
{
    public const string AL140x = "0xAC6F";

    public override string ProfileParameterIndex => "47360";    // Profile Index=0xB900 (47360)

    public override uint VendorIdSubIndex => 10;
    public override uint DeviceIdSubIndex => 12;
}
