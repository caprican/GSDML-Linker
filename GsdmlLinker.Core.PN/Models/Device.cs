﻿using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

using GSDML = ISO15745.GSDML;
using System.Collections.ObjectModel;

namespace GsdmlLinker.Core.PN.Models;

public abstract record Device : Core.Models.Device
{
    public abstract string GetLastIndentNumber(List<string> identNumberList);
    public abstract ushort GetModuleVendorId(GSDML.DeviceProfile.ParameterRecordDataT[]? recordDatas);
    public abstract string SetModuleVendorId(ushort Id);
    public abstract uint GetModuleDeviceId(GSDML.DeviceProfile.ParameterRecordDataT[]? recordDatas);
    public abstract string SetModuleDeviceId(uint Id);

    public List<string> IdentNumberList { get; set; } = [];
    public List<ModuleItem>? ModuleList { get; set; }
    public List<SubmoduleItem>? SubmoduleList { get; set; }
    public Dictionary<string, CategoryItem>? CategoryList { get; init; }
    public Dictionary<string, ValueItem>? ValueList { get; init; }

    public Device(string filePath, Match? match) : base (filePath)
    {
        if (match is null) return;

        //GSDML-V<Version>-<NomFabricant>-<NomProduit>-<Date>(-<heure>)?(-<langue>)?(-<Commentaire>)?
        SchematicVersion = match.Groups[1].Value;
        ManufacturerName = match.Groups[3].Value;
        DeviceFamily = match.Groups[4].Value;
        var lang = match.Groups[7].Value;
        var txtFree = match.Groups[8].Value;

        var dateMatch = Regexs.DateRegex().Match(match.Groups[5].Value);
        var timeMatch = Regexs.TimeRegex().Match(match.Groups[6].Value);
        Version = new DateTime(int.Parse(dateMatch.Groups[1].Value), int.Parse(dateMatch.Groups[2].Value), int.Parse(dateMatch.Groups[3].Value),
            timeMatch.Success ? int.Parse(timeMatch.Groups[1].Value) : 0, timeMatch.Success ? int.Parse(timeMatch.Groups[2].Value) : 0, timeMatch.Success ? int.Parse(timeMatch.Groups[3].Value) : 0);

        var serializer = new XmlSerializer(typeof(GSDML.DeviceProfile.ISO15745Profile));

        using var reader = XmlReader.Create(filePath);
        var device = serializer.Deserialize(reader) as GSDML.DeviceProfile.ISO15745Profile;

        VendorId = device?.ProfileBody?.DeviceIdentity?.VendorID ?? string.Empty;
        DeviceId = device?.ProfileBody?.DeviceIdentity?.DeviceID ?? string.Empty;


        if (device?.ProfileBody?.ApplicationProcess?.ExternalTextList?.PrimaryLanguage is GSDML.Primitives.ExternalTextT[] extTexts)
        {
            ExternalTextList = [];
            foreach (var extText in extTexts)
            {
                if(!string.IsNullOrEmpty(extText?.TextId) && extText?.Value is not null)
                {
                    ExternalTextList.Add(extText.TextId, new(extText.TextId, extText.Value));
                }
            }
        }
        VendorName = device?.ProfileBody?.DeviceIdentity?.VendorName?.Value ?? string.Empty;

        if (device?.ProfileBody?.ApplicationProcess?.CategoryList is not null)
        {
            CategoryList = [];
            foreach (var item in device.ProfileBody.ApplicationProcess.CategoryList)
            {
                if (!string.IsNullOrEmpty(item.ID) && !string.IsNullOrEmpty(item.TextId))
                {
                    CategoryList.Add(item.ID, new(item.ID, item.TextId));
                }
            }
        }

        if (device?.ProfileBody?.ApplicationProcess?.GraphicsList is not null)
        {
            GraphicsList = [];
            foreach (var item in device.ProfileBody.ApplicationProcess.GraphicsList)
            {
                if (item.GraphicFile is string gFile && !string.IsNullOrEmpty(item.ID))
                {
                    GraphicsList.Add(item.ID, new Core.Models.GraphicItem(item.ID, gFile));
                }
            }
        }

        if(device?.ProfileBody?.ApplicationProcess?.ValueList is not null)
        {
            ValueList = [];
            foreach (var value in device.ProfileBody.ApplicationProcess.ValueList)
            {
                if(value.Assignments is GSDML.DeviceProfile.Assign[] assigments && !string.IsNullOrEmpty(value.ID))
                {
                    ValueList.Add(value.ID, new(value.ID, assigments));
                }
            }
        }

        if (device?.ProfileBody?.ApplicationProcess?.SubmoduleList is not null)
        {
            SubmoduleList = [];
            foreach (var submodule in device.ProfileBody.ApplicationProcess.SubmoduleList)
            {
                if(submodule is GSDML.DeviceProfile.SubmoduleItemT item) SubmoduleList.Add(new SubmoduleItem(item));
            }
        }

        if(device?.ProfileBody?.ApplicationProcess?.ModuleList is not null)
        { 
            ModuleList = [];
            foreach (var module in device.ProfileBody.ApplicationProcess.ModuleList)
            {
                ModuleList.Add(new ModuleItem(module));
            }
        }

        if (device?.ProfileBody?.DeviceIdentity is GSDML.DeviceProfile.DeviceIdentityT deviceIdentity)
        {
            if(!string.IsNullOrEmpty(deviceIdentity.InfoText?.TextId) && ExternalTextList is not null)
            {
                Description = ExternalTextList?[deviceIdentity.InfoText.TextId].Item;
            }

            if (device?.ProfileBody?.ApplicationProcess?.DeviceAccessPointList is GSDML.DeviceProfile.DeviceAccessPointItemT[] deviceAccessPoints)
            {
                foreach (var dap in deviceAccessPoints)
                {
                    var symbolPath = string.Empty;
                    var iconPath = string.Empty;
                    if (dap.Graphics is not null)
                    {
                        var folderPath = Path.GetDirectoryName(filePath)!;
                        foreach (var graphic in dap.Graphics)
                        {
                            switch (graphic.Type)
                            {
                                case GSDML.Primitives.GraphicsTypeEnumT.DeviceSymbol:
                                    if (!string.IsNullOrEmpty(graphic.GraphicItemTarget) && GraphicsList is not null)
                                    {
                                        symbolPath = Path.Combine(folderPath, GraphicsList[graphic.GraphicItemTarget].Item) + ".bmp";
                                    }
                                    break;
                                case GSDML.Primitives.GraphicsTypeEnumT.DeviceIcon:
                                    if (!string.IsNullOrEmpty(graphic.GraphicItemTarget) && GraphicsList is not null)
                                    {
                                        iconPath = Path.Combine(folderPath, GraphicsList[graphic.GraphicItemTarget].Item) + ".bmp";
                                    }
                                    break;
                            }
                        }
                    }

                    string? softVersion = dap.ModuleInfo?.SoftwareRelease?.Value;
                    List<Core.Models.Module> modules = [];
                    if (dap.UseableModules is not null)
                    {
                        //UseableModules = [.. dap.UseableModules];
                        foreach (var useableModule in dap.UseableModules)
                        {
                            var module = ModuleList?.Find(f => f.ID == useableModule.ModuleItemTarget);
                            if (module is null) break;

                            ObservableCollection<Core.Models.Module> submodules = [];
                            if(module?.UseableSubmodules is not null)
                            {
                                //UseableSubmodules = [.. module.UseableSubmodules];
                                foreach (var useableSubmodule in module.UseableSubmodules)
                                {
                                    var submodule = SubmoduleList?.Find(f => f.ID == useableSubmodule.SubmoduleItemTarget);
                                    if (submodule is null) break;

                                    var ProfileParameter = submodule.RecordDataList?.ParameterRecordDataItem?.FirstOrDefault(param => param.Index == ProfileParameterIndex);
                                    var vendorRecord = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == VendorIdSubIndex);
                                    var deviceRecord = (GSDML.DeviceProfile.RecordDataRefT?)ProfileParameter?.Items?.FirstOrDefault(param => param is GSDML.DeviceProfile.RecordDataRefT recordData && recordData.ByteOffset == DeviceIdSubIndex);

                                    submodule.Name = (!string.IsNullOrEmpty(submodule.ModuleInfo?.Name?.TextId) ? ExternalTextList?[submodule.ModuleInfo.Name.TextId].Item : string.Empty) ?? string.Empty;
                                    submodule.Description = !string.IsNullOrEmpty(submodule.ModuleInfo?.InfoText?.TextId) ? ExternalTextList?[submodule.ModuleInfo.InfoText.TextId].Item : string.Empty;
                                    submodule.CategoryRef = GetCategoryText(submodule.ModuleInfo?.CategoryRef);
                                    submodule.SubCategoryRef = GetCategoryText(submodule.ModuleInfo?.SubCategory1Ref);
                                    submodule.VendorId = GetModuleVendorId(submodule.RecordDataList?.ParameterRecordDataItem);
                                    submodule.DeviceId = GetModuleDeviceId(submodule.RecordDataList?.ParameterRecordDataItem);
                                    submodules.Add(submodule);
                                }
                            }

                            module!.Name = (!string.IsNullOrEmpty(module!.ModuleInfo?.Name?.TextId) ? ExternalTextList?[module.ModuleInfo.Name.TextId].Item : string.Empty) ?? string.Empty;
                            module.Description = !string.IsNullOrEmpty(module.ModuleInfo?.InfoText?.TextId) ? ExternalTextList?[module.ModuleInfo.InfoText.TextId].Item : string.Empty;
                            module.CategoryRef = GetCategoryText(module.ModuleInfo?.CategoryRef);
                            module.SubCategoryRef = GetCategoryText(module.ModuleInfo?.SubCategory1Ref);
                            module.VendorId = GetModuleVendorId(module.VirtualSubmoduleList?.First().RecordDataList?.ParameterRecordDataItem);
                            module.DeviceId = GetModuleDeviceId(module.VirtualSubmoduleList?.First().RecordDataList?.ParameterRecordDataItem);
                            module.Submodules = submodules;
                            modules.Add(module);
                        }
                    }

                    DeviceAccessPoints.Add(new Core.Models.DeviceAccessPoint
                    {
                        Id = dap.ID!,
                        Name = !string.IsNullOrEmpty(dap.ModuleInfo?.Name?.TextId) && ExternalTextList is not null ? ExternalTextList[dap.ModuleInfo.Name.TextId].Item : string.Empty,
                        Description = !string.IsNullOrEmpty(dap.ModuleInfo?.InfoText?.TextId) && ExternalTextList is not null ? ExternalTextList[dap.ModuleInfo.InfoText.TextId].Item : string.Empty,
                        ProductId = dap.ModuleInfo?.OrderNumber?.Value,
                        DNS = dap.DNS_CompatibleName,
                        PhysicalSlots = dap.PhysicalSlots,
                        FixedInSlots = dap.FixedInSlots,

                        Version = Version,
                        SoftwareRelease = string.IsNullOrEmpty(softVersion) ? null : new System.Version(new string(softVersion.Where(c => char.IsDigit(c) || char.IsPunctuation(c)).ToArray())),

                        Symbol = symbolPath,
                        Icon = iconPath,

                        Modules = modules,
                    });
                }
            }
        }

        if (device?.ProfileBody?.ApplicationProcess?.ModuleList is not null)
        {
            foreach (var item in device.ProfileBody.ApplicationProcess.ModuleList)
            {
                if (!string.IsNullOrEmpty(item.ModuleIdentNumber))
                {
                    IdentNumberList.Add(item.ModuleIdentNumber);
                }
            }
        }

        if (device?.ProfileBody?.ApplicationProcess?.SubmoduleList is not null)
        {
            foreach (var item in device.ProfileBody.ApplicationProcess.SubmoduleList)
            {
                switch (item)
                {
                    case GSDML.DeviceProfile.PortSubmoduleItemT portSubmoduleItem:
                        if (portSubmoduleItem.SubmoduleIdentNumber is not null)
                        {
                            IdentNumberList.Add(portSubmoduleItem.SubmoduleIdentNumber);
                        }
                        break;
                    case GSDML.DeviceProfile.SubmoduleItemT submoduleItem:
                        if (submoduleItem.SubmoduleIdentNumber is not null)
                        {
                            IdentNumberList.Add(submoduleItem.SubmoduleIdentNumber);
                        }
                        break;
                }

            }
        }
    }

    public string GetCategoryText(string? categoryId)
    {
        if (string.IsNullOrEmpty(categoryId)) return string.Empty;

        var category = CategoryList?[categoryId];
        if(string.IsNullOrEmpty(category?.Item)) return string.Empty;

        return ExternalTextList?[category.Item].Item ?? string.Empty;
    }
}
