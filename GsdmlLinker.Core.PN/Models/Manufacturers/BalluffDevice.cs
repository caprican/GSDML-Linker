using System.Text.RegularExpressions;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models.Manufacturers;

public record BalluffDevice(string filePath, Match? match) : Device(filePath, match)
{
    public const string ManufactuereId = "0x0378";

    public override string ProfileParameterIndex => "3";
    public override uint VendorIdSubIndex => 0;
    public override uint DeviceIdSubIndex => 0;

    public override string GetLastIndentNumber(List<string> identList)
    {
        List<int> indentNumberList = [];
        identList.ForEach(mod =>
        {
            indentNumberList.Add(Convert.ToInt32(mod, 16));
        });

        var result = indentNumberList.Count > 0 ? indentNumberList.Max() + 1 : Convert.ToInt32("0x00000000", 16);

        return $"0x{result:X8}";
    }

    public override uint GetModuleDeviceId(GSDML.DeviceProfile.ParameterRecordDataT[]? recordDatas)
    {
        var ProfileParameter = recordDatas?.FirstOrDefault(param => param.Index == ProfileParameterIndex);
        var deviceRecord0 = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == 3);
        var deviceRecord1 = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == 4);
        var deviceRecord2 = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == 5);

        uint deviceId = 0;

        if(deviceRecord0 is not null && deviceRecord1 is not null && deviceRecord2 is not null)
        {
            var deviceIdx16 = Convert.ToInt16(deviceRecord0.DefaultValue).ToString("X2") + Convert.ToInt16(deviceRecord1.DefaultValue).ToString("X2") + Convert.ToInt16(deviceRecord2.DefaultValue).ToString("X2");
            deviceId = uint.Parse(deviceIdx16, System.Globalization.NumberStyles.HexNumber);
        }

        return deviceId;
    }

    public override ushort GetModuleVendorId(GSDML.DeviceProfile.ParameterRecordDataT[]? recordDatas)
    {
        var ProfileParameter = recordDatas?.FirstOrDefault(param => param.Index == ProfileParameterIndex);
        var vendorRecord0 = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == 1);
        var vendorRecord1 = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == 2);

        ushort vendorId = 0;
        if (vendorRecord0 is not null && vendorRecord1 is not null)
        {
            var deviceIdx16 = Convert.ToInt16(vendorRecord0.DefaultValue).ToString("X2") + Convert.ToInt16(vendorRecord1.DefaultValue).ToString("X2");
            vendorId = ushort.Parse(deviceIdx16, System.Globalization.NumberStyles.HexNumber);
        }

        return vendorId;
    }

    public override string SetModuleDeviceId(uint Id)
    {
        throw new NotImplementedException();
    }

    public override string SetModuleVendorId(ushort Id)
    {
        throw new NotImplementedException();
    }
}
