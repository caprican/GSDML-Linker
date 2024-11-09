namespace GsdmlLinker.Core.Models;

public class DeviceProcessData
{
    public Condition? Condition { get; set; }

    public DeviceProcessDataIn? ProcessDataIn { get; set; }

    public DeviceProcessDataOut? ProcessDataOut { get; set; }
}

public class Condition
{
    public string VariableId { get; set; } = string.Empty;

    public byte? Subindex { get; set; }

    public byte Value { get; set; }
}