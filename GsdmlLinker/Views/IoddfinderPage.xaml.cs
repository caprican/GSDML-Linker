using System.Windows.Controls;

using GsdmlLinker.ViewModels;

namespace GsdmlLinker.Views;

public partial class IoddfinderPage : Page
{
    public IoddfinderPage(IoddfinderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
