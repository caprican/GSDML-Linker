namespace GsdmlLinker.Contracts.Builders;

public interface IModuleBuilder
{
    public void CreateModule(Models.DeviceItem masterDevice, Models.DeviceItem slaveDevice, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters, string? moduleID = null);

    public void DeletedModule(Models.DeviceItem masterDevice, string moduleId);
}
