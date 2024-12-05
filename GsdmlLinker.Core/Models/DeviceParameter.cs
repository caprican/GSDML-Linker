using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GsdmlLinker.Core.Models;

public record DeviceParameter : INotifyPropertyChanged
{
    private bool isVisible = true;
    private bool isSelected = true;
    private bool isLocked = false;
    private string defaultValue = string.Empty;

    public string Name { get; init; } = string.Empty;
    
    public ushort Index { get; init; }
    public ushort? Subindex { get; init; } = null;
    public string IndexValue => Subindex is null ? $"{Index}" : $"{Index}.{Subindex}";

    public string Description { get; init; } = string.Empty;

    public bool IsReadOnly { get; init; } = false;

    public bool IsSelected
    { 
        get => isSelected;
        set
        {
            isSelected = value;
            OnPropertyChanged();
        }
    }

    public bool IsLocked
    {
        get => isLocked;
        set
        {
            isLocked = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible 
    { 
        get => isVisible;
        set 
        {
            isVisible = value;
            OnPropertyChanged();
        }
    }

    public string DefaultValue
    { 
        get => defaultValue;
        set
        {
            defaultValue = value;
            OnPropertyChanged();
        }
    }

    public byte FixedLength { get; init; }

    public DeviceDatatypes? DataType { get; init; }
    public ushort BitLength { get; init; }
    public ushort? BitOffset { get; init; }

    public Dictionary<string, string>? Values { get; init; }

    public object Minimum { get; init; } = 0;
    public object Maximum { get; init; } = 0;

    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public event PropertyChangedEventHandler? PropertyChanged;


}
