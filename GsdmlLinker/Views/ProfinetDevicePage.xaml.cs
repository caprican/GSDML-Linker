using System.Windows.Controls;

using GsdmlLinker.ViewModels;

namespace GsdmlLinker.Views;

public partial class ProfinetDevicePage : Page
{
    public ProfinetDevicePage(ProfinetDeviceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
