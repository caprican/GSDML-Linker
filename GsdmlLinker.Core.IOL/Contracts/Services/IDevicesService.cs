
using System.IO.Compression;

namespace GsdmlLinker.Core.IOL.Contracts.Services;

public interface IDevicesService
{
    public List<Models.Device> Devices { get; }
    public event EventHandler<Core.Models.DeviceEventArgs>? DeviceAdded;

    public void InitializeSettings();

    public void AddDevice(string path);
    public void AddDevice(ZipArchive archive);

    public IEnumerable<Core.Models.DeviceParameter>? GetParameters(string vendorId, string deviceId);
}
