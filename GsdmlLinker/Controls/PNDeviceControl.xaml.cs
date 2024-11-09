using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GsdmlLinker.Controls;

public partial class PNDeviceControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(PNDeviceControl), null);
    public static readonly DependencyProperty SelectedSubitemProperty = DependencyProperty.Register("SelectedSubitem", typeof(object), typeof(PNDeviceControl), null);

    public object SelectedItem
    {
        get { return GetValue(SelectedItemProperty); }
        set
        {
            SetValue(SelectedItemProperty, value);
            NotifyPropertyChanged(nameof(SelectedItem));
        }
    }

    public object SelectedSubitem
    {
        get { return GetValue(SelectedSubitemProperty); }
        set
        {
            SetValue(SelectedSubitemProperty, value);
            NotifyPropertyChanged(nameof(SelectedSubitem));
        }
    }

    public PNDeviceControl()
    {
        InitializeComponent();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = e.NewValue;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged(string aPropertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));
    }

    private void TreeView_SelectedItemChanged_1(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedSubitem = e.NewValue;
    }
}
