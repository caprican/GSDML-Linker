using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Builders.Manufacturers;

public class BalluffModuleBuilder(Core.Models.Device masterDevice) : ModuleBuilder(masterDevice)
{
    public override void BuildModule(Core.Models.Device device, string indentNumber, string categoryRef, string categoryVendor, string deviceName)
    {
        var ioData = new GSDML.DeviceProfile.SubmoduleItemBaseTIOData
        {
            Input = new GSDML.DeviceProfile.IODataT
            {
                Consistency = GSDML.Primitives.IODataConsistencyEnumT.AllItemsConsistency,
                DataItem = [.. inputDatas]
            }
        };
        if (outputDatas?.Count > 0)
        {
            ioData.Output = new GSDML.DeviceProfile.IODataT
            {
                Consistency = GSDML.Primitives.IODataConsistencyEnumT.AllItemsConsistency,
                DataItem = [.. outputDatas]
            };
        }

        var module =new Models.ModuleItem(new GSDML.DeviceProfile.ModuleItemT
        {
            ID = $"ID_Mod_{deviceName}_{indentNumber}",
            ModuleIdentNumber = indentNumber,
            //API = 19969,
            ModuleInfo = ModuleInfo(categoryRef, categoryVendor, indentNumber, deviceName),
            VirtualSubmoduleList = [
                new GSDML.DeviceProfile.BuiltInSubmoduleItemT
                {
                    ID = "",
                    SubmoduleIdentNumber = "0x0001",
                    MayIssueProcessAlarm = true,
                    IOData = ioData,
                    RecordDataList = new GSDML.DeviceProfile.SubmoduleItemBaseTRecordDataList
                    {
                        ParameterRecordDataItem = [.. RecordDataList]
                    },
                    ModuleInfo = ModuleInfo(categoryRef, categoryVendor, indentNumber, deviceName),
                }
            ]
            //Graphics = graphics is not null ? [.. graphics] : null
        })
        { 
            State = Core.Models.ItemState.Created
        };
        ((Models.Device)masterDevice).ModuleList?.Add(module);

        foreach (var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        {
            if (dap?.Modules is not null)
            {
                dap.Modules.Add(new Core.Models.Module
                {
                    Name = (!string.IsNullOrEmpty(module.ModuleInfo?.Name?.TextId) ? masterDevice.ExternalTextList?[module.ModuleInfo.Name.TextId] : string.Empty) ?? string.Empty,
                    Description = !string.IsNullOrEmpty(module.ModuleInfo?.InfoText?.TextId) ? masterDevice.ExternalTextList?[module.ModuleInfo.InfoText.TextId] : string.Empty,
                    VendorName = module.ModuleInfo?.VendorName?.Value ?? string.Empty,
                    OrderNumber = module.ModuleInfo?.OrderNumber?.Value ?? string.Empty,
                    HardwareRelease = module.ModuleInfo?.HardwareRelease?.Value ?? string.Empty,
                    SoftwareRelease = module.ModuleInfo?.SoftwareRelease?.Value ?? string.Empty,
                    CategoryRef = ((Models.Device)masterDevice).GetCategoryText(module.ModuleInfo?.CategoryRef),
                    SubCategoryRef = ((Models.Device)masterDevice).GetCategoryText(module.ModuleInfo?.SubCategory1Ref),

                    VendorId = Convert.ToUInt16(device.VendorId),
                    DeviceId = Convert.ToUInt32(device.DeviceId),
                    ProfinetDeviceId = module.ID
                });
            }
        }
    }

    public override GSDML.DeviceProfile.ParameterRecordDataT? BuildRecordParameter(string textId, uint index, ushort transfertSequence, 
                                                                                    IGrouping<ushort, Core.Models.DeviceParameter>? variable, Dictionary<string, string>? externalTextList)
    {
        List<object> items = [];
        uint paramaterLengt = 4;
        uint byteCount = 0, byteOffset = 4;

        if (variable is null) return null;
        var parameter = variable.First();
        var records = variable.Skip(1);

        switch (parameter.DataType)
        {
            case Core.Models.DeviceDatatypes.StringT:
            case Core.Models.DeviceDatatypes.OctetStringT:

                (byteCount, var stringDataRef) = StringToRecordDataRef(textId, byteOffset, parameter, externalTextList);
                items.Add(stringDataRef);

                paramaterLengt += byteCount;
                byteOffset += byteCount;
                break;
            case Core.Models.DeviceDatatypes.TimeT:
                break;
            case Core.Models.DeviceDatatypes.TimeSpanT:
                break;
            case Core.Models.DeviceDatatypes.BooleanT:
                (byteCount, var boolDataRef) = BoolToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                items.Add(boolDataRef);

                paramaterLengt += byteCount;
                byteOffset += byteCount;
                break;
            case Core.Models.DeviceDatatypes.UIntegerT:
            case Core.Models.DeviceDatatypes.IntegerT:

                (byteCount, var intDataRef) = IntegerToRecordDataRef(textId, byteOffset, parameter, externalTextList);
                items.Add(intDataRef);

                paramaterLengt += byteCount;
                byteOffset += byteCount;

                break;
            case Core.Models.DeviceDatatypes.Float32T:
                (byteCount, var floatDataRef) = FloatToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                items.Add(floatDataRef);

                paramaterLengt += byteCount;
                byteOffset += byteCount;
                break;
            case Core.Models.DeviceDatatypes.ArrayT:
                break;
            case Core.Models.DeviceDatatypes.RecordT:

                if (records is not null)
                {
                    int? byteNumber = 0;
                    foreach (var record in records)
                    {
                        int? boolOffset = null;
                        uint recordByteCount = 0;
                        if (record.BitOffset is not null)
                        {
                            boolOffset = record.BitOffset % 8;
                            var octalNumber = (record.BitOffset - boolOffset) / 8;
                            var bytePosition = (parameter.BitLength / 8) - octalNumber - 1;

                            if (bytePosition > byteNumber)
                            {
                                byteOffset++;
                            }
                            byteNumber = bytePosition;
                        }

                        switch (record.DataType)
                        {
                            case Core.Models.DeviceDatatypes.BooleanT:

                                (recordByteCount, var boolRecord) = BoolToRecordDataRef($"{textId}-{record.Subindex:D2}", byteOffset, record, externalTextList);

                                if (record.BitOffset is not null)
                                {
                                    boolRecord.BitOffset = (byte)(record.BitOffset! % 8);
                                }

                                items.Add(boolRecord);

                                if (!string.IsNullOrEmpty(record.Name) && !string.IsNullOrEmpty(boolRecord.TextId))
                                {
                                    masterDevice.ExternalTextList?.Add(boolRecord.TextId, record.Name);
                                }
                                break;
                            case Core.Models.DeviceDatatypes.UIntegerT:
                            case Core.Models.DeviceDatatypes.IntegerT:

                                (recordByteCount, var intRecord) = IntegerToRecordDataRef($"{textId}-{record.Subindex:D2}", byteOffset, record, externalTextList);

                                if (record.BitOffset is not null)
                                {
                                    intRecord.BitOffset = (byte)(record.BitOffset! % 8);
                                }

                                items.Add(intRecord);

                                if (!string.IsNullOrEmpty(record.Name) && !string.IsNullOrEmpty(intRecord.TextId))
                                {
                                    masterDevice.ExternalTextList?.Add(intRecord.TextId, record.Name);
                                }
                                break;
                            case Core.Models.DeviceDatatypes.Float32T:
                                (recordByteCount, var floatRecord) = FloatToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                                if (record.BitOffset is not null)
                                {
                                    floatRecord.BitOffset = (byte)(record.BitOffset! % 8);
                                }

                                items.Add(floatRecord);

                                if (!string.IsNullOrEmpty(record.Name) && !string.IsNullOrEmpty(floatRecord.TextId))
                                {
                                    masterDevice.ExternalTextList?.Add(floatRecord.TextId, record.Name);
                                }
                                break;

                            case Core.Models.DeviceDatatypes.OctetStringT:
                                break;
                            case Core.Models.DeviceDatatypes.TimeT:
                                break;
                            case Core.Models.DeviceDatatypes.TimeSpanT:
                                break;
                        }

                        if (record.BitOffset is not null)
                        {
                            if (boolOffset == 0)
                            {
                                paramaterLengt++;
                                byteCount++;
                            }
                        }
                        else
                        {
                            paramaterLengt += recordByteCount;
                            byteOffset += recordByteCount;
                            byteCount += recordByteCount;
                        }


                    }
                }

                break;
        }

        var parameterRecord = new GSDML.DeviceProfile.ParameterRecordDataT
        {
            Index = $"{index}",
            TransferSequence = transfertSequence,
            Length = paramaterLengt,
            Items = [.. items],
        };

        if (!string.IsNullOrEmpty(parameter.Name))
        {
            parameterRecord.Name = new GSDML.Primitives.ExternalTextRefT { TextId = $"{textId}" };
            masterDevice.ExternalTextList?.Add(parameterRecord.Name.TextId, parameter.Name);
        }

        return parameterRecord;
    }

    public override void CreateDataProcess(string indentNumber, IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>> processDatas)
    {
        inputDatas = [];
        ushort processDataIndex = 1;

        foreach (var processData in processDatas)
        {
            var inputLength = processData.Max(g => g.ProcessDataIn?.BitLength);
            var outputLength = processData.Max(g => g.ProcessDataOut?.BitLength);

            if (inputLength is not null)
            {
                inputDatas.Add(new GSDML.DeviceProfile.IODataTDataItem
                {
                    DataType = GSDML.Primitives.DataItemTypeEnumT.OctetString,
                    Length = (ushort)(inputLength / 8),
                    LengthSpecified = true,
                    TextId = $"TOK_Input_DataItem_{indentNumber}_{processDataIndex:D2}"
                });
                masterDevice.ExternalTextList?.Add($"TOK_Input_DataItem_{indentNumber}_{processDataIndex:D2}", $"Input data {inputLength} bits");
            }

            if (outputLength is not null)
            {
                outputDatas ??= [];
                outputDatas.Add(new GSDML.DeviceProfile.IODataTDataItem
                {
                    DataType = GSDML.Primitives.DataItemTypeEnumT.OctetString,
                    Length = (ushort)(outputLength / 8),
                    LengthSpecified = true,
                    TextId = $"TOK_Output_DataItem_{indentNumber}_{processDataIndex:D2}"
                });
                masterDevice.ExternalTextList?.Add($"TOK_Output_DataItem_{indentNumber}_{processDataIndex:D2}", $"Output data {outputLength} bits");
            }
            processDataIndex++;
        }
    }

    public override void CreateRecordParameters(Core.Models.Device? device, Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, 
                                                IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters)
    {
        ushort transfertSequence = 5;
        uint index = 16;

        if (device is not null) return;

        RecordDataList = [
            BuildPortParameter1(),
            BuildPortParameter2(),
            BuildPortParameter3(dataStorage, masterDevice.VendorId, masterDevice.DeviceId),
            BuildPortParameter4()
        ];

        if (parameters?.Count() > 0)
        {
            //if (supportBlockParameter)
            //{
            //    var startRecor = BuildStartRecord(index, transfertSequence);

            //    if (startRecor is not null)
            //    {
            //        RecordDataList.Add(startRecor);
            //        index++;
            //        transfertSequence++;
            //    }
            //}

            foreach (var variable in parameters)
            {
                var recordData = BuildRecordParameter($"TOK_{indentNumber}_Par{variable.Key:D3}", index, transfertSequence, variable, device.ExternalTextList);

                if (recordData is not null)
                {
                    RecordDataList.Add(recordData);

                    index++;
                    transfertSequence++;
                }
            }

            //if (supportBlockParameter)
            //{
            //    RecordDataList.Add(BuildEndRecord(index, transfertSequence));
            //}
        }
    }

    public override List<Core.Models.DeviceParameter> ReadRecordParameter(string deviceId)
    {
        var parameters = new List<Core.Models.DeviceParameter>();

        if (((Models.Device)masterDevice).ModuleList is IEnumerable<GSDML.DeviceProfile.ModuleItemT> moduleList)
        {
            if(moduleList.SingleOrDefault(s => s.ID == deviceId)?.VirtualSubmoduleList is GSDML.DeviceProfile.BuiltInSubmoduleItemT[] virtualSubmoduleList)
            {
                foreach (var virtualSubmodule in virtualSubmoduleList)
                {
                    var parameterRecordDatas = virtualSubmodule?.RecordDataList?.ParameterRecordDataItem;
                    if (parameterRecordDatas is not null)
                    {
                        foreach (var parameterRecordData in parameterRecordDatas)
                        {
                            if (parameterRecordData.Items is not null)
                            {
                                //int.TryParse(parameterRecordData.Index, out var index);
                                //parameterRecordData.Items.First()
                                //var items = parameterRecordData.Items.Where(w => w is GSDML.DeviceProfile.RecordDataRefT).Cast<GSDML.DeviceProfile.RecordDataRefT>().ToArray();
                                //if (items.Length == 1)
                                //{
                                //    parameters.Add(ReadRecordParameter(items[0], index));
                                //}
                                //else
                                //{
                                //    for (var i = 0; i < items.Length; i++)
                                //    {
                                //        parameters.Add(ReadRecordParameter(items[i], index, i + 1));
                                //    }
                                //}
                            }
                        }
                    }
                }

            }

        }
        return parameters;
    }

    public override void DeletModule(string moduleId)
    {

    }

    internal static GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameter1() =>
        new()
        {
            Index = "1",
            Length = 2,
            TransferSequence = 1,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "TOK_ParaRecIOLink_Cycle" },
            Items = 
            [
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    ValueItemTarget = "VAL_CycleTime",
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                    BitLength = "8",
                    ByteOffset = 0,
                    BitOffset = 0,
                    DefaultValue = "0",
                    Changeable = true,
                    AllowedValues = "0 16 32 48 64 68 72 76 80 84 88 92 96 100 104 108 112 116 120 124 128 129 130 131 132 133 134 135 136 137 138 139 140 141 142 143 144 145 146 147 148 149 150 151 152 153 154 155 156 157 158 159 160 161 162 163 164 165 166 167 168 169 170 171 172 173 174 175 176 177 178 179 180 181 182 183 184 185 186 187 188 189 190 191",
                    TextId = "TOK_CycleTime",
                }
            ]
        };
    internal static GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameter2() =>
        new()
        {
            Index = "2",
            Length = 2,
            TransferSequence = 2,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "TOK_ParaRecIOLink_DataWindow" },
            Items =
            [
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 0,
                    DefaultValue = "0",
                    Changeable = false,
                    TextId = "TOK_Param1",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 1,
                    DefaultValue = "2",
                    Changeable = false,
                    TextId = "TOK_Param2",
                }
            ]
        };
    internal static GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameter3(Core.Models.DeviceDataStorage dataStorage, string vendorId, string deviceId) =>
        new()
        {
            Index = "3",
            Length = 22,
            TransferSequence = 3,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "TOK_ParaRecIOLink_Validation" },
            Items =
            [
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    ValueItemTarget = "VAL_Validation",
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                    BitLength = "2",
                    ByteOffset = 0,
                    BitOffset = 2,
                    DefaultValue = "0",
                    Changeable = true,
                    AllowedValues = "0..1",
                    TextId = "TOK_Validation",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 1,
                    DefaultValue = "3",
                    Changeable = false,
                    TextId = "TOK_Param3",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 2,
                    DefaultValue = "120",
                    Changeable = false,
                    TextId = "TOK_Param4",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 3,
                    DefaultValue = "5",
                    Changeable = false,
                    TextId = "TOK_Param5",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 4,
                    DefaultValue = "4",
                    Changeable = true,
                    AllowedValues = "0..255",
                    TextId = "TOK_Param6",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    ByteOffset = 5,
                    DefaultValue = "80",
                    Changeable = true,
                    AllowedValues = "0..255",
                    TextId = "TOK_Param7",
                },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.VisibleString,
                    Length = 16,
                    LengthSpecified = true,
                    Visible = false,
                    ByteOffset = 6,
                    DefaultValue = "",
                    Changeable = false,
                    TextId = "TOK_Param8",
                }
            ]
        };
    internal static GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameter4() =>
        new()
        {
            Index = "4",
            Length = 1,
            TransferSequence = 4,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "TOK_ParaRecIOLink_ParaServer" },
            Items =
            [
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    ValueItemTarget = "VAL_DsState",
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                    BitLength = "8",
                    ByteOffset = 0,
                    BitOffset = 0,
                    DefaultValue = "0",
                    Changeable = false,
                    TextId = "TOK_ParServEnable",
                }
            ]
        };
}
