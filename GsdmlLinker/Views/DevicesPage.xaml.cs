using System.Windows.Controls;

using GsdmlLinker.ViewModels;

namespace GsdmlLinker.Views;

public partial class DevicesPage : Page
{
    public DevicesPage(DevicesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
