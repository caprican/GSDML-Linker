using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Builders.Manufacturers;

public class IfmModuleBuilder(Core.Models.Device masterDevice) : ModuleBuilder(masterDevice)
{
    internal static string HexValue(uint value) => $"0x{$"{value:X4}"[..2]},0x{$"{value:X4}".Substring(2, 2)}";
    static int IntValue(string hexValue) => int.Parse(hexValue.Replace("0x", string.Empty), System.Globalization.NumberStyles.HexNumber);

    public override void BuildModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor,string deviceName)
    {
        if (device is null) return;

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

        var submodule = new Models.SubmoduleItem(new GSDML.DeviceProfile.SubmoduleItemT
        {
            ID = $"IDS {deviceName} {indentNumber}",
            SubmoduleIdentNumber = indentNumber,
            API = 19969,
            MayIssueProcessAlarm = false,
            ModuleInfo = ModuleInfo(categoryRef, categoryVendor, indentNumber, deviceName),
            IOData = ioData,
            RecordDataList = new GSDML.DeviceProfile.SubmoduleItemBaseTRecordDataList
            {
                ParameterRecordDataItem = [.. RecordDataList]
            },
            //Graphics = graphics is not null ? [.. graphics] : null
        })
        {
            State = Core.Models.ItemState.Created
        };

        //((Models.Device)masterDevice).UseableSubmodules?.Add(new GSDML.DeviceProfile.UseableSubmodulesTSubmoduleItemRef
        //{
        //    AllowedInSubslots="2..9",
        //    SubmoduleItemTarget = submodule.ID

        //});

        ((Models.Device)masterDevice).SubmoduleList?.Add(submodule);
        foreach (var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        {
            if(dap.Modules is not null)
            {
                foreach (var module in dap.Modules)
                {
                    module.Submodules ??= [];
                    module.Submodules.Add(new Core.Models.Module
                    {
                        Name = ExternalTextGet(submodule.ModuleInfo?.Name?.TextId) ?? string.Empty,
                        Description = ExternalTextGet(submodule.ModuleInfo?.InfoText?.TextId) ?? string.Empty,
                        VendorName = submodule.ModuleInfo?.VendorName?.Value ?? string.Empty,
                        OrderNumber = submodule.ModuleInfo?.OrderNumber?.Value ?? string.Empty,
                        HardwareRelease = submodule.ModuleInfo?.HardwareRelease?.Value ?? string.Empty,
                        SoftwareRelease = submodule.ModuleInfo?.SoftwareRelease?.Value ?? string.Empty,
                        CategoryRef = ((Models.Device)masterDevice).GetCategoryText(submodule.ModuleInfo?.CategoryRef),
                        SubCategoryRef = ((Models.Device)masterDevice).GetCategoryText(submodule.ModuleInfo?.SubCategory1Ref),

                        VendorId = Convert.ToUInt16(device.VendorId),
                        DeviceId = Convert.ToUInt32(device.DeviceId),
                        ProfinetDeviceId = submodule.ID
                    });
                }
            }
        }
    }

    public override void UpdateModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor, string deviceName)
    {

    }

    public override GSDML.DeviceProfile.ParameterRecordDataT? BuildRecordParameter(string textId, uint index, ushort transfertSequence, 
                                                                                    IGrouping<ushort, Core.Models.DeviceParameter>? variable, Dictionary<string, Core.Models.ExternalTextItem>? externalTextList)
    {
        List<object> items = [];
        var recordConst = new GSDML.DeviceProfile.RecordDataConstT { Data = HexValue(variable!.Key) };
        uint paramaterLengt = 4;
        uint byteCount = 0, byteOffset = 4;

        var parameter = variable.First();
        var records = variable.Skip(1);

        switch (parameter.DataType)
        {
            case Core.Models.DeviceDatatypes.StringT:
            case Core.Models.DeviceDatatypes.OctetStringT :

                (byteCount, var stringDataRef) = StringToRecordDataRef(textId, byteOffset, parameter, externalTextList);
                items.Add(stringDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                paramaterLengt += byteCount;
                byteOffset += byteCount;

                break;
            case Core.Models.DeviceDatatypes.TimeT :
                break;
            case Core.Models.DeviceDatatypes.TimeSpanT :
                break;
            case Core.Models.DeviceDatatypes.BooleanT :
                (byteCount, var boolDataRef) = BoolToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                items.Add(boolDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                paramaterLengt += byteCount;
                byteOffset += byteCount;
                break;
            case Core.Models.DeviceDatatypes.UIntegerT :
            case Core.Models.DeviceDatatypes.IntegerT :

                (byteCount, var intDataRef) = IntegerToRecordDataRef(textId, byteOffset, parameter, externalTextList);
                items.Add(intDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                paramaterLengt += byteCount;
                byteOffset += byteCount;

                break;
            case Core.Models.DeviceDatatypes.Float32T :
                (byteCount, var floatDataRef) = FloatToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                items.Add(floatDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                paramaterLengt += byteCount;
                byteOffset += byteCount;
                break;
            case Core.Models.DeviceDatatypes.ArrayT :
                break;
            case Core.Models.DeviceDatatypes.RecordT :

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
                            case Core.Models.DeviceDatatypes.BooleanT :

                                (recordByteCount, var boolRecord) = BoolToRecordDataRef($"{textId}-{record.Subindex:D2}", byteOffset, record, externalTextList);

                                if (record.BitOffset is not null)
                                {
                                    boolRecord.BitOffset = (byte)(record.BitOffset! % 8);
                                }

                                items.Add(boolRecord);

                                if (!string.IsNullOrEmpty(record.Name) && !string.IsNullOrEmpty(boolRecord.TextId))
                                {
                                    ExternalTextSet(boolRecord.TextId, record.Name);
                                }
                                break;
                            case Core.Models.DeviceDatatypes.UIntegerT :
                            case Core.Models.DeviceDatatypes.IntegerT :

                                (recordByteCount, var intRecord) = IntegerToRecordDataRef($"{textId}-{record.Subindex:D2}", byteOffset, record, externalTextList);

                                if (record.BitOffset is not null)
                                {
                                    intRecord.BitOffset = (byte)(record.BitOffset! % 8);
                                }

                                items.Add(intRecord);

                                if (!string.IsNullOrEmpty(record.Name) && !string.IsNullOrEmpty(intRecord.TextId))
                                {
                                    ExternalTextSet(intRecord.TextId, record.Name);
                                }
                                break;
                            case Core.Models.DeviceDatatypes.Float32T :
                                (recordByteCount, var floatRecord) = FloatToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                                if (record.BitOffset is not null)
                                {
                                    floatRecord.BitOffset = (byte)(record.BitOffset! % 8);
                                }

                                items.Add(floatRecord);

                                if (!string.IsNullOrEmpty(record.Name) && !string.IsNullOrEmpty(floatRecord.TextId))
                                {
                                    ExternalTextSet(floatRecord.TextId, record.Name);
                                }
                                break;

                            case Core.Models.DeviceDatatypes.OctetStringT :
                                break;
                            case Core.Models.DeviceDatatypes.TimeT :
                                break;
                            case Core.Models.DeviceDatatypes.TimeSpanT :
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
                    recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                }
                break;
        }
        items.Insert(0, recordConst);

        var parameterRecord = new GSDML.DeviceProfile.ParameterRecordDataT
        {
            Index = $"{index}",
            TransferSequence = transfertSequence,
            Length = paramaterLengt,
            Items = [.. items],
        };

        if (!string.IsNullOrEmpty(parameter.Name))
        {
            parameterRecord.Name = new GSDML.Primitives.ExternalTextRefT { TextId = $"{textId}_Text" };
            ExternalTextSet(parameterRecord.Name.TextId, parameter.Name);
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
                var id = $"IOLD_{indentNumber}_inputDatas{processDataIndex:D2}_Text";
                inputDatas.Add(new GSDML.DeviceProfile.IODataTDataItem
                {
                    DataType = GSDML.Primitives.DataItemTypeEnumT.OctetString,
                    Length = (ushort)(inputLength / 8),
                    LengthSpecified = true,
                    TextId = id
                });
                ExternalTextSet(id, $"Input data {inputLength} bits");
            }

            if (outputLength is not null)
            {
                var id = $"IOLD_{indentNumber}_outputDatas{processDataIndex:D2}_Text";
                outputDatas ??= [];
                outputDatas.Add(new GSDML.DeviceProfile.IODataTDataItem
                {
                    DataType = GSDML.Primitives.DataItemTypeEnumT.OctetString,
                    Length = (ushort)(outputLength / 8),
                    LengthSpecified = true,
                    TextId = id
                });
                ExternalTextSet(id, $"Output data {outputLength} bits");
            }
            processDataIndex++;
        }

        inputDatas.Add(PQIBuid());
    }
    
    public override void CreateRecordParameters(Core.Models.Device? device, Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, 
                                                IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters)
    {
        ushort transfertSequence = 3;
        uint index = 1024;

        if (device is null) return;

        RecordDataList = [
            BuildPortParameters(dataStorage, ((Models.Device)masterDevice).SetModuleVendorId(Convert.ToUInt16(device.VendorId)), 
                                             ((Models.Device)masterDevice).SetModuleDeviceId(Convert.ToUInt32(device.DeviceId))),
            BuildSafeRecord()
        ];

        if (parameters?.Count() > 0)
        {
            if (supportBlockParameter)
            {
                var startRecor = BuildStartRecord(index, transfertSequence);

                if (startRecor is not null)
                {
                    RecordDataList.Add(startRecor);
                    index++;
                    transfertSequence++;
                }
            }

            foreach (var variable in parameters)
            {
                var recordData = BuildRecordParameter($"IOLD_{indentNumber}_Par{variable.Key:D3}", index, transfertSequence, variable, device.ExternalTextList);

                if (recordData is not null)
                {
                    RecordDataList.Add(recordData);

                    index++;
                    transfertSequence++;
                }
            }

            if (supportBlockParameter)
            {
                RecordDataList.Add(BuildEndRecord(index, transfertSequence));
            }
        }
    }

    public override List<Core.Models.DeviceParameter> ReadRecordParameter(string deviceId)
    {
        var parameters = new List<Core.Models.DeviceParameter>();

        if (((Models.Device)masterDevice).SubmoduleList is IEnumerable<Models.SubmoduleItem> submoduleList)
        {
            var parameterRecordDatas = submoduleList.SingleOrDefault(s => s.ID == deviceId)?.RecordDataList?.ParameterRecordDataItem;
            if(parameterRecordDatas is not null)
            {
                foreach (var parameterRecordData in parameterRecordDatas)
                {
                    if(parameterRecordData.Items is not null)
                    {
                        var recordConst = (GSDML.DeviceProfile.RecordDataConstT?)parameterRecordData.Items.FirstOrDefault(f => f is GSDML.DeviceProfile.RecordDataConstT);
                        var recordConstSplit = recordConst?.Data?.Split(',');
                        if(recordConstSplit?.Length >= 2)
                        {
                            var index = IntValue(string.Join("", recordConstSplit[..2]));

                            var items = parameterRecordData.Items.Where(w => w is GSDML.DeviceProfile.RecordDataRefT).Cast<GSDML.DeviceProfile.RecordDataRefT>().ToArray();
                            if (items.Length == 1)
                            {
                                parameters.Add(ReadRecordParameter(items[0], index));
                            }
                            else
                            {
                                for (var i = 0; i < items.Length; i++)
                                {
                                    parameters.Add(ReadRecordParameter(items[i], index, i + 1));
                                }
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
        ((Models.Device)masterDevice).SubmoduleList?.RemoveAll(a => a.ID == moduleId);

        foreach(var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        {
            if(dap.Modules is not null)
            {
                foreach(var module in dap.Modules)
                {
                    if(module.Submodules is not null)
                    {
                        foreach (var submodule in module.Submodules.Where(w => w.ProfinetDeviceId == moduleId).ToArray())
                        {
                            module.Submodules.Remove(submodule);
                        }
                    }
                }
            }
        }
    }

    internal static GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameters(Core.Models.DeviceDataStorage dataStorage, string vendorId, string deviceId) =>
     new()
     {
         Index = "45312",
         Length = 12,
         TransferSequence = 1,
         Name = new GSDML.Primitives.ExternalTextRefT { TextId = "Port parameters" },
         Items =
         [
             new GSDML.DeviceProfile.RecordDataConstT
                {
                    ByteOffset = 0,
                    Data = "0x00,0x01,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00"
                },
            new GSDML.DeviceProfile.RecordDataRefT
            {
                ValueItemTarget = "VAL_PortMode",
                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                BitLength = "4",
                ByteOffset = 2,
                BitOffset = 0,
                DefaultValue = "11",
                Changeable = true,
                AllowedValues = "11",
                TextId = "RecIOL_PortMode",
            },
            new GSDML.DeviceProfile.RecordDataRefT
            {
                ValueItemTarget = "M_Cycle",
                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                ByteOffset = 4,
                BitOffset = 0,
                BitLength = "8",
                DefaultValue = "0",
                AllowedValues = "0 20 40 68 88 128 148 188",
                TextId = "CycleTime",
                Visible = true,
                Changeable = true,
            },
            new GSDML.DeviceProfile.RecordDataRefT
            {
                ValueItemTarget = "I_Level",
                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                ByteOffset = 3,
                BitOffset = 1,
                BitLength = "3",
                DefaultValue = $"{dataStorage}",
                AllowedValues = "0..4",
                TextId = "IntegrationLevel",
                Visible = true,
                Changeable = false,
            },
            new GSDML.DeviceProfile.RecordDataRefT
            {
                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned16,
                ByteOffset = 5,
                DefaultValue = vendorId,//"0",
                AllowedValues = vendorId,//"0..65535",
                TextId = "VendorID",
                Changeable = false,
                Visible = true,
            },
            new GSDML.DeviceProfile.RecordDataRefT
            {
                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned32,
                ByteOffset = 7,
                DefaultValue = deviceId,//"0",
                AllowedValues = deviceId, //"0..16777215",
                TextId = "DeviceID",
                Changeable = false,
            },
            new GSDML.DeviceProfile.RecordDataRefT
            {
                ValueItemTarget = "VAL_DisableEvents",
                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                BitLength = "1",
                ByteOffset = 11,
                BitOffset = 0,
                DefaultValue = "0",
                Changeable = true,
                AllowedValues = "0 1",
                TextId = "RecIOL_DisableEvents"
            },
         ],
     };

    internal static GSDML.DeviceProfile.ParameterRecordDataT BuildSafeRecord() =>
        new()
        {
            Index = "300",
            Length = 1, //33,
            TransferSequence = 2,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "Fail Safe parameter" },
            Items =
            [
                //new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00" },
                new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00" },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    ValueItemTarget = "VAL_FailSafeMode",
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                    BitLength = "8",
                    ByteOffset = 0,
                    DefaultValue = "0",
                    Changeable = false,
                    AllowedValues = "0 1 2 3",
                    TextId = "RecFSM_FailSafeMode",
                },
                //new GSDML.DeviceProfile.RecordDataRefT
                //{
                //    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.OctetString,
                //    Length = 32,
                //    LengthSpecified = true,
                //    ByteOffset = 1,
                //    DefaultValue = "0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00",
                //    TextId = "PatternValue",
                //    Changeable = true,
                //    Visible = true,
                //}
            ]
        };

    internal static GSDML.DeviceProfile.IODataTDataItem PQIBuid() =>
        new()
        {
            DataType = GSDML.Primitives.DataItemTypeEnumT.Unsigned8,
            UseAsBits = true,
            TextId = "PQI",
            BitDataItem =
            [
                new() { BitOffset = 0, TextId = "DI4" },
                new() { BitOffset = 1, TextId = "DI2" },
                new() { BitOffset = 2, TextId = "NP" },
                new() { BitOffset = 3, TextId = "SV" },
                new() { BitOffset = 4, TextId = "DA" },
                new() { BitOffset = 5, TextId = "DC" },
                new() { BitOffset = 6, TextId = "DE" },
                new() { BitOffset = 7, TextId = "PQ" },
            ]
        };

    internal GSDML.DeviceProfile.ParameterRecordDataT BuildStartRecord(uint index, ushort transfertSequence)
    {
        var txtId = "T_ParamDownloadStart";
        masterDevice.ExternalTextList?.TryAdd(txtId, new(txtId, "Blockparameterization ParamDownloadStart") { State = Core.Models.ItemState.Created });

        return new GSDML.DeviceProfile.ParameterRecordDataT
        {
            Index = $"{index}",
            Length = 5,
            TransferSequence = transfertSequence,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_ParamDownloadStart" },
            Items = [new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00,0x02,0x00,0x01,0x03" }]
        };
    }

    internal GSDML.DeviceProfile.ParameterRecordDataT BuildEndRecord(uint index, ushort transfertSequence)
    {
        var txtId = "T_ParamDownloadEnd";
        masterDevice.ExternalTextList?.TryAdd(txtId, new(txtId, "Blockparameterization ParamDownloadEnd") { State = Core.Models.ItemState.Created });

        return new GSDML.DeviceProfile.ParameterRecordDataT
        {
            Index = $"{index}",
            Length = 5,
            TransferSequence = transfertSequence,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = txtId },
            Items = [new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00,0x02,0x00,0x01,0x04" }]
        };
    }
}
