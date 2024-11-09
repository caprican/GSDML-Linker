using System.Windows.Controls;

using GsdmlLinker.ViewModels;

namespace GsdmlLinker.Views;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var txtbox = sender as TextBox;
        txtbox?.SelectAll();
    }
}

