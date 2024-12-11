using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Builders.Manufacturers;

public class MurrElectronicModuleBuilder(Core.Models.Device masterDevice) : ModuleBuilder(masterDevice)
{
    internal static string HexValue(uint value) => $"0x{$"{value:X4}"[..2]},0x{$"{value:X4}".Substring(2, 2)}";

    public override void BuildModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor, string deviceName)
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
                API = 0,
                MayIssueProcessAlarm = true,
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

        ((Models.Device)masterDevice).SubmoduleList?.Add(submodule);
        foreach (var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        {
            if (dap.Modules is not null)
            {
                foreach (var module in dap.Modules)
                {
                    module.Submodules ??= [];
                    module.Submodules.Add(new Core.Models.Module
                    {
                        Name = (!string.IsNullOrEmpty(submodule.ModuleInfo?.Name?.TextId) ? masterDevice.ExternalTextList?[submodule.ModuleInfo.Name.TextId].Item : string.Empty) ?? string.Empty,
                        Description = !string.IsNullOrEmpty(submodule.ModuleInfo?.InfoText?.TextId) ? masterDevice.ExternalTextList?[submodule.ModuleInfo.InfoText.TextId].Item : string.Empty,
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
            case Core.Models.DeviceDatatypes.OctetStringT:

                (byteCount, var stringDataRef) = StringToRecordDataRef(textId, byteOffset, parameter, externalTextList);
                items.Add(stringDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
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

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                paramaterLengt += byteCount;
                byteOffset += byteCount;
                break;
            case Core.Models.DeviceDatatypes.UIntegerT:
            case Core.Models.DeviceDatatypes.IntegerT:

                (byteCount, var intDataRef) = IntegerToRecordDataRef(textId, byteOffset, parameter, externalTextList);
                items.Add(intDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
                paramaterLengt += byteCount;
                byteOffset += byteCount;

                break;
            case Core.Models.DeviceDatatypes.Float32T:
                (byteCount, var floatDataRef) = FloatToRecordDataRef(textId, byteOffset, parameter, externalTextList);

                items.Add(floatDataRef);

                recordConst.Data += $"," + HexValue(byteCount) + string.Concat(Enumerable.Repeat(",0x00", (int)byteCount));
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
                                    masterDevice.ExternalTextList?.Add(boolRecord.TextId, new(boolRecord.TextId, record.Name) { State = Core.Models.ItemState.Created });
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
                                    masterDevice.ExternalTextList?.Add(intRecord.TextId, new(intRecord.TextId, record.Name) { State = Core.Models.ItemState.Created });
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
                                    masterDevice.ExternalTextList?.Add(floatRecord.TextId, new(floatRecord.TextId, record.Name) { State = Core.Models.ItemState.Created });
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
            masterDevice.ExternalTextList?.Add(parameterRecord.Name.TextId, new(parameterRecord.Name.TextId, parameter.Name) { State = Core.Models.ItemState.Created });
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
                masterDevice.ExternalTextList?.Add(id, new(id, $"Input data {inputLength} bits") { State = Core.Models.ItemState.Created });
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
                masterDevice.ExternalTextList?.Add(id, new(id, $"Output data {outputLength} bits") { State = Core.Models.ItemState.Created });
            }
            processDataIndex++;
        }

        inputDatas.Add(PQIBuid());
    }

    public override void CreateRecordParameters(Core.Models.Device? device, Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters, bool unloclDeviceId)
    {
        ushort transfertSequence = 3;
        uint index = 1024;

        if (device is null) return;

        RecordDataList = [
            //BuildPortParameters(dataStorage, masterDevice.VendorId, masterDevice.DeviceId),
            //BuildSafeRecord()
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
                var recordData = BuildRecordParameter($"IOLD_{indentNumber}_Par{variable.Key:D3}", index, transfertSequence, variable, device.ExternalTextList);

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

    public override List<Core.Models.DeviceParameter> GetRecordParameters(string deviceId)
    {
        var parameters = new List<Core.Models.DeviceParameter>();


        return parameters;
    }

    public override List<Core.Models.DeviceParameter> GetPortParameters(string deviceId)
    {
        var parameters = new List<Core.Models.DeviceParameter>();

        /// TODO : Get port paramters
        return parameters;
    }

    internal static GSDML.DeviceProfile.IODataTDataItem PQIBuid() =>
    new()
    {
        DataType = GSDML.Primitives.DataItemTypeEnumT.Unsigned8,
        UseAsBits = true,
        TextId = "TID_PQI",
        BitDataItem =
        [
            new() { BitOffset = 2, TextId = "TID_PQI_NP" },
            new() { BitOffset = 3, TextId = "TID_PQI_SV" },
            new() { BitOffset = 4, TextId = "TID_PQI_DA" },
            new() { BitOffset = 5, TextId = "TID_PQI_DC" },
            new() { BitOffset = 6, TextId = "TID_PQI_DE" },
            new() { BitOffset = 7, TextId = "TID_PQI_PQ" },
        ]
    };

    public override void UpdateModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor, string deviceName)
    {
    }

    public override void DeletModule(string moduleId)
    {
        //((Models.Device)masterDevice).SubmoduleList?.RemoveAll(a => a.ID == moduleId);

        //foreach (var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        //{
        //    if (dap.Modules is not null)
        //    {
        //        foreach (var module in dap.Modules)
        //        {
        //            if (module.Submodules is not null)
        //            {
        //                foreach (var submodule in module.Submodules.Where(w => w.ProfinetDeviceId == moduleId).ToArray())
        //                {
        //                    module.Submodules.Remove(submodule);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
