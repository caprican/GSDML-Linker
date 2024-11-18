using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

using GsdmlLinker.Core.PN.Contracts.Factories;
using GsdmlLinker.Core.PN.Models;
using GsdmlLinker.Core.PN.Models.Manufacturers;

using GSDML = ISO15745.GSDML;
using GsdmlLinker.Core.PN.Contracts.Builders;

namespace GsdmlLinker.Core.PN.Factories;

public class DevicesFactory : IDevicesFactory
{
    public Device CreateDevice(string filePath, Match? match)
    {
        var serializer = new XmlSerializer(typeof(GSDML.DeviceProfile.ISO15745Profile));
        using var reader = XmlReader.Create(filePath);
        var device = serializer.Deserialize(reader) as GSDML.DeviceProfile.ISO15745Profile;
        var vendorId = device?.ProfileBody?.DeviceIdentity?.VendorID ?? string.Empty;
        var deviceId = device?.ProfileBody?.DeviceIdentity?.DeviceID ?? string.Empty;

        return vendorId switch 
        {
            BalluffDevice.ManufactuereId => new BalluffDevice(filePath, match),
            IfmDevice.ManufactuereId => deviceId switch
            {
                IfmDeviceV2.AL140x => new IfmDeviceV2(filePath, match),
                _ => new IfmDevice(filePath, match)
            },
            MurrElectronicDevice.ManufactuereId => new MurrElectronicDevice(filePath, match),
            _ => new UnknowDevice(filePath, match)
        };
    }

    public Contracts.Builders.IModuleBuilder? CreateModule(Core.Models.Device masterDevice) =>
        masterDevice.VendorId switch
        {
            BalluffDevice.ManufactuereId => new Builders.Manufacturers.BalluffModuleBuilder(masterDevice),
            IfmDevice.ManufactuereId => masterDevice.DeviceId switch
            {
                IfmDeviceV2.AL140x => new Builders.Manufacturers.IfmModuleBuilderV2(masterDevice),
                _ => new Builders.Manufacturers.IfmModuleBuilder(masterDevice)
            },
            MurrElectronicDevice.ManufactuereId => new Builders.Manufacturers.MurrElectronicModuleBuilder(masterDevice),
            _ => null
        };
}
