using System.Text.RegularExpressions;

using GsdmlLinker.Core.PN.Contracts.Factories;
using GsdmlLinker.Core.PN.Contracts.Services;
using GsdmlLinker.Core.PN.Models;

using Microsoft.Extensions.Options;

namespace GsdmlLinker.Core.PN.Services;

public class DevicesService(IOptions<Core.Models.AppConfig> appConfig, IDevicesFactory devicesFactory) : IDevicesService
{
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly IOptions<Core.Models.AppConfig> appConfig = appConfig;
    private readonly IDevicesFactory devicesFactory = devicesFactory;

    private readonly List<Device> devices = [];

    public List<Device> Devices => devices;

    public event EventHandler<Core.Models.DeviceEventArgs>? DeviceAdded;

    public void InitializeSettings()
    {
        if (!Directory.Exists(Path.Combine(localAppData, appConfig.Value.GSDMLFolder)))
        {
            Directory.CreateDirectory(Path.Combine(localAppData, appConfig.Value.GSDMLFolder));
        }

        foreach (var folder in Directory.EnumerateDirectories(Path.Combine(localAppData, appConfig.Value.GSDMLFolder)))
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(path);
                if (Regexs.FileNameRegex().Match(fileName) is Match GSDmatch && GSDmatch.Success)
                {
                    var device = devicesFactory.CreateDevice(path, GSDmatch);
                    devices.Add(device);
                }
            }
        }
    }

    public void AddDevice(string path)
    {
        //foreach (var filePath in Directory.EnumerateFiles(path, "*.xml"))
        //{
        var fileName = Path.GetFileNameWithoutExtension(path);
        //GSDML-V<Version>-<NomFabricant>-<NomProduit>-<Date>(-<heure>)?(-<langue>)?(-<Commentaire>)?
        if (Regexs.FileNameRegex().Match(fileName) is Match GSDmatch && GSDmatch.Success)
        {
            var schematicVersion = GSDmatch.Groups[1].Value;
            var manufacturerName = GSDmatch.Groups[3].Value;
            var deviceFamily = GSDmatch.Groups[4].Value;

            var localFilePath = Path.Combine(localAppData, appConfig.Value.GSDMLFolder, manufacturerName, $"{deviceFamily}-V{schematicVersion}");
            if (!Directory.Exists(localFilePath))
            {
                Directory.CreateDirectory(localFilePath);
            }

            File.Copy(path, Path.Combine(localFilePath, Path.GetFileName(path)), true);

            var device = devicesFactory.CreateDevice(Path.Combine(localFilePath, Path.GetFileName(path)), GSDmatch);

            if (device.GraphicsList is not null)
            {
                var folderPath = Path.GetDirectoryName(path);
                foreach (var graphic in device.GraphicsList)
                {
                    File.Copy(Path.Combine(folderPath!, graphic.Value.Item + ".bmp"), Path.Combine(localFilePath, graphic.Value.Item + ".bmp"), true);
                }
            }

            devices.Add(device);

            DeviceAdded?.Invoke(this, new Core.Models.DeviceEventArgs { Device = device });
        }
        //}
    }

    public IEnumerable<Core.Models.Module>? GetModules(string vendorId, string deviceId, string deviceAccessId, DateTime? version)
    {
        var device = devices.FirstOrDefault(f => f.VendorId == vendorId && f.DeviceId == deviceId && f.Version == version);
        if (device is not null)
        {
            return device.DeviceAccessPoints.FirstOrDefault(f => f.Id == deviceAccessId)?.Modules;
        }
        else
        {
            return null;
        }
    }

    public IEnumerable<Core.Models.DeviceParameter> GetRecordParameters(string vendorId, string deviceId, DateTime? version, string profinetDeviceId)
    {
        var device = devices.SingleOrDefault(f => f.VendorId == vendorId && f.DeviceId == deviceId && f.Version == version);
        var parameters = new List<Core.Models.DeviceParameter>();

        if (device is not null)
        {
            var factory = devicesFactory.CreateModule(device);
            if (factory is null) return parameters;

            parameters.AddRange(factory.ReadRecordParameter(profinetDeviceId));
        }

        return parameters;
    }
}
