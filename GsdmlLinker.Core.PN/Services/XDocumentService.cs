using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Linq;

using GsdmlLinker.Core.PN.Contracts.Services;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

using GSDML = ISO15745.GSDML;
using System.Xml;

namespace GsdmlLinker.Core.PN.Services;

public class XDocumentService(IOptions<Core.Models.AppConfig> appConfig) : IXDocumentService
{
    private readonly IOptions<Core.Models.AppConfig> appConfig = appConfig;

    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private XDocument? xDevice;
    private string? manufacturerName;
    private string? schematicVersion;
    private string? nameDeviceFamily;

    public bool SaveAs(string path)
    {
        if (xDevice is null)
        {
            return false;
        }
        xDevice.Save(path);
        return true;
    }

    public bool Load(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        if (Regexs.FileNameRegex().Match(fileName) is Match GSDMLmatch && GSDMLmatch.Success)
        {
            schematicVersion = GSDMLmatch.Groups[1].Value;
            manufacturerName = GSDMLmatch.Groups[2].Value;
            nameDeviceFamily = GSDMLmatch.Groups[3].Value;

            xDevice = XDocument.Load(filePath);
            return true;
        }
        else
        {
            return false;
        }
    }

    public (string?, string?, List<string>?) Create(Models.Device? device, string path)
    {
        if (string.IsNullOrEmpty(device?.FilePath)) return (null, null, null);

        Load(device.FilePath);

        if (device is null || xDevice is null) return (null, null, null);

        List<string>? graphicsPath = null;
        var localFilePath = Path.Combine(localAppData, appConfig.Value.GSDMLFolder, device.ManufacturerName!, $"{device.DeviceFamily}-V{device.SchematicVersion}");
        var fileName = $"GSDML-V{device.SchematicVersion}-{device.ManufacturerName}-{device.DeviceFamily}-{DateTime.Now:yyyyMMdd-HHmm00}";
        if (!Directory.Exists(localFilePath))
        {
            Directory.CreateDirectory(localFilePath);
        }

        var ns = xDevice.Root!.GetDefaultNamespace();

        var encoding = Encoding.GetEncoding(xDevice.Declaration?.Encoding!);

        var deviceAccessPointList = (from d in xDevice.Descendants() where d.Name.LocalName == "DeviceAccessPointList" select d).FirstOrDefault();
        var moduleList = (from d in xDevice.Descendants() where d.Name.LocalName == "ModuleList" select d).FirstOrDefault();
        var submoduleList = (from d in xDevice.Descendants() where d.Name.LocalName == "SubmoduleList" select d).FirstOrDefault();
        var valueList = (from d in xDevice.Descendants() where d.Name.LocalName == "ValueList" select d).FirstOrDefault();

        var categoryList = (from d in xDevice.Descendants() where d.Name.LocalName == "CategoryList" select d).FirstOrDefault();
        var graphicList = (from d in xDevice.Descendants() where d.Name.LocalName == "GraphicsList" select d).FirstOrDefault();
        
        var ExternalTextList = (from d in xDevice.Descendants() where d.Name.LocalName == "ExternalTextList" select d).FirstOrDefault();
        var primaryLanguage = (from d in xDevice.Descendants() where d.Name.LocalName == "PrimaryLanguage" select d).FirstOrDefault();
        
        var xCommentElement = (from d in xDevice.Nodes() where d is XComment select d).FirstOrDefault();
        var deviceIdentity = (from d in xDevice.Descendants() where d.Name.LocalName == "DeviceIdentity" select d).FirstOrDefault();

        var resumeActions = "";

        if (device.ModuleList?.Count > 0 && moduleList is not null && deviceAccessPointList is not null)
        {
            foreach (var module in device.ModuleList)
            {
                switch(module.State)
                {
                    case Core.Models.ItemState.Created:
                        using (var memoryStream = new MemoryStream())
                        {
                            using TextWriter streamWriter = new StreamWriter(memoryStream);
                            moduleList.Add(new XComment($"{module.ModuleInfo?.OrderNumber?.Value} IO-Link device"));
                            var subList = new List<GSDML.DeviceProfile.ModuleItemT> { module.Item };

                            var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.ModuleItemT[]), ns.NamespaceName);
                            xmlSerializer.Serialize(streamWriter, subList.ToArray());
                            var xelement = XElement.Parse(encoding.GetString(memoryStream.ToArray()));

                            moduleList.Add(xelement.Elements().First());

                            foreach (var deviceAccessPointItem in deviceAccessPointList.Descendants(ns + "UseableModules"))
                            {
                                var deviceAccessPoint = device.DeviceAccessPoints.First(f => f.Id == deviceAccessPointItem.Attribute("ID")!.Value);
                                var allowedInSlots = deviceAccessPoint.PhysicalSlots;

                                if (allowedInSlots?.Contains("..") == true)
                                {
                                    var split = allowedInSlots.Split("..");
                                    var min = int.Parse(split[0]);
                                    var max = int.Parse(split[1]);
                                    var fixedInSlots = int.Parse(deviceAccessPoint.FixedInSlots ?? "0");

                                    min = (fixedInSlots > 0 ? fixedInSlots + 1 : min);

                                    if (min == max)
                                        allowedInSlots = $"{min}";
                                    else
                                        allowedInSlots = $"{min}..{max}";
                                }

                                var useableMmodules = deviceAccessPointItem.Descendants(ns + "UseableModules").FirstOrDefault();
                                useableMmodules!.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
                                useableMmodules!.Add(new XElement(ns + "ModuleItemRef", new XAttribute("ModuleItemTarget", module.ID!),
                                                                                                new XAttribute("AllowedInSlots", allowedInSlots!)));
                            }

                            resumeActions += HistoryRelease($"create {module.Name}");
                        }
                        break;
                    case Core.Models.ItemState.Modified:
                        using (var memoryStream = new MemoryStream())
                        {
                            using TextWriter streamWriter = new StreamWriter(memoryStream);
                            moduleList.Add(new XComment($"{module.ModuleInfo?.OrderNumber?.Value} IO-Link device"));
                            var subList = new List<GSDML.DeviceProfile.ModuleItemT> { module.Item };

                            var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.ModuleItemT[]), ns.NamespaceName);
                            xmlSerializer.Serialize(streamWriter, subList.ToArray());
                            var xelement = XElement.Parse(encoding.GetString(memoryStream.ToArray()));

                            moduleList.Descendants(ns + "ModuleItem").SingleOrDefault(e => e.Attribute("ID")!.Value == module.ID)?.ReplaceWith(xelement.Elements().First());

                            resumeActions += HistoryRelease($"update {module.Name}");
                        }
                        break;
                    case Core.Models.ItemState.Deleted:
                        foreach (var moduleItem in moduleList.Descendants(ns + "ModuleItem").Where(e => e.Attribute("ID")!.Value == module.ID).ToList())
                            moduleItem.Remove();
                        foreach (var moduleItem in moduleList.Descendants(ns + "ModuleItem"))
                        {
                            foreach (var moduleItemRef in moduleItem.Descendants(ns + "ModuleItemRef").Where(e => e.Attribute("SubmoduleItemTarget")!.Value == module.ID).ToList())
                            {
                                moduleItemRef.Remove();
                            }
                        }

                        resumeActions += HistoryRelease($"delete {module.Name}");
                        break;
                }
            }
        }

        if (device.SubmoduleList?.Count > 0 && moduleList is not null && submoduleList is not null)
        {
            foreach (var submodule in device.SubmoduleList)
            {
                switch(submodule.State)
                {
                    case Core.Models.ItemState.Created:
                        using (var memoryStream = new MemoryStream())
                        {
                            using TextWriter streamWriter = new StreamWriter(memoryStream);
                            submoduleList.Add(new XComment($"{submodule.ModuleInfo?.OrderNumber?.Value} IO-Link device"));
                            var subList = new List<GSDML.DeviceProfile.SubmoduleItemT> { submodule.Item };

                            var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.SubmoduleItemT[]), ns.NamespaceName);
                            xmlSerializer.Serialize(streamWriter, subList.ToArray());
                            var xelement = XElement.Parse(encoding.GetString(memoryStream.ToArray()));

                            submoduleList.Add(xelement.Elements().First());

                            foreach (var moduleItem in moduleList.Descendants(ns + "ModuleItem"))
                            {
                                var module = device.ModuleList?.Find(f => f.ID == moduleItem.Attribute("ID")!.Value);
                                var allowedInSubslots = module?.PhysicalSubslots;
                                if (allowedInSubslots?.Contains("..") == true)
                                {
                                    var split = allowedInSubslots.Split("..");
                                    var min = int.Parse(split[0]);
                                    var max = int.Parse(split[1]);
                                    var fixedInSubslots = int.Parse(module?.UseableSubmodules?.Max(m => m.FixedInSubslots) ?? "0");

                                    min = (fixedInSubslots > 0 ? fixedInSubslots + 1 : min);

                                    if (min == max)
                                        allowedInSubslots = $"{min}";
                                    else
                                        allowedInSubslots = $"{min}..{max}";
                                }

                                var useableSubmodules = moduleItem.Descendants(ns + "UseableSubmodules").FirstOrDefault();
                                useableSubmodules!.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
                                useableSubmodules!.Add(new XElement(ns + "SubmoduleItemRef", new XAttribute("SubmoduleItemTarget", submodule.ID!),
                                                                                                new XAttribute("AllowedInSubslots", allowedInSubslots!)));
                            }

                            resumeActions += HistoryRelease($"create {submodule.Name}");
                        }
                        break;
                    case Core.Models.ItemState.Modified:
                        using (var memoryStream = new MemoryStream())
                        {
                            using TextWriter streamWriter = new StreamWriter(memoryStream);

                            var subList = new List<GSDML.DeviceProfile.SubmoduleItemT> { submodule.Item };
                            var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.SubmoduleItemT[]), ns.NamespaceName);
                            xmlSerializer.Serialize(streamWriter, subList.ToArray());
                            var xelement = XElement.Parse(encoding.GetString(memoryStream.ToArray()));

                            submoduleList.Descendants(ns + "SubmoduleItem").SingleOrDefault(e => e.Attribute("ID")!.Value == submodule.ID)?.ReplaceWith(xelement.Elements().First());

                            resumeActions += HistoryRelease($"update {submodule.Name}");
                        }
                        break;
                    case Core.Models.ItemState.Deleted:
                        foreach (var submoduleItem in submoduleList.Descendants(ns + "SubmoduleItem").Where(e => e.Attribute("ID")!.Value == submodule.ID).ToList())
                            submoduleItem.Remove();
                        foreach (var moduleItem in moduleList.Descendants(ns + "ModuleItem"))
                        {
                            foreach (var submoduleItemRef in moduleItem.Descendants(ns + "SubmoduleItemRef").Where(e => e.Attribute("SubmoduleItemTarget")!.Value == submodule.ID).ToList())
                            {
                                submoduleItemRef.Remove();
                            }
                        }

                        resumeActions += HistoryRelease($"delete {submodule.Name}");

                        break;
                }
            }
        }

        if (device.ValueList?.Count > 0 && valueList is not null)
        {
            //valueList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var value in device.ValueList)
            {
                var valueItem = value.Value;
                switch(valueItem.State)
                {
                    case Core.Models.ItemState.Created:
                        using (var memoryStreamValueList = new MemoryStream())
                        {
                            using TextWriter streamWriterValueList = new StreamWriter(memoryStreamValueList);
                            var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.ValueItemT[]), ns.NamespaceName);
                            xmlSerializer.Serialize(streamWriterValueList, new GSDML.DeviceProfile.ValueItemT[] { new() { ID = value.Key, Assignments = valueItem.Assigments.ToArray()/*[.. value.Value]*/ } });

                            var xelement = XElement.Parse(encoding.GetString(memoryStreamValueList.ToArray()));
                            valueList.Add(xelement.Elements());
                        }
                        break;
                    case Core.Models.ItemState.Modified:
                        break;
                    case Core.Models.ItemState.Deleted:
                        break;
                }
            }
        }

        if (device.CategoryList?.Count > 0 && categoryList is not null)
        {
            //categoryList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var category in device.CategoryList)
            {
                var categoryItem = category.Value;
                switch(categoryItem.State)
                {
                    case Core.Models.ItemState.Created:
                        categoryList.Add(new XElement(ns + "CategoryItem", new XAttribute("ID", category.Key), new XAttribute("TextId", categoryItem.Item)));
                        break;
                    case Core.Models.ItemState.Modified:
                        break;
                    case Core.Models.ItemState.Deleted:
                        break;
                }
            }
        }

        if (device.GraphicsList?.Count > 0 && graphicList is not null)
        {
            //graphicList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var graphic in device.GraphicsList)
            {
                graphicsPath ??= [];
                var graphicItem = graphic.Value;
                if(File.Exists(Path.Combine(localFilePath, graphicItem.Item + ".bmp")))
                {
                    graphicsPath.Add(Path.Combine(localFilePath, graphicItem.Item + ".bmp"));
                }
                else
                {
                    var ioddFolder = Path.Combine(localAppData, appConfig.Value.IODDFolder);
                }

                switch(graphicItem.State)
                {
                    case Core.Models.ItemState.Created:
                        graphicList.Add(new XElement(ns + "GraphicItem", new XAttribute("ID", graphic.Key), new XAttribute("GraphicFile", graphicItem.Item)));
                        break;
                    case Core.Models.ItemState.Modified:
                        break;
                    case Core.Models.ItemState.Deleted:
                        break;
                }
            }
        }

        if (device.ExternalTextList?.Count > 0 && ExternalTextList is not null && primaryLanguage is not null)
        {
            //primaryLanguage.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var primaryText in device.ExternalTextList)
            {
                var externalTextItem = primaryText.Value;
                switch(externalTextItem.State)
                {
                    case Core.Models.ItemState.Created:
                        primaryLanguage.Add(new XElement(ns + "Text", new XAttribute("TextId", primaryText.Key), new XAttribute("Value", externalTextItem.Item)));
                        break;
                    case Core.Models.ItemState.Modified:
                        break;
                    case Core.Models.ItemState.Deleted:
                        break;
                }
            }
        }

        if (device.DeviceAccessPoints.Count > 0 && deviceAccessPointList is not null && primaryLanguage is not null) 
        {
            foreach (var dap in device.DeviceAccessPoints)
            {
                var xDeviceAccessPoint = deviceAccessPointList.Descendants(ns + "DeviceAccessPointItem").FirstOrDefault(arg => arg.Attribute("ID")!.Value == dap.Id);
                var infoTextId = xDeviceAccessPoint?.Descendants(ns + "ModuleInfo").FirstOrDefault()?.Descendants(ns + "InfoText").Single().Attribute("TextId")?.Value;

                if(infoTextId is not null && primaryLanguage.Descendants(ns + "Text").Any(e => e.Attribute("TextId")!.Value == infoTextId))
                {
                    var element = primaryLanguage.Descendants(ns + "Text").Where(arg => arg.Attribute("TextId")!.Value == infoTextId).Single();
                    if (element is not null)
                    {
                        element.Attribute("Value")!.Value = dap.Description ?? string.Empty;
                    }
                }
            }
        }

        var infoText = deviceIdentity?.Descendants(ns + "InfoText").Single()?.Attribute("TextId")?.Value;
        if (infoText is not null && primaryLanguage is not null)
        {
            if (primaryLanguage.Descendants(ns + "Text").Any(e => e.Attribute("TextId")!.Value == infoText))
            {
                var element = primaryLanguage.Descendants(ns + "Text").Where(arg => arg.Attribute("TextId")!.Value == infoText).Single();
                if(element is not null)
                {
                    element.Attribute("Value")!.Value = device.Description ?? string.Empty;
                }
            }
        }

        if (xCommentElement is null)
        {
            xDevice.AddFirst(new XComment(@$"VERSION HISTORY :{resumeActions}"));
        }
        else
        {
            if (xCommentElement is XComment comment)
            {
                comment.Value += resumeActions;
            }
        }

        
        SaveAs(Path.Combine(localFilePath, $"{fileName}.xml"));

        return (localFilePath, fileName, graphicsPath);
    }

    public (string?, string?, List<string>?) GetDevicePaths(Models.Device? device, string path)
    {
        if(device is null) return (null, null, null);

        List<string>? graphicsPath = null;
        var localFilePath = Path.GetDirectoryName(device.FilePath);
        var fileName = Path.GetFileNameWithoutExtension(device.FilePath);

        if (!string.IsNullOrEmpty(localFilePath) && device.GraphicsList?.Count > 0)
        {
            foreach (var graphic in device.GraphicsList)
            {
                graphicsPath ??= [];
                var graphicItem = graphic.Value;
                if (File.Exists(Path.Combine(localFilePath, graphicItem.Item + ".bmp")))
                {
                    graphicsPath.Add(Path.Combine(localFilePath, graphicItem.Item + ".bmp"));
                }
                else
                {
                    var ioddFolder = Path.Combine(localAppData, appConfig.Value.IODDFolder);
                }
            }
        }

        return (localFilePath, fileName, graphicsPath);
    }

    private static string HistoryRelease(string action)
    {
        var release =
$@"
{DateTime.Today.ToShortDateString()}           Vx.xx:          - GSDML linker - IO-Link modules {action}";

        return release;
    }
}
