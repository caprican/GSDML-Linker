using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using GsdmlLinker.Core.Models;

namespace GsdmlLinker.Models;

public class ModuleTreeItem : ObservableObject
{
    private ItemState state;
    private readonly Module? module;

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
            SetProperty(ref state, value);
            if(module is not null)
            {
                module.State = State;
            }
        }
    }

    public ObservableCollection<ModuleTreeItem>? SubmodulesCaterogies { get; set; }

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
