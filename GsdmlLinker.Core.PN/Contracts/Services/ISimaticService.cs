namespace GsdmlLinker.Core.PN.Contracts.Services;

public interface ISimaticService
{
    public void CreateUdtFile(string path, string udtName, List<Core.Models.DeviceDataStructure> dataStructures);
}
