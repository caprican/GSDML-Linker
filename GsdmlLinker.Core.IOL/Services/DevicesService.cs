using System.IO.Compression;
using System.Text.RegularExpressions;

using GsdmlLinker.Core.IOL.Contracts.Services;
using GsdmlLinker.Core.IOL.Models;

using Microsoft.Extensions.Options;

namespace GsdmlLinker.Core.IOL.Services;

public class DevicesService(IOptions<Core.Models.AppConfig> appConfig) : IDevicesService
{
    private readonly IOptions<Core.Models.AppConfig> appConfig = appConfig;
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    
    private readonly List<Device> devices = [];

    public List<Device> Devices => devices;

    public event EventHandler<Core.Models.DeviceEventArgs>? DeviceAdded;

    public void InitializeSettings()
    {
        if (!Directory.Exists(Path.Combine(localAppData, appConfig.Value.IODDFolder)))
        {
            Directory.CreateDirectory(Path.Combine(localAppData, appConfig.Value.IODDFolder));
        }

        foreach (var folder in Directory.EnumerateDirectories(Path.Combine(localAppData, appConfig.Value.IODDFolder)))
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(path);
                if (Regexs.FileNameRegex().Match(fileName) is Match IODDmatch && IODDmatch.Success)
                {
                    var device = new Device(path, IODDmatch);
                    if (device is not null && !string.IsNullOrEmpty(device.DeviceId))
                    {
                        devices.Add(device);
                    }
                }
            }
        }
    }

    public void AddDevice(string path)
    {
        foreach (var filePath in Directory.EnumerateFiles(path, "*.xml"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (Regexs.FileNameRegex().Match(fileName) is Match IODDmatch && IODDmatch.Success)
            {
                //<vendor>-<code_produit>-<date>-IODD<version>(-<langue>)?
                var schematicVersion = IODDmatch.Groups[4].Value;
                var name = IODDmatch.Groups[2].Value;
                var manufacturerName = IODDmatch.Groups[1].Value;

                var localFilePath = Path.Combine(localAppData, appConfig.Value.IODDFolder, manufacturerName, $"{name}-IODD{schematicVersion}");
                if (!Directory.Exists(localFilePath))
                {
                    Directory.CreateDirectory(localFilePath);
                }

                File.Copy(filePath, Path.Combine(localFilePath, Path.GetFileName(filePath)), true);

                var device = new Device(Path.Combine(localFilePath, Path.GetFileName(filePath)), IODDmatch);

                if(device.GraphicsList is not null)
                {
                    var folderPath = Path.GetDirectoryName(filePath);
                    foreach (var graphic in device.GraphicsList)
                    {
                        File.Copy(Path.Combine(folderPath!, graphic.Value.Item), Path.Combine(localFilePath, graphic.Value.Item), true);
                    }
                }

                devices.Add(device);

                DeviceAdded?.Invoke(this, new Core.Models.DeviceEventArgs { Device = device });
            }
        }
    }

    public void AddDevice(ZipArchive archive)
    {
        foreach (var file in archive.Entries.Where(w => w.Name.EndsWith(".xml")))
        {
            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (Regexs.FileNameRegex().Match(fileName) is Match IODDmatch && IODDmatch.Success)
            {
                //<vendor>-<code_produit>-<date>-IODD<version>(-<langue>)?
                var schematicVersion = IODDmatch.Groups[4].Value;
                var name = IODDmatch.Groups[2].Value;
                var manufacturerName = IODDmatch.Groups[1].Value;

                var localFilePath = Path.Combine(localAppData, appConfig.Value.IODDFolder, manufacturerName, $"{name}-IODD{schematicVersion}");
                if (!Directory.Exists(localFilePath))
                {
                    Directory.CreateDirectory(localFilePath);
                }

                file.ExtractToFile(Path.Combine(localFilePath, Path.GetFileName(file.Name)), true);

                var device = new Device(Path.Combine(localFilePath, Path.GetFileName(file.Name)), IODDmatch);

                if (device.GraphicsList is not null)
                {
                    foreach (var graphic in device.GraphicsList)
                    {
                        var zipGraphic = archive.Entries.SingleOrDefault(s => s.Name.EndsWith($"{graphic.Value}.bmp"));
                        zipGraphic?.ExtractToFile(Path.Combine(localFilePath, graphic.Value.Item), true);
                    }
                }

                devices.Add(device);

                DeviceAdded?.Invoke(this, new Core.Models.DeviceEventArgs { Device = device });
            }
        }
    }

    public IEnumerable<Core.Models.DeviceParameter>? GetParameters(string vendorId, string deviceId)
    {
        var device = devices.FirstOrDefault(f => f.VendorId == vendorId && f.DeviceId == deviceId);
        if (device is null) return null;
        
        return device.Parameters;
    }

    public IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>>? GetProcessData(string vendorId, string deviceId)
    {
        var device = devices.FirstOrDefault(f => f.VendorId == vendorId && f.DeviceId == deviceId);
        if (device is null) return null;

        return device.ProcessDatas;
    }
}
