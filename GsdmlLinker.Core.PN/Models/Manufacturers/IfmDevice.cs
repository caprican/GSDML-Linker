using System.Text.RegularExpressions;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models.Manufacturers;

public record IfmDevice(string filePath, Match? match) : Device(filePath, match)
{
    public const string ManufactuereId = "0x0136";

    public override string ProfileParameterIndex => "45312";    // Profile Index=0xB100 (45312)
    public override uint VendorIdSubIndex => 5;
    public override uint DeviceIdSubIndex => 7;

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
            deviceId = Convert.ToUInt32(deviceRecord?.DefaultValue);
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
            vendorId = Convert.ToUInt16(vendorRecord?.DefaultValue);
        }

        return vendorId;
    }

    public override string SetModuleDeviceId(uint Id) => $"{Id}";

    public override string SetModuleVendorId(ushort Id) => $"{Id}";
}
