
namespace GsdmlLinker.Core.PN.Contracts.Services;

public interface IDevicesService
{
    public List<Models.Device> Devices { get; }
    public event EventHandler<Core.Models.DeviceEventArgs>? DeviceAdded;

    public void InitializeSettings();

    public void AddDevice(string path);
    public void AddDevice(string localFilePath, string fileName, List<string> graphicsPath);

    public IEnumerable<Core.Models.Module>? GetModules(string vendorId, string deviceId, string deviceAccessId, DateTime? version);

    public IEnumerable<Core.Models.DeviceParameter> GetRecordParameters(string vendorId, string deviceId, DateTime? version, string profinetDeviceId);
    public Core.Models.DevicePortParameter? GetPortParameters(string vendorId, string deviceId, DateTime? version, string profinetDeviceId);
}
