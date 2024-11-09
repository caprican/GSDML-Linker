using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

using GsdmlLinker.Core.PN.Contracts.Factories;
using GsdmlLinker.Core.PN.Models;
using GsdmlLinker.Core.PN.Models.Manufacturers;

using GSDML = ISO15745.GSDML;

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

    public Contracts.Builders.IModuleBuilder? CreateModule(Core.Models.Device masterDevice, Core.Models.Device device) =>
        masterDevice.VendorId switch
        {
            BalluffDevice.ManufactuereId => new Builders.Manufacturers.BalluffModuleBuilder(masterDevice, device),
            IfmDevice.ManufactuereId => masterDevice.DeviceId switch
            {
                IfmDeviceV2.AL140x => new Builders.Manufacturers.IfmModuleBuilderV2(masterDevice, device),
                _ => new Builders.Manufacturers.IfmModuleBuilder(masterDevice, device)
            },
            MurrElectronicDevice.ManufactuereId => new Builders.Manufacturers.MurrElectronicModuleBuilder(masterDevice, device),
            _ => null
        };

    public Contracts.Builders.IModuleBuilder? ReadModule(Core.Models.Device masterDevice) =>
        masterDevice.VendorId switch
        {
            BalluffDevice.ManufactuereId => new Builders.Manufacturers.BalluffModuleBuilder(masterDevice, null),
            IfmDevice.ManufactuereId => masterDevice.DeviceId switch
            {
                IfmDeviceV2.AL140x => new Builders.Manufacturers.IfmModuleBuilderV2(masterDevice, null),
                _ => new Builders.Manufacturers.IfmModuleBuilder(masterDevice, null)
            },
            MurrElectronicDevice.ManufactuereId => new Builders.Manufacturers.MurrElectronicModuleBuilder(masterDevice, null),
            _ => null
        };
}
