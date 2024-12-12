using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GsdmlLinker.Models;

public record DeviceItem : INotifyPropertyChanged
{
    public readonly Core.Models.Device? Device;

    private bool deviceIdChangeable;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; set; } = string.Empty;
    public string VendorId {  get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;

    public bool DeviceIdChangeable 
    {
        get => deviceIdChangeable; 
        set
        {
            deviceIdChangeable = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceIdChangeable)));
        }
    }

    public string DNS {  get; init; } = string.Empty;
    public string DeviceAccessId {  get; init; } = string.Empty;

    public string VendorName { get; init; } = string.Empty;
    public string DeviceFamily { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;


    public DateTime? Version { get;init; }
    public string ProfileVersion { get; init; } = string.Empty;

    public bool Editing { get; set; } = false;

    public bool CanEdit { get; init; } = false;

    public ObservableCollection<DeviceItem>? Releases { get; set; }

    public DeviceItem(Core.Models.Device? device)
    {
        Device = device;
    }
}
