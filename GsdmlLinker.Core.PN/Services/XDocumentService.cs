using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Linq;

using GsdmlLinker.Core.PN.Contracts.Services;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

using GSDML = ISO15745.GSDML;

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

        var deviceAccessPointList = (from d in xDevice.Descendants() where d.Name.LocalName == "DeviceAccessPointList" select d).First();
        var moduleList = (from d in xDevice.Descendants() where d.Name.LocalName == "ModuleList" select d).First();
        var submoduleList = (from d in xDevice.Descendants() where d.Name.LocalName == "SubmoduleList" select d).First();
        var valueList = (from d in xDevice.Descendants() where d.Name.LocalName == "ValueList" select d).First();

        var categoryList = (from d in xDevice.Descendants() where d.Name.LocalName == "CategoryList" select d).First();
        var graphicList = (from d in xDevice.Descendants() where d.Name.LocalName == "GraphicsList" select d).First();
        
        var ExternalTextList = (from d in xDevice.Descendants() where d.Name.LocalName == "ExternalTextList" select d).First();
        var primaryLanguage = (from d in xDevice.Descendants() where d.Name.LocalName == "PrimaryLanguage" select d).First();
        
        var xCommentElement = (from d in xDevice.Nodes() where d is XComment select d).FirstOrDefault();
        var deviceIdentity = (from d in xDevice.Descendants() where d.Name.LocalName == "DeviceIdentity" select d).First();

        if (device.ModuleList is not null)
        {
            if(device.ModuleList.Count > moduleList.Elements().Count())
            {
                moduleList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
                foreach (var module in device.ModuleList)
                {
                    if(!moduleList.Descendants(ns + "ModuleItem").Any(e => e.Attribute("ID")!.Value == module.ID))
                    {
                        using var memoryStream = new MemoryStream();
                        using TextWriter streamWriter = new StreamWriter(memoryStream);
                        moduleList.Add(new XComment($"{module.ModuleInfo?.OrderNumber?.Value} IO-Link device"));
                        var subList = new List<GSDML.DeviceProfile.ModuleItemT> { module };

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
                    }
                }
            }
            else if(device.ModuleList.Count < moduleList.Elements().Count())
            {
                moduleList.Add(new XComment($"GSDML linker - removed {DateTime.Today.ToShortDateString()}"));

            }
        }

        if (device.SubmoduleList is not null)
        {
            //if(device.SubmoduleList.Count > submoduleList.Elements().Count())
            //{
            //    submoduleList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));

            var submoduleId = submoduleList.Descendants(ns + "SubmoduleItem").Select(e => e.Attribute("ID")!.Value);

            foreach (var submodule in device.SubmoduleList)
            {
                if (!submoduleList.Descendants(ns + "SubmoduleItem").Any(e => e.Attribute("ID")!.Value == submodule.ID))
                {
                    using var memoryStream = new MemoryStream();
                    using TextWriter streamWriter = new StreamWriter(memoryStream);
                    submoduleList.Add(new XComment($"{submodule.ModuleInfo?.OrderNumber?.Value} IO-Link device"));
                    var subList = new List<GSDML.DeviceProfile.SubmoduleItemT> { submodule };

                    var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.SubmoduleItemT[]), ns.NamespaceName);
                    xmlSerializer.Serialize(streamWriter, subList.ToArray());

                    var xelement = XElement.Parse(encoding.GetString(memoryStream.ToArray()));

                    submoduleList.Add(xelement.Elements().First());

                    foreach (var moduleItem in moduleList.Descendants(ns + "ModuleItem"))
                    {
                        var module = device.ModuleList?.Find(f => f.ID == moduleItem.Attribute("ID")!.Value);
                        var allowedInSubslots = module?.PhysicalSubslots;
                        if(allowedInSubslots?.Contains("..") == true)
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
                }
            }
            //}
            //else if (device.SubmoduleList.Count < submoduleList.Elements().Count())
            //{
            //    submoduleList.Add(new XComment($"GSDML linker - removed {DateTime.Today.ToShortDateString()}"));

            //}
        }

        if (device.ValueList?.Count > 0)
        {
            valueList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var value in device.ValueList)
            {
                if(!valueList.Descendants(ns + "ValueItem").Any(e => e.Attribute("ID")!.Value == value.Key))
                {
                    using var memoryStreamValueList = new MemoryStream();
                    using TextWriter streamWriterValueList = new StreamWriter(memoryStreamValueList);
                    var xmlSerializer = new XmlSerializer(typeof(GSDML.DeviceProfile.ValueItemT[]), ns.NamespaceName);
                    xmlSerializer.Serialize(streamWriterValueList, new GSDML.DeviceProfile.ValueItemT[] { new GSDML.DeviceProfile.ValueItemT  { ID = value.Key, Assignments = [.. value.Value] } });

                    var xelement = XElement.Parse(encoding.GetString(memoryStreamValueList.ToArray()));
                    valueList.Add(xelement.Elements());
                }
            }
        }

        if (device.CategoryList?.Count > 0)
        {
            categoryList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var category in device.CategoryList)
            {
                if (!categoryList.Descendants(ns + "CategoryItem").Any(e => e.Attribute("ID")!.Value == category.Key))
                {
                    categoryList.Add(new XElement(ns + "CategoryItem", new XAttribute("ID", category.Key), new XAttribute("TextId", category.Value)));
                }
            }
        }

        if (device.GraphicsList?.Count > 0)
        {
            graphicList.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
            foreach (var graphic in device.GraphicsList)
            {
                graphicsPath ??= [];
                if(File.Exists(Path.Combine(localFilePath, graphic.Value + ".bmp")))
                {
                    graphicsPath.Add(Path.Combine(localFilePath, graphic.Value + ".bmp"));
                }
                else
                {
                    var ioddFolder = Path.Combine(localAppData, appConfig.Value.IODDFolder);

                }

                if (!graphicList.Descendants(ns + "GraphicItem").Any(e => e.Attribute("ID")!.Value ==  graphic.Key))
                {
                    graphicList.Add(new XElement(ns + "GraphicItem", new XAttribute("ID", graphic.Key), new XAttribute("GraphicFile", graphic.Value)));
                }
            }
        }

        if (device.ExternalTextList?.Count > 0)
        {
            if(device.ExternalTextList.Count > primaryLanguage.Elements().Count())
            {
                primaryLanguage.Add(new XComment($"GSDML linker - Add {DateTime.Today.ToShortDateString()}"));
                foreach (var primaryText in device.ExternalTextList)
                {
                    if (!primaryLanguage.Descendants(ns + "Text").Any(e => e.Attribute("TextId")!.Value == primaryText.Key))
                    {
                        primaryLanguage.Add(new XElement(ns + "Text", new XAttribute("TextId", primaryText.Key), new XAttribute("Value", primaryText.Value)));
                    }
                }
            }
            else if (device.ExternalTextList.Count < primaryLanguage.Elements().Count())
            {
                primaryLanguage.Add(new XComment($"GSDML linker - remove {DateTime.Today.ToShortDateString()}"));

            }
        }

        if (device.DeviceAccessPoints.Count > 0) 
        {
            foreach (var dap in device.DeviceAccessPoints)
            {
                var xDeviceAccessPoint = deviceAccessPointList.Descendants(ns + "DeviceAccessPointItem").FirstOrDefault(arg => arg.Attribute("ID")!.Value == dap.Id);
                var infoTextId = xDeviceAccessPoint?.Descendants(ns + "ModuleInfo").FirstOrDefault()?.Descendants(ns + "InfoText").Single().Attribute("TextId")?.Value;

                if(infoTextId is not null)
                {
                    if (primaryLanguage.Descendants(ns + "Text").Any(e => e.Attribute("TextId")!.Value == infoTextId))
                    {
                        var element = primaryLanguage.Descendants(ns + "Text").Where(arg => arg.Attribute("TextId")!.Value == infoTextId).Single();
                        if (element is not null)
                        {
                            element.Attribute("Value")!.Value = dap.Description ?? string.Empty;
                        }
                    }
                }
            }
        }

        var infoText = deviceIdentity.Descendants(ns + "InfoText").Single()?.Attribute("TextId")?.Value;
        if (infoText is not null)
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
            var comment = @$"VERSION HISTORY :
GSDML linker - Add {DateTime.Today.ToShortDateString()}";

            xDevice.AddFirst(new XComment(comment));
        }
        else
        {
            if (xCommentElement is XComment comment)
            {
                comment.Value += @$"
GSDML linker - Add {DateTime.Today.ToShortDateString()}";
            }
        }

        
        SaveAs(Path.Combine(localFilePath, $"{fileName}.xml"));

        return (localFilePath, fileName, graphicsPath);
    }

    private string HisotryRelease()
    {
        var release = 
$@"{DateTime.Today.ToShortDateString()}           Vx.xx:          - GSDML linker - IO-Link modules added :
                                                                        ";
        return release;
    }
}
