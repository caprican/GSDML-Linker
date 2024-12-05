using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Models;

namespace GsdmlLinker.ViewModels.Dialogs;

public class IOLinkDeviceMappingViewModel(Action<IOLinkDeviceMappingViewModel> closeHandler,
                                          ProcessDataBase[]? processDataIn, ProcessDataBase[]? processDataOut) : ObservableObject
{
    private ICommand? closeCommand;

    public ICommand CloseCommand => closeCommand ??= new RelayCommand(OnClose);

    public ProcessDataBase[]? ProcessDataIn => processDataIn;
    public ProcessDataBase[]? ProcessDataOut => processDataOut;

    private void OnClose()
    {
        closeHandler(this);
    }
}
