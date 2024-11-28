using System.Windows.Controls;

using GsdmlLinker.ViewModels;

namespace GsdmlLinker.Views;

public partial class IOLinkDevicePage : Page
{
    public IOLinkDevicePage(IOLinkDeviceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
