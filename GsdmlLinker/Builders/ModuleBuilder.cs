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

    public void CreateModule(Models.DeviceItem masterDevice, Models.DeviceItem slaveDevice, IEnumerable<IGrouping< ushort, Core.Models.DeviceParameter>> parameters)
    {
        MasterDevice = masterDevice.device as Core.PN.Models.Device;
        Device = slaveDevice.device as Core.IOL.Models.Device;

        string cateroryRef = string.Empty;
        if (MasterDevice is null || Device is null) return;

        Builder = devicesFactory.CreateModule(MasterDevice, Device);
        if (Builder is null) return;

        //MasterDevice.BeginEdit();

        if (MasterDevice.ExternalTextList is not null && MasterDevice.CategoryList is not null)
        {
            cateroryRef = AddCategoryRef(MasterDevice.ExternalTextList, MasterDevice.CategoryList);
        }

        var categoryVendor = AddCategoryVendor(MasterDevice.ExternalTextList!, MasterDevice.CategoryList!, Device.VendorId, Device.VendorName);
        var indentNumber = MasterDevice.GetLastIndentNumber(MasterDevice.IdentNumberList);

        Builder.CreateRecordParameters(Device.DataStorage, Device.SupportBlockParameter, indentNumber, parameters);

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

        Builder.BuildModule(indentNumber, cateroryRef, categoryVendor, Device.Name ?? "");
        MasterDevice.IdentNumberList.Add(indentNumber);
    }

    internal string AddCategoryRef(Dictionary<string, string> externalText, Dictionary<string, string> categories)
    {
        var catRefText = externalText.FirstOrDefault(txt => txt.Value == "IO-Link Devices").Key;
        var categoryRef = categories.FirstOrDefault(cat => cat.Value == catRefText).Key;

        if (string.IsNullOrEmpty(categoryRef))
        {
            MasterDevice?.ExternalTextList?.Add("Devices_Category_Text", "IO-Link Devices");
            MasterDevice?.CategoryList?.Add("IOL_Devices_Category", "Devices_Category_Text");
            categoryRef = "IOL_Devices_Category";
        }
        return categoryRef;
    }

    internal string AddCategoryVendor(Dictionary<string, string> externalText, Dictionary<string, string> categories, string vendorId, string vendorName)
    {
        var categoryVendor = externalText.FirstOrDefault(txt => txt.Value == vendorName).Key;
        categoryVendor = categories?.FirstOrDefault(txtId => txtId.Value == categoryVendor).Key;
        if (string.IsNullOrEmpty(categoryVendor))
        {
            if (MasterDevice?.ExternalTextList?.ContainsKey($"Devices_CategoryVendor{vendorId}_Text") == false)
            {
                MasterDevice.ExternalTextList?.Add($"Devices_CategoryVendor{vendorId}_Text", vendorName);
                MasterDevice?.CategoryList?.Add($"Devices_Category_{vendorId}_SubCategory", $"Devices_CategoryVendor{vendorId}_Text");
            }
            categoryVendor = $"Devices_Category_{vendorId}_SubCategory";
        }

        return categoryVendor;
    }

}
