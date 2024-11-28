using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

using IODD = ISO15745.IODD;

namespace GsdmlLinker.Core.IOL.Models;

public record Device : Core.Models.Device
{
    public string IolSchematicVersion { get; } = string.Empty;
 
    public string? VendorText { get; }
    public string? VendorUrl { get; }
    public string? VendorLogo {  get; }

    public Core.Models.DeviceDataStorage DataStorage { get; }
    public bool SupportBlockParameter { get; }

    public List<DeviceVariantDescription>? Variants { get; init; }

    public Dictionary<string, DatatypeItem>? DatatypeList { get; }
    
    public List<Core.Models.DeviceParameter>? Parameters { get; }

    public IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>>? ProcessDatas { get; }

    public override string ProfileParameterIndex => string.Empty;

    public override uint VendorIdSubIndex => 0;

    public override uint DeviceIdSubIndex => 0;

    public Device(string filePath, Match? match) : base(filePath)
    {
        if (match is null)
        {
            return;
        }

        //<vendor>-<code_produit>-<date>-IODD<version>(-<langue>)?(-<Commentaire>)?
        SchematicVersion = match.Groups[4].Value;
        ManufacturerName = match.Groups[1].Value;
        Name = match.Groups[2].Value;

        var lang = match.Groups[6].Value;
        var txtFree = match.Groups[7].Value;

        var dateMatch = Regexs.DateRegex().Match(match.Groups[3].Value);
        Version = new DateTime(int.Parse(dateMatch.Groups[1].Value), int.Parse(dateMatch.Groups[2].Value), int.Parse(dateMatch.Groups[3].Value)); 

        IolSchematicVersion = (SchematicVersion) switch
        {
            "1.0" => "http://www.io-link.com/IODD/2009/11",
            "1.0.1" => "http://www.io-link.com/IODD/2009/11",
            "1.1" => "http://www.io-link.com/IODD/2010/10",
            _ => string.Empty
        };

        //var settings = new XmlReaderSettings
        //{
        //    ValidationType = ValidationType.Schema
        //};
        //settings.Schemas.Add(null, @"Resources\xml.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-Primitives1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-Datatypes1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-Variables1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-Events1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-UserInterface1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-Communication1.1.xsd");
        //settings.Schemas.Add(null, @"Resources\IODD\IODD-WirelessCommunication1.1.xsd");

        if(string.IsNullOrEmpty(lang))
        {
            var serialize = new XmlSerializer(typeof(IODD.IODevice), IolSchematicVersion);
            using var reader = XmlReader.Create(filePath/*, settings*/);
            var device = serialize.Deserialize(reader) as IODD.IODevice;

            VendorId = device?.ProfileBody?.DeviceIdentity?.VendorId.ToString() ?? string.Empty;
            VendorName = device?.ProfileBody?.DeviceIdentity?.VendorName ?? string.Empty;
            DeviceId = device?.ProfileBody?.DeviceIdentity?.DeviceId.ToString() ?? string.Empty;

            DataStorage = device?.ProfileHeader?.ProfileRevision switch 
            {
                IODD.ProfileRevision.Revision10 => Core.Models.DeviceDataStorage.CompatibleV1_0,
                IODD.ProfileRevision.Revision11 => Core.Models.DeviceDataStorage.CompatibleV1_1,
                _ => Core.Models.DeviceDataStorage.NoCheckAndClear
            };
            SupportBlockParameter = device?.ProfileBody?.DeviceFunction?.Features?.BlockParameter ?? false;

            if (device?.ExternalTextCollection?.PrimaryLanguage?.Items is IODD.Primitives.TextDefinitionT[] extTexts)
            {
                ExternalTextList = [];
                foreach (var extText in extTexts)
                {
                    if (!string.IsNullOrEmpty(extText?.Id) && extText?.Value is not null)
                    {
                        ExternalTextList.Add(extText.Id, new(extText.Id, extText.Value));
                    }
                }
            }

            if (device?.ProfileBody?.DeviceFunction?.DatatypeCollection?.Datatype is not null)
            {
                DatatypeList = [];
                foreach (var dataType in device.ProfileBody.DeviceFunction.DatatypeCollection.Datatype!)
                {
                    DatatypeList.Add(dataType.Id, new(dataType));
                }
            }

            if (device?.ProfileBody?.DeviceFunction?.ProcessDataCollection?.ProcessData is not null)
            {
                var processDatas = new List<Core.Models.DeviceProcessData>();
                foreach (var processData in device.ProfileBody.DeviceFunction.ProcessDataCollection.ProcessData)
                {
                    var processDataItem = new Core.Models.DeviceProcessData
                    {
                        Condition = processData.Condition is not null ? new Core.Models.Condition
                        {
                            VariableId = processData.Condition.VariableId,
                            Subindex = processData.Condition.Subindex,
                            Value = processData.Condition.Value
                        } : null,
                        ProcessDataIn = processData.ProcessDataIn is not null ? new Core.Models.DeviceProcessDataIn
                        {
                            BitLength = processData.ProcessDataIn.BitLength,
                            Name = ExternalTextList?[processData.ProcessDataIn.Name?.TextId!].Item ?? string.Empty
                        } : null,
                        ProcessDataOut = processData.ProcessDataOut is not null ? new Core.Models.DeviceProcessDataOut
                        {
                            BitLength = processData.ProcessDataOut.BitLength,
                            Name = ExternalTextList?[processData.ProcessDataOut.Name?.TextId!].Item ?? string.Empty
                        } : null
                    };
                    processDatas.Add(processDataItem);
                }

                ProcessDatas = processDatas.GroupBy(g => g.Condition?.VariableId);
            }

            if (device?.ProfileBody?.DeviceFunction?.VariableCollection?.Variable is not null)
            {
                foreach (var deviceVariable in device.ProfileBody.DeviceFunction.VariableCollection.Variable!.OrderBy(variable => variable.Index))
                {
                    if (DeviceParameterBuild(deviceVariable) is List<Core.Models.DeviceParameter> parameter)
                    {
                        Parameters ??= [];
                        Parameters.AddRange(parameter);
                    }

                }
            }

            if (device?.ProfileBody?.DeviceIdentity is IODD.DeviceIdentityT deviceIdentity)
            {
                if (ExternalTextList is not null)
                {
                    VendorText = !string.IsNullOrEmpty(deviceIdentity.VendorText?.TextId) ? ExternalTextList[deviceIdentity.VendorText.TextId].Item : null;
                    VendorUrl = !string.IsNullOrEmpty(deviceIdentity.VendorUrl?.TextId) ? ExternalTextList[deviceIdentity.VendorUrl.TextId].Item : null;
                    Name = !string.IsNullOrEmpty(deviceIdentity.DeviceName?.TextId) ? ExternalTextList[deviceIdentity.DeviceName.TextId].Item : null;
                    DeviceFamily = !string.IsNullOrEmpty(deviceIdentity.DeviceFamily?.TextId) ? ExternalTextList[deviceIdentity.DeviceFamily.TextId].Item : null;
                }

                var folderPath = Path.GetDirectoryName(filePath);
                if(!string.IsNullOrEmpty(deviceIdentity.VendorLogo?.Name))
                {
                    VendorLogo = !string.IsNullOrEmpty(folderPath) ? Path.Combine(folderPath, deviceIdentity.VendorLogo.Name) : string.Empty;

                    GraphicsList = [];
                    GraphicsList.Add("VendorLogo", new("VendorLogo", deviceIdentity.VendorLogo.Name));
                }

                if (deviceIdentity.DeviceVariantCollection?.DeviceVariant is not null)
                {
                    Variants = [];
                    foreach (var variant in deviceIdentity.DeviceVariantCollection.DeviceVariant)
                    {
                        var name = ExternalTextList is not null && !string.IsNullOrEmpty(variant.Name?.TextId) ? ExternalTextList[variant.Name.TextId].Item : string.Empty;
                        var description = ExternalTextList is not null && !string.IsNullOrEmpty(variant.Description?.TextId) ? ExternalTextList[variant.Description.TextId].Item : string.Empty;

                        if (string.IsNullOrEmpty(name))
                        {
                            name = ExternalTextList is not null && !string.IsNullOrEmpty(variant.ProductName?.TextId) ? ExternalTextList[variant.ProductName.TextId].Item : string.Empty;
                        }
                        if (string.IsNullOrEmpty(description))
                        {
                            description = ExternalTextList is not null && !string.IsNullOrEmpty(variant.ProductText?.TextId) ? ExternalTextList[variant.ProductText.TextId].Item : string.Empty;
                        }

                        Variants.Add(new DeviceVariantDescription
                        {
                            ProductId = variant.ProductId,
                            Symbol = !string.IsNullOrEmpty(folderPath) ? Path.Combine(folderPath, variant.DeviceSymbol) : string.Empty,
                            Icon = !string.IsNullOrEmpty(folderPath) ? Path.Combine(folderPath, variant.DeviceIcon) : string.Empty,
                            Name = name,
                            Description = description,
                        });

                        GraphicsList ??= [];
                        if (!string.IsNullOrEmpty(variant.DeviceSymbol))
                        {
                            GraphicsList.Add($"{variant.ProductId}_Symbol", new($"{variant.ProductId}_Symbol", variant.DeviceSymbol));
                        }
                        if (!string.IsNullOrEmpty(variant.DeviceIcon))
                        {
                            GraphicsList.Add($"{variant.ProductId}_Icon", new($"{variant.ProductId}_Icon", variant.DeviceIcon));
                        }
                    }
                }
            }

        }
        else
        {

        }
    }

    private List<Core.Models.DeviceParameter>? DeviceParameterBuild(object deviceVariable)
    {
        List<Core.Models.DeviceParameter>? parameters = null;

        Dictionary<string, string>? values = null;
        object min = 0, max = 0;
        string description = string.Empty, defaultValue = string.Empty, name = string.Empty;
        ushort index = 0;
        bool isReadOnly = false, isSelected = true;

        object? variable = deviceVariable;
        List<IODD.Variables.RecordItemInfoT>? recordInfo = null;

        switch (deviceVariable)
        {
            case IODD.Variables.VariableCollectionTVariable tVariable:
                description = !string.IsNullOrEmpty(tVariable.Description?.TextId) ? ExternalTextList?[tVariable.Description.TextId].Item ?? string.Empty : string.Empty;
                defaultValue = tVariable.DefaultValue ?? string.Empty;
                name = !string.IsNullOrEmpty(tVariable.Name?.TextId) ? ExternalTextList?[tVariable.Name.TextId].Item ?? string.Empty : string.Empty;
                index = tVariable.Index;

                isReadOnly = tVariable.AccessRights != IODD.Primitives.AccessRightsT.rw;
                isSelected = tVariable.AccessRights == IODD.Primitives.AccessRightsT.rw;

                recordInfo = tVariable.RecordItemInfo?.ToList();
                if (tVariable.Item is IODD.Datatypes.DatatypeRefT dtref)
                {
                    var dt = DatatypeList?[dtref.DatatypeId].Item;
                    variable = dt;
                }
                else
                {
                    variable = tVariable.Item;
                }
                break;
            case IODD.Datatypes.RecordItemT recItem:
                description = !string.IsNullOrEmpty(recItem.Description?.TextId) ? ExternalTextList?[recItem.Description.TextId].Item ?? string.Empty : string.Empty;
                //defaultValue = recItem.DefaultValue ?? string.Empty;
                name = !string.IsNullOrEmpty(recItem.Name?.TextId) ? ExternalTextList?[recItem.Name.TextId].Item ?? string.Empty : string.Empty;
                //index = recItem.Index;

                isReadOnly = recItem.AccessRightRestriction != IODD.Primitives.AccessRightsT.rw && recItem.AccessRightRestrictionSpecified;
                isSelected = (!recItem.AccessRightRestrictionSpecified || (recItem.AccessRightRestriction == IODD.Primitives.AccessRightsT.rw));

                if (recItem.Item is IODD.Datatypes.DatatypeRefT itemDtref)
                {
                    var dt = DatatypeList?[itemDtref.DatatypeId].Item;
                    variable = dt;
                }
                else
                {
                    variable = recItem.Item;
                }
                break;
        }

        switch (variable)
        {
            case IODD.Datatypes.ProcessDataOutUnionT pduOut:
            case IODD.Datatypes.ProcessDataInUnionT pduIn:
            case IODD.Datatypes.ProcessDataUnionT pdu:
                break;
            case IODD.Datatypes.RecordT recvar:
                if (recvar.RecordItem is not null)
                {
                    parameters ??= [];
                    parameters.Add(new Core.Models.DeviceParameter
                    {
                        Name = name,
                        Index = index,
                        //Subindex = ,
                        IsReadOnly = isReadOnly,
                        IsSelected = isSelected,
                        DataType = Core.Models.DeviceDatatypes.RecordT,
                        BitLength = recvar.BitLength,
                        Description = description,
                    });

                    foreach (var record in recvar.RecordItem)
                    {
                        var subParameters = DeviceParameterBuild(record);
                        if (subParameters is not null)
                        {
                            foreach (var subParameter in subParameters.Where(w => w is not null))
                            {
                                var def = recordInfo?.Find(f => f.Subindex == record.Subindex)?.DefaultValue;
                                string deftValue = !string.IsNullOrEmpty(def) ? def : subParameter.DefaultValue;
                                if (subParameter?.Values is not null && !string.IsNullOrEmpty(deftValue))
                                {
                                    deftValue = subParameter.Values?.FirstOrDefault(param => param.Key.Equals(deftValue, StringComparison.InvariantCultureIgnoreCase)).Key ?? deftValue;
                                }

                                parameters.Add(new Core.Models.DeviceParameter
                                {
                                    Name = subParameter!.Name,
                                    Index = index,
                                    Subindex = record.Subindex,
                                    IsReadOnly = isReadOnly || (record.AccessRightRestriction != IODD.Primitives.AccessRightsT.rw && record.AccessRightRestrictionSpecified),
                                    IsSelected = isSelected, //(record.Subindex >= 60) && (!record.AccessRightRestrictionSpecified || (record.AccessRightRestriction != AccessRightsT.ro)),
                                    IsVisible = false,
                                    DataType = subParameter.DataType,
                                    BitLength = subParameter.BitLength,
                                    BitOffset = subParameter.BitLength % 8 != 0 ? record.BitOffset : null,
                                    Description = subParameter.Description,
                                    DefaultValue = deftValue,
                                    Values = subParameter.Values,
                                    Minimum = subParameter.Minimum,
                                    Maximum = subParameter.Maximum
                                });
                            }
                        }
                    }
                }

                break;
            case IODD.Datatypes.ArrayT arrvar:
            case IODD.Datatypes.ComplexDatatypeT complexvar:
            case IODD.Datatypes.TimeSpanT timespanvar:
            case IODD.Datatypes.TimeT timevar:
                break;
            case IODD.Datatypes.StringT stringvar:
                parameters ??= [];

                parameters.Add(new Core.Models.DeviceParameter
                {
                    Name = name,
                    Index = index,
                    //Subindex = ,
                    IsReadOnly = isReadOnly,
                    IsSelected = isSelected,
                    DataType = Core.Models.DeviceDatatypes.StringT,
                    FixedLength = stringvar.FixedLength,
                    //BitLength = boolvar.
                    Description = description,
                    DefaultValue = !string.IsNullOrEmpty(defaultValue) ? defaultValue : string.Empty,
                    //Values = values,
                    //Minimum = min,
                    //Maximum = max
                });
                break;
            case IODD.Datatypes.OctetStringT octectvar:
                break;
            case IODD.Datatypes.BooleanT boolvar:
                (min, values, max) = ValueDatatypeBoolean(boolvar);
                parameters ??= [];

                parameters.Add(new Core.Models.DeviceParameter
                {
                    Name = name,
                    Index = index,
                    //Subindex = ,
                    IsReadOnly = isReadOnly,
                    IsSelected = isSelected,
                    DataType = Core.Models.DeviceDatatypes.BooleanT,
                    BitLength = 1,
                    Description = description,
                    DefaultValue = !string.IsNullOrEmpty(defaultValue) ? defaultValue : bool.FalseString,
                    Values = values,
                    Minimum = min,
                    Maximum = max
                });
                break;
            case IODD.Datatypes.Float32T floatvar:
                (min, values, max) = ValueDatatypeFloat(floatvar);
                parameters ??= [];
                parameters.Add(new Core.Models.DeviceParameter
                {
                    Name = name,
                    Index = index,
                    //Subindex = ,
                    IsReadOnly = isReadOnly,
                    IsSelected = isSelected,
                    DataType = Core.Models.DeviceDatatypes.Float32T,
                    Description = description,
                    DefaultValue = !string.IsNullOrEmpty(defaultValue) ? defaultValue : "0",
                    Values = values,
                    Minimum = min,
                    Maximum = max
                });
                break;
            case IODD.Datatypes.UIntegerT uintvar:
                (min, values, max) = ValueDatatypeUInteger(uintvar);
                parameters ??= [];
                parameters.Add(new Core.Models.DeviceParameter
                {
                    Name = name,
                    Index = index,
                    //Subindex = ,
                    IsReadOnly = isReadOnly,
                    IsSelected = isSelected,
                    DataType = Core.Models.DeviceDatatypes.UIntegerT,
                    BitLength = uintvar.BitLength,
                    Description = description,
                    DefaultValue = !string.IsNullOrEmpty(defaultValue) ? defaultValue : $"{uint.MinValue}",
                    Values = values,
                    Minimum = min,
                    Maximum = max
                });
                break;
            case IODD.Datatypes.IntegerT intvar:
                (min, values, max) = ValueDatatypeInteger(intvar);
                parameters ??= [];
                parameters.Add(new Core.Models.DeviceParameter
                {
                    Name = name,
                    Index = index,
                    //Subindex = ,
                    IsReadOnly = isReadOnly,
                    IsSelected = isSelected,
                    DataType = Core.Models.DeviceDatatypes.IntegerT,
                    BitLength = intvar.BitLength,
                    Description = description,
                    DefaultValue = !string.IsNullOrEmpty(defaultValue) ? defaultValue : "0",
                    Values = values,
                    Minimum = min,
                    Maximum = max
                });
                break;
            case IODD.Datatypes.NumberT numbervar:
            case IODD.Datatypes.SimpleDatatypeT simplevar:
                break;
        }

        return parameters;
    }


    private (bool, Dictionary<string, string>?, bool) ValueDatatypeBoolean(IODD.Datatypes.BooleanT boolType)
    {
        Dictionary<string, string>? values = null;
        bool min = false, max = true;
        if (boolType?.SingleValue is not null)
        {
            switch (boolType.SingleValue)
            {
                case IODD.Datatypes.BooleanValueT[] booleanValues:
                    values = [];
                    foreach (var item in booleanValues)
                    {
                        if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                        {
                            values.Add(item.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                        }
                    }
                    break;
            }
        }
        return (min, values, max);
    }

    private (ulong, Dictionary<string, string>?, ulong) ValueDatatypeUInteger(IODD.Datatypes.UIntegerT uIntType)
    {
        Dictionary<string, string>? values = null;
        ulong min = 0, max = 0;
        if (uIntType?.Items is not null)
        {
            switch (uIntType.Items)
            {
                case IODD.Datatypes.UIntegerValueT[] uIntegerValues:
                    values = [];
                    foreach (var item in uIntegerValues)
                    {
                        if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                        {
                            values.Add(item.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                        }
                    }
                    break;
                default:
                    foreach (var item in uIntType.Items)
                    {
                        switch (item)
                        {
                            case IODD.Datatypes.UIntegerValueT uIntegerValues:
                                if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                                {
                                    values ??= [];
                                    values.Add(uIntegerValues.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                                }

                                break;
                            case IODD.Datatypes.UIntegerValueRangeT uIntegerValueRange:
                                min = uIntegerValueRange.LowerValue;
                                max = uIntegerValueRange.UpperValue;
                                break;
                        }
                    }
                    break;
            }
        }
        return (min, values, max);
    }

    private (long, Dictionary<string, string>?, long) ValueDatatypeInteger(IODD.Datatypes.IntegerT intType)
    {
        Dictionary<string, string>? values = null;
        long min = 0, max = 0;
        if (intType?.Items is not null)
        {
            switch (intType.Items)
            {
                case IODD.Datatypes.IntegerValueT[] integerValues:
                    values = [];
                    foreach (var item in integerValues)
                    {
                        if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                        {
                            values.Add(item.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                        }
                    }
                    break;
                default:
                    foreach (var item in intType.Items)
                    {
                        switch (item)
                        {
                            case IODD.Datatypes.IntegerValueT integerValues:
                                if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                                {
                                    values ??= [];
                                    values.Add(integerValues.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                                }

                                break;
                            case IODD.Datatypes.IntegerValueRangeT integerValueRange:
                                min = integerValueRange.LowerValue;
                                max = integerValueRange.UpperValue;
                                break;
                        }
                    }
                    break;
            }
        }
        return (min, values, max);
    }

    private (float, Dictionary<string, string>?, float) ValueDatatypeFloat(IODD.Datatypes.Float32T floatType)
    {
        Dictionary<string, string>? values = null;
        float min = 0, max = 0;
        if (floatType?.Items is not null)
        {
            switch (floatType.Items)
            {
                case IODD.Datatypes.Float32ValueT[] floatValues:
                    values = [];
                    foreach (var item in floatValues)
                    {
                        if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                        {
                            values.Add(item.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                        }
                    }
                    break;
                default:
                    foreach (var item in floatType.Items)
                    {
                        switch (item)
                        {
                            case IODD.Datatypes.Float32ValueT floatValues:
                                if (!string.IsNullOrEmpty(item.Name?.TextId) && ExternalTextList is not null)
                                {
                                    values ??= [];
                                    values.Add(floatValues.Value.ToString(), ExternalTextList[item.Name.TextId].Item);
                                }

                                break;
                            case IODD.Datatypes.Float32ValueRangeT floatValueRange:
                                min = floatValueRange.LowerValue;
                                max = floatValueRange.UpperValue;
                                break;
                        }
                    }
                    break;
            }
        }
        return (min, values, max);
    }

}
