namespace GsdmlLinker.Contracts.Builders;

public interface IModuleBuilder
{
    public void CreateModule(Models.DeviceItem masterDevice, Models.DeviceItem slaveDevice, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters);

}
