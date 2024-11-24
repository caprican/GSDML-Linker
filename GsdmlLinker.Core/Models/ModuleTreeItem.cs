using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GsdmlLinker.Core.Models;

public record ModuleTreeItem : INotifyPropertyChanged
{
    private ItemState state;
    private Module? module;

    public string Name { get; init; } = string.Empty;

    public ushort VendorId { get; init; }
    public uint DeviceId { get; init; }
    public string? ProfinetDeviceId { get; init; }

    public ItemState OriginalState { get; init; }

    public ItemState State
    {
        get => state;
        set
        {
            state = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }
    }

    public ObservableCollection<ModuleTreeItem>? SubmodulesCaterogies { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ModuleTreeItem(string name)
    {
        module = default;
        Name = name;
        state = ItemState.IsCategory;
        OriginalState = ItemState.IsCategory;
    }

    public ModuleTreeItem(Module module)
    {
        this.module = module;

        Name = module.Name;

        VendorId = module.VendorId;
        DeviceId = module.DeviceId;
        ProfinetDeviceId = module.ProfinetDeviceId;

        OriginalState = module.State;
        State = module.State;
    }
}
