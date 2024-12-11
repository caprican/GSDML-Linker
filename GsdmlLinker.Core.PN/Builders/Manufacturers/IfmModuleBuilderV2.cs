using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Builders.Manufacturers;

public class IfmModuleBuilderV2(Core.Models.Device masterDevice) : IfmModuleBuilder(masterDevice)
{
    public override void BuildModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor, string deviceName)
    {
        if (device is null) return;

        var ioData = new GSDML.DeviceProfile.SubmoduleItemBaseTIOData();
        if (inputDatas?.Count > 0)
        {
            ioData.Input = new GSDML.DeviceProfile.IODataT
            {
                Consistency = GSDML.Primitives.IODataConsistencyEnumT.AllItemsConsistency,
                DataItem = [.. inputDatas]
            };
        }
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
            State = Core.Models.ItemState.Created,
        };
        submodule.Name = ExternalTextGet(submodule.ModuleInfo?.Name?.TextId) ?? string.Empty;
        submodule.Description = ExternalTextGet(submodule.ModuleInfo?.InfoText?.TextId) ?? string.Empty;
        submodule.CategoryRef = ((Models.Device)masterDevice).GetCategoryText(submodule.ModuleInfo?.CategoryRef);
        submodule.SubCategoryRef = ((Models.Device)masterDevice).GetCategoryText(submodule.ModuleInfo?.SubCategory1Ref);
        submodule.VendorId = Convert.ToUInt16(device.VendorId);
        submodule.DeviceId = Convert.ToUInt32(device.DeviceId);
        //((Models.Device)masterDevice).UseableSubmodules?.Add(new GSDML.DeviceProfile.UseableSubmodulesTSubmoduleItemRef
        //{
        //    AllowedInSubslots="2..9",
        //    SubmoduleItemTarget = submodule.ID
        //});

        ((Models.Device)masterDevice).SubmoduleList?.Add(submodule);
        foreach (var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        {
            if (dap.Modules is not null)
            {
                foreach (var module in dap.Modules)
                {
                    module.Submodules ??= [];
                    module.Submodules.Add(submodule);
                }
            }
        }
    }

    public override void UpdateModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor, string deviceName)
    {
        if (device is null) return;

        var ioData = new GSDML.DeviceProfile.SubmoduleItemBaseTIOData();
        if(inputDatas?.Count > 0)
        {
            ioData.Input = new GSDML.DeviceProfile.IODataT
            {
                Consistency = GSDML.Primitives.IODataConsistencyEnumT.AllItemsConsistency,
                DataItem = [.. inputDatas]
            };
        }
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
            State = Core.Models.ItemState.Modified,
        };
        submodule.Name = ExternalTextGet(submodule.ModuleInfo?.Name?.TextId) ?? string.Empty;
        submodule.Description = ExternalTextGet(submodule.ModuleInfo?.InfoText?.TextId) ?? string.Empty;
        submodule.CategoryRef = ((Models.Device)masterDevice).GetCategoryText(submodule.ModuleInfo?.CategoryRef);
        submodule.SubCategoryRef = ((Models.Device)masterDevice).GetCategoryText(submodule.ModuleInfo?.SubCategory1Ref);
        submodule.VendorId = Convert.ToUInt16(device.VendorId);
        submodule.DeviceId = Convert.ToUInt32(device.DeviceId);
        //((Models.Device)masterDevice).UseableSubmodules?.Add(new GSDML.DeviceProfile.UseableSubmodulesTSubmoduleItemRef
        //{
        //    AllowedInSubslots="2..9",
        //    SubmoduleItemTarget = submodule.ID

        //});

        var index = ((Models.Device)masterDevice).SubmoduleList?.FindIndex(s => s.ProfinetDeviceId == submodule.ID);
        if(index is not null && index >= 0)
        {
            ((Models.Device)masterDevice).SubmoduleList[(int)index] = submodule;
        }

        foreach (var dap in ((Models.Device)masterDevice).DeviceAccessPoints)
        {
            if (dap.Modules is not null)
            {
                foreach (var module in dap.Modules)
                {
                    module.Submodules ??= [];

                    var source = module.Submodules.SingleOrDefault(s => s.ProfinetDeviceId == submodule.ProfinetDeviceId);
                    module.Submodules.Insert(module.Submodules.IndexOf(source), submodule);
                    module.Submodules.Remove(source);
                }
            }
        }
    }

    public override void CreateRecordParameters(Core.Models.Device? device, Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, 
                                                IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters, bool unloclDeviceId)
    {
        ushort transfertSequence = 3;
        uint index = 1024;

        if (device is null) return;

        RecordDataList = [
            BuildPortParameters(dataStorage, ((Models.Device)masterDevice).SetModuleVendorId(Convert.ToUInt16(device.VendorId)),
                                             ((Models.Device)masterDevice).SetModuleDeviceId(Convert.ToUInt32(device.DeviceId)), unloclDeviceId),
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

    public override List<Core.Models.DeviceParameter> GetPortParameters(string deviceId)
    {
        var parameters = new List<Core.Models.DeviceParameter>();

        if (((Models.Device)masterDevice).SubmoduleList is IEnumerable<Models.SubmoduleItem> submoduleList)
        {
            var parameterRecordDatas = submoduleList.SingleOrDefault(s => s.ID == deviceId)?.RecordDataList?.ParameterRecordDataItem;
            if (parameterRecordDatas is not null)
            {
                foreach (var parameterRecordData in parameterRecordDatas.Where(w => w.Index == "47360"))
                {
                    if (parameterRecordData.Items is not null)
                    {
                        var recordConst = (GSDML.DeviceProfile.RecordDataConstT?)parameterRecordData.Items.FirstOrDefault(f => f is GSDML.DeviceProfile.RecordDataConstT);
                        var recordConstSplit = recordConst?.Data?.Split(',');
                        if (recordConstSplit?.Length >= 2)
                        {
                            var items = parameterRecordData.Items.Where(w => w is GSDML.DeviceProfile.RecordDataRefT).Cast<GSDML.DeviceProfile.RecordDataRefT>().ToArray();
                            if (items.Length == 1)
                            {
                                parameters.Add(ReadRecordParameter(items[0], 0));
                            }
                            else
                            {
                                for (var i = 0; i < items.Length; i++)
                                {
                                    parameters.Add(ReadRecordParameter(items[i], 0));
                                }
                            }
                        }
                    }
                }
            }
        }
        return parameters;
    }

    internal new static GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameters(Core.Models.DeviceDataStorage dataStorage, string vendorId, string deviceId, bool unloclDeviceId) =>
        new()
        {
            Index = "47360",
            Length = 16,
            TransferSequence = 1,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_IOLD_Port_parameters" },
            Items =
            [
                new GSDML.DeviceProfile.RecordDataConstT
                    {
                        Data = "0x01,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00"
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 3,
                        BitOffset = 0,
                        TextId = "T_Enable_Port_Diagnosis",
                        ID = "ID_PNPC_Bit_0",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
                        DefaultValue = "1"
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 3,
                        BitOffset = 1,
                        TextId = "T_Enable_Process_Alarm",
                        ID = "ID_PNPC_Bit_1",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
                        DefaultValue = "1",
                        Changeable = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 3,
                        BitOffset = 2,
                        TextId = "T_PortConfigSource",
                        ID = "ID_PNPC_Bit_2",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
                        DefaultValue = "1"
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 3,
                        BitOffset = 3,
                        TextId = "T_Enable_Input_fraction",
                        ID = "ID_PNPC_Bit_3",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
                        DefaultValue = "0",
                        Changeable = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 3,
                        BitOffset = 4,
                        TextId = "T_Enable_Pull_Plug",
                        ID = "ID_PNPC_Bit_4",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
                        DefaultValue = "1"
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 4,
                        TextId = "T_hidden",
                        ID = "ID_PNPC_PDin_Length",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                        DefaultValue = "0",
                        Changeable = false,
                        Visible = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 5,
                        TextId = "T_hidden",
                        ID = "ID_PNPC_PDout_Length",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                        DefaultValue = "2",
                        Changeable = false,
                        Visible = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ValueItemTarget = "V_PortModeIOL",
                        ByteOffset = 6,
                        TextId = "T_PortModeIOL",
                        ID = "ID_PortModeIOL",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                        DefaultValue = "1",
                        AllowedValues = "1",
                        Changeable = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ValueItemTarget = "V_Validation_Backup",
                        ByteOffset = 7,
                        TextId = "T_Validation_Backup",
                        ID = "ID_Validation_Backup",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                        DefaultValue = $"{(uint)dataStorage}",
                        AllowedValues = "2",
                        Changeable = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ValueItemTarget = "V_PNPC_PortIQ_Behaviour",
                        ByteOffset = 8,
                        TextId = "T_PNPC_PortIQ_Behaviour",
                        ID = "ID_PNPC_PortIQ_Behaviour",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                        DefaultValue = "0",
                        Changeable = false,
                        Visible = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ValueItemTarget = "V_MasterCycleTime",
                        ByteOffset = 9,
                        TextId = "T_MasterCycleTime",
                        ID = "ID_MasterCycleTime",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                        DefaultValue = "0",
                        AllowedValues = "0 20 40 68 88 128 148 188"
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 10,
                        TextId = "T_Validation_VendorID",
                        ID = "ID_Validation_VendorID",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned16,
                        DefaultValue = $"{vendorId}",
                        AllowedValues = $"{vendorId}",
                        Changeable = false
                    },
                    new GSDML.DeviceProfile.RecordDataRefT
                    {
                        ByteOffset = 12,
                        TextId = "T_Validation_DeviceID",
                        ID = "ID_Validation_DeviceID",
                        DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned32,
                        DefaultValue = deviceId,
                        AllowedValues = !unloclDeviceId ? deviceId : "0..16777215",
                        Changeable = unloclDeviceId
                    }
            ],
            MenuList =
            [
                new GSDML.DeviceProfile.MenuItemT
                    {
                        ID = "MenuItemPort_parameters",
                        Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_IOLD_Port_parameters" },
                        Items =
                        [
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_0" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_1" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_2" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_3" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_4" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_PDin_Length" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_PDout_Length" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PortModeIOL" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_Validation_Backup" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_PortIQ_Behaviour" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_Validation_VendorID" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_Validation_DeviceID" },
                            new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_MasterCycleTime" }
                        ]
                    }
            ]
        };

    internal new static GSDML.DeviceProfile.ParameterRecordDataT BuildSafeRecord() =>
        new()
        {
            Index = "300",
            Length = 1,
            TransferSequence = 2,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_FailSafeRecord" },
            Items =
            [
                new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00" },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    ValueItemTarget = "VAL_FailSafeMode",
                    ByteOffset = 0,
                    BitLength = "8",
                    TextId = "RecFSM_FailSafeMode",
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                    DefaultValue = "0",
                    AllowedValues = "0",
                    Changeable = false
                }
            ]
        };

    internal new static GSDML.DeviceProfile.IODataTDataItem PQIBuid() =>
        new()
        {
            DataType = GSDML.Primitives.DataItemTypeEnumT.Unsigned8,
            UseAsBits = true,
            TextId = "PQI",
            BitDataItem =
            [
                new() { BitOffset = 0, TextId = "RE" },
                new() { BitOffset = 1, TextId = "RE" },
                new() { BitOffset = 2, TextId = "NP" },
                new() { BitOffset = 3, TextId = "SV" },
                new() { BitOffset = 4, TextId = "PO" },
                new() { BitOffset = 5, TextId = "DC" },
                new() { BitOffset = 6, TextId = "DE" },
                new() { BitOffset = 7, TextId = "PQ" },
            ]
        };

    internal new GSDML.DeviceProfile.ParameterRecordDataT BuildStartRecord(uint index, ushort transfertSequence)
    {
        var txtId = "T_ParamDownloadStart";
        masterDevice.ExternalTextList?.TryAdd(txtId, new(txtId, "Blockparameterization ParamDownloadStart") { State = Core.Models.ItemState.Created });

        return new GSDML.DeviceProfile.ParameterRecordDataT
        {
            Index = $"{index}",
            Length = 5,
            TransferSequence = transfertSequence,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = txtId },
            Items = [new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00,0x02,0x00,0x01,0x03" }]
        };
    }

    internal new GSDML.DeviceProfile.ParameterRecordDataT BuildEndRecord(uint index, ushort transfertSequence)
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
