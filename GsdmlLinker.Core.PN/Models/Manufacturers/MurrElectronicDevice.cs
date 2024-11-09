using System.Text.RegularExpressions;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models.Manufacturers;

public record MurrElectronicDevice(string filePath, Match? match) : Device(filePath, match)
{
    public const string ManufactuereId = "0x012F";

    public override string ProfileParameterIndex => "47360";    // Profile Index=0xB900 (47360)
    public override uint VendorIdSubIndex => 10;
    public override uint DeviceIdSubIndex => 13;

    public override string GetLastIndentNumber(List<string> identList)
    {
        List<int> indentNumberList = [];
        identList.ForEach(mod =>
        {
            if (mod.StartsWith("0x40"))
            {
                indentNumberList.Add(Convert.ToInt32(mod, 16));
            }
        });

        var result = indentNumberList.Count > 0 ? indentNumberList.Max() + 1 : Convert.ToInt32("0x40000000", 16);

        return $"0x{result:X8}";
    }

    public override uint GetModuleDeviceId(GSDML.DeviceProfile.ParameterRecordDataT[]? recordDatas)
    {
        var ProfileParameter = recordDatas?.FirstOrDefault(param => param.Index == ProfileParameterIndex);
        var deviceRecord = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == DeviceIdSubIndex);

        uint deviceId = 0;
        if(deviceRecord is not null)
        {
            var index = string.Join("", deviceRecord.DefaultValue?.Replace("0x", "").Split(',') ?? []);
            deviceId = Convert.ToUInt32($"0x{index}", 16);
            //deviceId = Convert.ToUInt32();
        }
        return deviceId;
    }

    public override ushort GetModuleVendorId(GSDML.DeviceProfile.ParameterRecordDataT[]? recordDatas)
    {
        var ProfileParameter = recordDatas?.FirstOrDefault(param => param.Index == ProfileParameterIndex);
        var vendorRecord = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == VendorIdSubIndex);

        ushort vendorId = 0;
        if(vendorRecord is not null)
        {
            var index = string.Join("", vendorRecord.DefaultValue?.Replace("0x", "").Split(',') ?? []);
            vendorId = Convert.ToUInt16($"0x{index}", 16);
        }
        return vendorId;
    }

    public override string SetModuleDeviceId(uint Id) => $"0x{$"{Id:X4}"[..2]},0x{$"{Id:X4}".Substring(2, 2)}";

    public override string SetModuleVendorId(ushort Id) => $"0x{$"{Id:X4}"[..2]},0x{$"{Id:X4}".Substring(2, 2)}";
}
