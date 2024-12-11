using GsdmlLinker.Contracts.Builders;

namespace GsdmlLinker.Builders;

public class ModuleBuilder(Core.PN.Contracts.Services.IDevicesService gsdDevicesService, Core.IOL.Contracts.Services.IDevicesService iolDevicesService,
    Core.PN.Contracts.Factories.IDevicesFactory devicesFactory) : IModuleBuilder
{
    private readonly Core.PN.Contracts.Services.IDevicesService gsdDevicesService = gsdDevicesService;
    private readonly Core.IOL.Contracts.Services.IDevicesService iolDevicesService = iolDevicesService;
    private readonly Core.PN.Contracts.Factories.IDevicesFactory devicesFactory = devicesFactory;

    private Core.PN.Contracts.Builders.IModuleBuilder? Builder { get; set; }
    private Core.PN.Models.Device? MasterDevice { get; set; }
    private Core.IOL.Models.Device? Device { get; set; }

    public void CreateModule(Models.DeviceItem masterDevice, Models.DeviceItem slaveDevice, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters)
    {
        MasterDevice = masterDevice.Device as Core.PN.Models.Device;
        Device = slaveDevice.Device as Core.IOL.Models.Device;

        string cateroryRef = string.Empty;
        string indentNumber = string.Empty;
        string categoryVendor = string.Empty;
        if (MasterDevice is null || Device is null) return;

        Builder = devicesFactory.CreateModule(MasterDevice);
        if (Builder is null) return;

        //MasterDevice.BeginEdit();
        if (MasterDevice.ExternalTextList is not null && MasterDevice.CategoryList is not null)
        {
            cateroryRef = AddCategoryRef(MasterDevice.ExternalTextList, MasterDevice.CategoryList);
        }

        categoryVendor = AddCategoryVendor(MasterDevice.ExternalTextList!, MasterDevice.CategoryList!, Device.VendorId, Device.VendorName);
        indentNumber = MasterDevice.GetLastIndentNumber(MasterDevice.IdentNumberList);

        Builder.CreateRecordParameters(Device, Device.DataStorage, Device.SupportBlockParameter, indentNumber, parameters, slaveDevice.UnlockId);

        if (Device.ProcessDatas is not null)
        {
            Builder.CreateDataProcess(indentNumber, Device.ProcessDatas);
        }

        //List<GSDML.DeviceProfile.GraphicsReferenceTGraphicItemRef>? graphics = null;
        //List<string>? pictures = null;
        //if (!string.IsNullOrEmpty(productId))
        //{
        //    pictures = [];
        //    graphics = [];
        //    var variant = slave.Description.Variants?.FirstOrDefault(f => f.ProductId == productId);
        //    if (!string.IsNullOrEmpty(variant?.Symbol))
        //    {
        //        pictures.Add(variant.Value.Symbol);
        //        var graphicId = $"GI-{Path.GetFileNameWithoutExtension(variant.Value.Symbol)}";
        //        GraphicsList?.Add(graphicId, Path.GetFileNameWithoutExtension(variant.Value.Symbol));

        //        graphics.Add(new GSDML.DeviceProfile.GraphicsReferenceTGraphicItemRef
        //        {
        //            Type = GSDML.Primitives.GraphicsTypeEnumT.DeviceSymbol,
        //            GraphicItemTarget = graphicId,
        //        });

        //    }

        //    if (!string.IsNullOrEmpty(variant?.Icon))
        //    {
        //        pictures.Add(variant.Value.Icon);
        //        var graphicId = $"GI-{Path.GetFileNameWithoutExtension(variant.Value.Icon)}";
        //        GraphicsList?.Add(graphicId, Path.GetFileNameWithoutExtension(variant.Value.Icon));

        //        graphics.Add(new GSDML.DeviceProfile.GraphicsReferenceTGraphicItemRef
        //        {
        //            Type = GSDML.Primitives.GraphicsTypeEnumT.DeviceIcon,
        //            GraphicItemTarget = graphicId,
        //        });
        //    }
        //}
        
        Builder.BuildModule(Device, indentNumber, cateroryRef, categoryVendor, Device.Name ?? "");
        MasterDevice.IdentNumberList.Add(indentNumber);
    }

    public void ModifiedModule(Models.DeviceItem masterDevice, Models.DeviceItem slaveDevice, IEnumerable<IGrouping<ushort, Core.Models.DeviceParameter>> parameters, string moduleID)
    {
        MasterDevice = masterDevice.Device as Core.PN.Models.Device;
        Device = slaveDevice.Device as Core.IOL.Models.Device;

        string cateroryRef = string.Empty;
        string indentNumber = string.Empty;
        string categoryVendor = string.Empty;
        if (MasterDevice is null || Device is null) return;

        Builder = devicesFactory.CreateModule(MasterDevice);
        if (Builder is null) return;

        if (MasterDevice.ExternalTextList is not null && MasterDevice.CategoryList is not null)
        {
            cateroryRef = AddCategoryRef(MasterDevice.ExternalTextList, MasterDevice.CategoryList);
        }

        categoryVendor = AddCategoryVendor(MasterDevice.ExternalTextList!, MasterDevice.CategoryList!, Device.VendorId, Device.VendorName);
        indentNumber = MasterDevice.IdentNumberList.Find(f => moduleID.EndsWith(f)) ?? string.Empty;
        
        Builder.CreateRecordParameters(Device, Device.DataStorage, Device.SupportBlockParameter, indentNumber, parameters, slaveDevice.UnlockId);

        if (Device.ProcessDatas is not null)
        {
            Builder.CreateDataProcess(indentNumber, Device.ProcessDatas);
        }

        Builder.UpdateModule(Device, indentNumber, cateroryRef, categoryVendor, Device.Name ?? "");
    }

    public void DeletedModule(Models.DeviceItem masterDevice, string moduleId)
    {
        MasterDevice = masterDevice.Device as Core.PN.Models.Device;

        if (MasterDevice is null) return;
        Builder = devicesFactory.CreateModule(MasterDevice);

        if (Builder is null) return;
        Builder.DeletModule(moduleId);
    }

    internal string AddCategoryRef(Dictionary<string, Core.Models.ExternalTextItem> externalText, Dictionary<string, Core.PN.Models.CategoryItem> categories)
    {
        var catRefText = externalText.FirstOrDefault(txt => txt.Value.Item == "IO-Link Devices").Key;
        var categoryRef = categories.FirstOrDefault(cat => cat.Value.Item == catRefText).Key;

        if (string.IsNullOrEmpty(categoryRef))
        {
            categoryRef = "IOL_Devices_Category";
            MasterDevice?.ExternalTextList?.TryAdd("Devices_Category_Text", new("Devices_Category_Text", "IO-Link Devices") { State = Core.Models.ItemState.Created });
            MasterDevice?.CategoryList?.TryAdd(categoryRef, new(categoryRef, "Devices_Category_Text") { State = Core.Models.ItemState.Created });
        }
        return categoryRef;
    }

    internal string AddCategoryVendor(Dictionary<string, Core.Models.ExternalTextItem> externalText, Dictionary<string, Core.PN.Models.CategoryItem> categories, string vendorId, string vendorName)
    {
        var categoryVendor = externalText.FirstOrDefault(txt => txt.Value.Item == vendorName).Key;
        categoryVendor = categories?.FirstOrDefault(txtId => txtId.Value.Item == categoryVendor).Key;
        if (string.IsNullOrEmpty(categoryVendor) && MasterDevice?.ExternalTextList?.ContainsKey($"Devices_CategoryVendor{vendorId}_Text") == false)
        {
            categoryVendor = $"Devices_Category_{vendorId}_SubCategory";
            MasterDevice.ExternalTextList?.TryAdd($"Devices_CategoryVendor{vendorId}_Text", new($"Devices_CategoryVendor{vendorId}_Text", vendorName) { State = Core.Models.ItemState.Created });
            MasterDevice?.CategoryList?.TryAdd(categoryVendor, new(categoryVendor, $"Devices_CategoryVendor{vendorId}_Text") { State = Core.Models.ItemState.Created });
        }

        return categoryVendor ?? string.Empty;
    }

}
