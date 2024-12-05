using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Core.Models.IoddFinder;

namespace GsdmlLinker.ViewModels.Dialogs;

public class IoddfinderSelectDeviceViewModel(Action<IoddfinderSelectDeviceViewModel> closeHandler, List<Iodd> devices) : ObservableObject
{
    private ICommand? closeCommand;
    private Iodd? selectedItem;
    public List<Iodd> Devices => devices;

    public Iodd? SelectedItem
    {
        get => selectedItem;
        set
        {
            SetProperty(ref selectedItem, value);
            OnClose();
        }
    }

    public ICommand CloseCommand => closeCommand ??= new RelayCommand(OnClose);

    private void OnClose()
    {
        closeHandler(this);
    }
}
