using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Contracts.Builders;

public interface IModuleBuilder
{
    public void CreateRecordParameters(Core.Models.Device? device, Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters, bool unloclDeviceId);

    public void CreateDataProcess(string indentNumber, IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>> ProcessDatas);

    public void BuildModule(Core.Models.Device device, string indentNumber, string categoryRef, string categoryVendor, string deviceName);

    public GSDML.DeviceProfile.ParameterRecordDataT? BuildRecordParameter(string textId, uint index, ushort transfertSequence, IGrouping<ushort, Core.Models.DeviceParameter>? variable, Dictionary<string, Core.Models.ExternalTextItem>? externalTextList);

    public List<Core.Models.DeviceParameter> GetRecordParameters(string deviceId);
    public List<Core.Models.DeviceParameter> GetPortParameters(string deviceId);

    public void UpdateModule(Core.Models.Device device, string indentNumber, string categoryRef, string categoryVendor, string deviceName);

    public void DeletModule(string moduleId);
}
