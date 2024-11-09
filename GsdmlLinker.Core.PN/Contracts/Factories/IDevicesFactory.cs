using System.Text.RegularExpressions;

using GsdmlLinker.Core.PN.Models;

namespace GsdmlLinker.Core.PN.Contracts.Factories;

public interface IDevicesFactory
{
    public Device CreateDevice(string filePath, Match? match);
    
    public Builders.IModuleBuilder? CreateModule(Core.Models.Device masterDevice, Core.Models.Device device);

    public Builders.IModuleBuilder? ReadModule(Core.Models.Device masterDevice);
}
