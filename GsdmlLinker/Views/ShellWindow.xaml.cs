using System.Windows.Controls;

using GsdmlLinker.Contracts.Views;
using GsdmlLinker.ViewModels;

using MahApps.Metro.Controls;

namespace GsdmlLinker.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame() => shellFrame;

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();
}
