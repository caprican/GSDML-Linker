using GsdmlLinker.Core.PN.Contracts.Builders;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Builders;

public abstract class ModuleBuilder(Core.Models.Device masterDevice) : IModuleBuilder, Core.Contracts.IMementoable
{
    internal List<GSDML.DeviceProfile.IODataTDataItem>? inputDatas;
    internal List<GSDML.DeviceProfile.IODataTDataItem>? outputDatas;

    internal List<GSDML.DeviceProfile.ParameterRecordDataT>? RecordDataList;

    internal Core.Models.Device masterDevice = masterDevice;

    public abstract void CreateRecordParameters(Core.Models.Device? device, Core.Models.DeviceDataStorage dataStorage, bool supportBlockParameter, string indentNumber, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters, bool unloclDeviceId);

    public abstract void CreateDataProcess(string indentNumber, IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>> ProcessDatas);

    public abstract List<Core.Models.DeviceParameter> GetRecordParameters(string deviceId);

    public abstract Core.Models.DevicePortParameter GetPortParameters(string deviceId);

    public abstract void BuildModule(Core.Models.Device device, string indentNumber, string categoryRef, string categoryVendor, string deviceName);

    public abstract GSDML.DeviceProfile.ParameterRecordDataT? BuildRecordParameter(string textId, uint index, ushort transfertSequence, IGrouping<ushort, Core.Models.DeviceParameter>? variable, Dictionary<string, Core.Models.ExternalTextItem>? externalTextList);

    public abstract void UpdateModule(Core.Models.Device? device, string indentNumber, string categoryRef, string categoryVendor, string deviceName);

    public abstract void DeletModule(string moduleId);

    internal protected (uint, GSDML.DeviceProfile.RecordDataRefT) BoolToRecordDataRef(string textId, uint byteOffset, Core.Models.DeviceParameter parameter, Dictionary<string, Core.Models.ExternalTextItem>? IOLExternalText)
    {
        uint byteCount = 1;

        string defaultVaue;
        if (bool.TryParse(parameter.DefaultValue, out bool boolValueResult))
        {
            defaultVaue = $"{Convert.ToInt16(boolValueResult)}";
        }
        else
        {
            defaultVaue = $"{Convert.ToInt16(parameter.DefaultValue)}";
        }

        var boolDataRef = new GSDML.DeviceProfile.RecordDataRefT
        {
            ByteOffset = byteOffset,
            TextId = $"{textId}_Text",
            DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
            DefaultValue = defaultVaue
        };

        //if (boolData.SingleValue is not null && IOLExternalText is not null)
        //{
        //    var (valueItemTarget, allowedValues) = SetAllowedValueBool(boolData.SingleValue, dataRefTextId, IOLExternalText);
        //    booleanDataref.ValueItemTarget = valueItemTarget;
        //    //booleanDataref.AllowedValues = allowedValues;
        //}

        if (parameter.Values is not null && IOLExternalText is not null)
        {
            var values = new List<GSDML.DeviceProfile.Assign>();
            var vtTextId = $"{textId}_VT{parameter.Subindex:D2}";
            foreach (var value in parameter.Values)
            {
                var txtId = $"{vtTextId}_{Convert.ToInt16(bool.Parse(value.Key))}";
                values.Add(new GSDML.DeviceProfile.Assign
                {
                    TextId = txtId,
                    Content = $"{Convert.ToInt16(bool.Parse(value.Key))}"
                });
                ExternalTextSet(txtId, value.Value);
            }

            ValueSet(vtTextId, values);

            boolDataRef.ValueItemTarget = vtTextId;
            //boolDataRef.AllowedValues = string.Join(" ", parameter.Values.Select(s => s.Key));
        }

        if(parameter.Changeable)
        {
            boolDataRef.AllowedValues = boolDataRef.DefaultValue;
        }

        return (byteCount, boolDataRef);
    }

    internal protected Core.Models.DeviceParameter RecordDataRefToBool(GSDML.DeviceProfile.RecordDataRefT recordDataRef, int index, int subIndex)
    {
        string defaultValue;
        if(Int16.TryParse(recordDataRef.DefaultValue, out short shortValueResult))
        {
            defaultValue = $"{Convert.ToBoolean(shortValueResult)}";
        }
        else
        {
            defaultValue = $"{Convert.ToBoolean(recordDataRef.DefaultValue)}";
        }

        var boolParameter = new Core.Models.DeviceParameter
        {
            Index = (ushort)index,
            Subindex =  (ushort)subIndex,
            DataType = Core.Models.DeviceDatatypes.BooleanT,
            DefaultValue = defaultValue,
            Changeable = !recordDataRef.Changeable
        };

        return boolParameter;
    }

    internal protected (uint, GSDML.DeviceProfile.RecordDataRefT) IntegerToRecordDataRef(string textId, uint byteOffset, Core.Models.DeviceParameter parameter, Dictionary<string, Core.Models.ExternalTextItem>? IOLExternalText)
    {
        uint byteCount = 0;
        var recordDataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit;
        switch (parameter.BitLength)
        {
            case 64:
                byteCount = 8;
                recordDataType = parameter.DataType switch
                {
                    Core.Models.DeviceDatatypes.IntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Integer64,
                    Core.Models.DeviceDatatypes.UIntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned64,
                    _ => GSDML.Primitives.RecordDataRefTypeEnumT.BitArea
                };
                break;
            case 32:
                byteCount = 4;
                recordDataType = parameter.DataType switch
                {
                    Core.Models.DeviceDatatypes.IntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Integer32,
                    Core.Models.DeviceDatatypes.UIntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned32,
                    _ => GSDML.Primitives.RecordDataRefTypeEnumT.BitArea
                };
                break;
            case 16:
                byteCount = 2;
                recordDataType = parameter.DataType switch
                {
                    Core.Models.DeviceDatatypes.IntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Integer16,
                    Core.Models.DeviceDatatypes.UIntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned16,
                    _ => GSDML.Primitives.RecordDataRefTypeEnumT.BitArea
                };
                break;
            case 8:
                byteCount = 1;
                recordDataType = parameter.DataType switch
                {
                    Core.Models.DeviceDatatypes.IntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Integer8,
                    Core.Models.DeviceDatatypes.UIntegerT => GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
                    _ => GSDML.Primitives.RecordDataRefTypeEnumT.BitArea
                };
                break;
            default:
                recordDataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea;
                break;
        }

        var intDataRef = new GSDML.DeviceProfile.RecordDataRefT
        {
            ByteOffset = byteOffset,
            TextId = $"{textId}_Text",
            DataType = recordDataType,
            BitLength = (recordDataType == GSDML.Primitives.RecordDataRefTypeEnumT.BitArea) && (parameter.BitLength > 0) ? $"{parameter.BitLength}" : "1",
            DefaultValue = parameter.DefaultValue
        };

        if (parameter.Values is not null && IOLExternalText is not null)
        {
            var values = new List<GSDML.DeviceProfile.Assign>();
            var vtTextId = $"{textId}_VT{parameter.Subindex:D2}";
            foreach (var value in parameter.Values)
            {
                var txtId = $"{vtTextId}_{value.Key}";
                values.Add(new GSDML.DeviceProfile.Assign
                {
                    TextId = txtId,
                    Content = $"{value.Key}"
                });

                ExternalTextSet(txtId, value.Value);
            }

            ValueSet(vtTextId, values);

            intDataRef.ValueItemTarget = vtTextId;
            intDataRef.AllowedValues = string.Join(" ", parameter.Values.Select(s => s.Key));
        }
        else if (parameter.Minimum is not null && parameter.Maximum is not null)
        {
            intDataRef.AllowedValues = $"{parameter.Minimum}..{parameter.Maximum}";
        }

        if (parameter.Changeable)
        {
            intDataRef.AllowedValues = intDataRef.DefaultValue;
        }

        return (byteCount, intDataRef);
    }

    internal protected Core.Models.DeviceParameter RecordDataRefToInteger(GSDML.DeviceProfile.RecordDataRefT recordDataRef, int index, int subindex)
    {
        return new Core.Models.DeviceParameter
        {
            Index = (ushort)index,
            Subindex = (ushort)subindex,
            DataType = recordDataRef.DataType switch 
            {
                GSDML.Primitives.RecordDataRefTypeEnumT.Integer8 => Core.Models.DeviceDatatypes.IntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8 => Core.Models.DeviceDatatypes.UIntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Integer16 => Core.Models.DeviceDatatypes.IntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned16 => Core.Models.DeviceDatatypes.UIntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Integer32 => Core.Models.DeviceDatatypes.IntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned32 => Core.Models.DeviceDatatypes.UIntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Integer64 => Core.Models.DeviceDatatypes.IntegerT,
                GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned64 => Core.Models.DeviceDatatypes.UIntegerT,
                _ => Core.Models.DeviceDatatypes.IntegerT
            },
            DefaultValue = recordDataRef.DefaultValue ?? string.Empty,
            Changeable = !recordDataRef.Changeable
        };
    }

    protected internal (uint, GSDML.DeviceProfile.RecordDataRefT) FloatToRecordDataRef(string textId, uint byteOffset, Core.Models.DeviceParameter parameter, Dictionary<string, Core.Models.ExternalTextItem>? IOLExternalText)
    {
        uint byteCount;
        GSDML.Primitives.RecordDataRefTypeEnumT recordDataType;
        switch (parameter.BitLength)
        {
            case > 32:
                byteCount = 8;
                recordDataType = GSDML.Primitives.RecordDataRefTypeEnumT.Float64;
                break;
            default:
                byteCount = 4;
                recordDataType = GSDML.Primitives.RecordDataRefTypeEnumT.Float32;
                break;
        }

        var floatDataRef = new GSDML.DeviceProfile.RecordDataRefT
        {
            ByteOffset = byteOffset,
            TextId = $"{textId}_Text",
            DataType = recordDataType,
            DefaultValue = parameter.DefaultValue
        };

        if (parameter.Changeable)
        {
            floatDataRef.AllowedValues = floatDataRef.DefaultValue;
        }

        return (byteCount, floatDataRef);
    }

    protected internal (uint, GSDML.DeviceProfile.RecordDataRefT) StringToRecordDataRef(string textId, uint byteOffset, Core.Models.DeviceParameter parameter, Dictionary<string, Core.Models.ExternalTextItem>? IOLExternalText)
    {
        uint byteCount = parameter.FixedLength;
        string defString = string.Empty;

        if (!string.IsNullOrEmpty(parameter.DefaultValue))
        {
            defString = $"0x{parameter.DefaultValue[0]:X2}";
            foreach (var c in parameter.DefaultValue.Skip(1))
            {
                defString += $",0x{c:X2}";
            }
        }

        for (int i = parameter.DefaultValue.Length; i < parameter.FixedLength; i++)
        {
            if(i == 0)
            {
                defString += "0x00";
            }
            else
            {
                defString += ",0x00";
            }
        }

        var stringDataRef = new GSDML.DeviceProfile.RecordDataRefT
        {
            ByteOffset = byteOffset,
            TextId = $"{textId}_Text",
            DataType = GSDML.Primitives.RecordDataRefTypeEnumT.OctetString,
            Length = parameter.FixedLength,
            LengthSpecified = true,
            DefaultValue = defString
        };

        if (parameter.Changeable)
        {
            stringDataRef.AllowedValues = stringDataRef.DefaultValue;
        }

        return (byteCount, stringDataRef);
    }

    internal protected Core.Models.DeviceParameter RecordDataRefToString(GSDML.DeviceProfile.RecordDataRefT recordDataRef, int index, int subindex)
    {
        string defString = string.Empty;

        ///TODO : read string

        return new Core.Models.DeviceParameter
        {
            Index = (ushort)index,
            Subindex = (ushort)subindex,
            DataType = Core.Models.DeviceDatatypes.OctetStringT,
            FixedLength = recordDataRef.Length,
            DefaultValue = defString,
        };
    }

    internal GSDML.DeviceProfile.ModuleInfoT ModuleInfo(string categoryRef, string subCategoryRef, string identNumber, string name)
    {
        var txtId = $"IOLD_ProductName_{identNumber}";
        ExternalTextSet(txtId, name);

        var moduleInfo = new GSDML.DeviceProfile.ModuleInfoT
        {
            CategoryRef = categoryRef,
            SubCategory1Ref = subCategoryRef,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = txtId },
            InfoText = new GSDML.Primitives.ExternalTextRefT { TextId = txtId },
            OrderNumber = new GSDML.Primitives.TokenParameterT { Value = name }
        };
        return moduleInfo;
    }

    internal protected Core.Models.DeviceParameter ReadRecordParameter(GSDML.DeviceProfile.RecordDataRefT recordDataRef, int index, int subindex = 0) =>
        recordDataRef.DataType switch 
        {
            GSDML.Primitives.RecordDataRefTypeEnumT.Boolean => RecordDataRefToBool(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Bit => RecordDataRefToBool(recordDataRef, index, subindex),

            GSDML.Primitives.RecordDataRefTypeEnumT.BitArea => RecordDataRefToInteger(recordDataRef, index, subindex),

            GSDML.Primitives.RecordDataRefTypeEnumT.Integer8 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Integer16 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned16 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Integer32 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned32 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Integer64 => RecordDataRefToInteger(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned64 => RecordDataRefToInteger(recordDataRef, index, subindex),

            GSDML.Primitives.RecordDataRefTypeEnumT.OctetString => RecordDataRefToString(recordDataRef, index, subindex),
            GSDML.Primitives.RecordDataRefTypeEnumT.VisibleString => RecordDataRefToString(recordDataRef, index, subindex),

            _ => throw new NotImplementedException()
        };

    internal protected void ExternalTextSet(string id, string value)
    {
        if (masterDevice.ExternalTextList?.ContainsKey(id) != true)
            masterDevice.ExternalTextList?.TryAdd(id, new(id, value) { State = Core.Models.ItemState.Created });
        else
            masterDevice.ExternalTextList[id] = new(id, value) { State = Core.Models.ItemState.Modified };
    }

    internal protected string? ExternalTextGet(string? id)
    {
        if (!string.IsNullOrEmpty(id) && masterDevice.ExternalTextList?.ContainsKey(id) == true)
            return masterDevice.ExternalTextList?[id].Item;
        else
            return null;
    }

    internal protected void ValueSet(string id, IEnumerable<GSDML.DeviceProfile.Assign> assigns)
    {
        var valueList = ((Models.Device)masterDevice).ValueList;
        if (valueList?.ContainsKey(id) != true)
        {
            valueList?.TryAdd(id, new(id, [.. assigns]) { State = Core.Models.ItemState.Created });
        }
        else
        {
            valueList[id] = new(id, [.. assigns]) { State = Core.Models.ItemState.Modified };
        }
    }
}
