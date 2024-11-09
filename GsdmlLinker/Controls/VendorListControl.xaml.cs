using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GsdmlLinker.Controls
{
    public partial class VendorListControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(VendorListControl), null);
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(VendorListControl), new PropertyMetadata(null/*new PropertyChangedCallback(OnItemsSourcePropertyChanged)*/));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set
            {
                SetValue(SelectedItemProperty, value);
                NotifyPropertyChanged(nameof(SelectedItem));
            }
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public VendorListControl()
        {
            InitializeComponent();
            TreeVendor.SetBinding(TreeView.ItemsSourceProperty, new Binding("ItemsSource") { Source = this });
            //TreeVendor.ItemsSource = new Binding { Path = new PropertyPath( ItemsSource )};
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

        //private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = sender as VendorListControl;
        //    if (control != null)
        //        control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        //}

        //private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        //{
        //    // Remove handler for oldValue.CollectionChanged
        //    var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

        //    if (null != oldValueINotifyCollectionChanged)
        //    {
        //        oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
        //    }
        //    // Add handler for newValue.CollectionChanged (if possible)
        //    var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
        //    if (null != newValueINotifyCollectionChanged)
        //    {
        //        newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
        //    }

        //}

        //void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    //Do your stuff here.
        //}
    }
}
