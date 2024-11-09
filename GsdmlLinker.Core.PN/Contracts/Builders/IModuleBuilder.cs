using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Contracts.Builders;

public interface IModuleBuilder
{
    public void CreateRecordParameters(Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters);

    public void CreateDataProcess(string indentNumber, IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>> ProcessDatas);

    public void BuildModule(string indentNumber, string categoryRef, string categoryVendor, string deviceName);

    public GSDML.DeviceProfile.ParameterRecordDataT? BuildRecordParameter(string textId, uint index, ushort transfertSequence, IGrouping<ushort, Core.Models.DeviceParameter>? variable);

    public List<Core.Models.DeviceParameter> ReadRecordParameter(/*GSDML.DeviceProfile.ParameterRecordDataT parameterRecordData*/ string deviceId);
}
