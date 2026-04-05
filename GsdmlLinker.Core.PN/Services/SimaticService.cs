using System.Xml.Serialization;

using GsdmlLinker.Core.PN.Contracts.Services;

namespace GsdmlLinker.Core.PN.Services;

public class SimaticService : ISimaticService
{
    public uint TiaPortalVersion { get; set; } = 20;

    public void CreateUdtFile(string path, string udtName, List<Core.Models.DeviceDataStructure> dataStructures)
    {
        var plcStrucBuilder = new SimaticML.Builders.PlcStructBuilder() as SimaticML.Contracts.Builders.IPlcStructBuilder;
        
        

        var document = plcStrucBuilder.Build(TiaPortalVersion, udtName, ConvertData(dataStructures));

        var serializer = new XmlSerializer(typeof(SimaticML.Document));
        using var myFile = new FileStream(@$"{path}\{udtName}.xml", FileMode.Create);
        serializer.Serialize(myFile, document);
    }

    private static List<SimaticML.Models.PlcData> ConvertData(List<Core.Models.DeviceDataStructure> dataStructures)
    {
        var plcData = new List<SimaticML.Models.PlcData>();

        foreach (var dataStructure in dataStructures)
        {
            switch(dataStructure.DataType)
            {
                case Core.Models.DeviceDatatypes.BooleanT:
                    plcData.Add(new SimaticML.Models.PlcData
                    {
                        Name = dataStructure.Name,
                        Description = dataStructure.Description,
                        DataType = "Bool",
                        BitOffset = dataStructure.BitOffset,
                        BitLength = dataStructure.BitLength
                    });
                    break;
                case Core.Models.DeviceDatatypes.UIntegerT:
                    switch(dataStructure.BitLength)
                    {
                        case 8:
                            plcData.Add(new SimaticML.Models.PlcData
                            {
                                Name = dataStructure.Name,
                                Description = dataStructure.Description,
                                DataType = "USint",
                                BitOffset = dataStructure.BitOffset,
                                BitLength = dataStructure.BitLength
                            });
                            break;
                        case 16:
                            plcData.Add(new SimaticML.Models.PlcData
                            {
                                Name = dataStructure.Name,
                                Description = dataStructure.Description,
                                DataType = "UInt",
                                BitOffset = dataStructure.BitOffset,
                                BitLength = dataStructure.BitLength
                            });
                            break;
                        case 32:
                            plcData.Add(new SimaticML.Models.PlcData
                            {
                                Name = dataStructure.Name,
                                Description = dataStructure.Description,
                                DataType = "UDInt",
                                BitOffset = dataStructure.BitOffset,
                                BitLength = dataStructure.BitLength
                            });
                            break;
                        default:
                            throw new NotSupportedException($"Bit length {dataStructure.BitLength} is not supported for unsigned integers.");
                    }
                    break;
                case Core.Models.DeviceDatatypes.IntegerT:
                    switch (dataStructure.BitLength)
                    {
                        case 8:
                            plcData.Add(new SimaticML.Models.PlcData
                            {
                                Name = dataStructure.Name,
                                Description = dataStructure.Description,
                                DataType = "Sint",
                                BitOffset = dataStructure.BitOffset,
                                BitLength = dataStructure.BitLength
                            });
                            break;
                        case 16:
                            plcData.Add(new SimaticML.Models.PlcData
                            {
                                Name = dataStructure.Name,
                                Description = dataStructure.Description,
                                DataType = "Int",
                                BitOffset = dataStructure.BitOffset,
                                BitLength = dataStructure.BitLength
                            });
                            break;
                        case 32:
                            plcData.Add(new SimaticML.Models.PlcData
                            {
                                Name = dataStructure.Name,
                                Description = dataStructure.Description,
                                DataType = "DInt",
                                BitOffset = dataStructure.BitOffset,
                                BitLength = dataStructure.BitLength
                            });
                            break;
                        default:
                            throw new NotSupportedException($"Bit length {dataStructure.BitLength} is not supported for unsigned integers.");
                    }
                    break;
                case Core.Models.DeviceDatatypes.Float32T:
                    plcData.Add(new SimaticML.Models.PlcData
                    {
                        Name = dataStructure.Name,
                        Description = dataStructure.Description,
                        DataType = "Real",
                        BitOffset = dataStructure.BitOffset,
                        BitLength = dataStructure.BitLength
                    });
                    break;
                default:
                    throw new NotSupportedException($"Data type {dataStructure.DataType} is not supported.");
            }
            
        }

        return plcData;
    }
}
