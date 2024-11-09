using System.ComponentModel;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.Input;

namespace GsdmlLinker.Controls
{
    public partial class IolDeviceDatagrid : UserControl, INotifyPropertyChanged
    {
        private double? checkColumnSize = double.NaN;
        private double? indexColumnSize = double.NaN;
        private double? nameColumnSize = null;
        private double? parameterColumnSize = null;

        public double? CheckColumnSize 
        { 
            get => checkColumnSize; 
            set
            {
                checkColumnSize = value;
                NotifyPropertyChanged(nameof(CheckColumnSize));
            }
        }
        public double? IndexColumnSize 
        {
            get => indexColumnSize;
            set
            {
                indexColumnSize = value;
                NotifyPropertyChanged(nameof(IndexColumnSize));
            }
        }

        public double? NameColumnSize
        {
            get => nameColumnSize;
            set
            {
                nameColumnSize = value;
                NotifyPropertyChanged(nameof(NameColumnSize));
            }
        }

        public double? ParameterColumnSize 
        {
            get => parameterColumnSize;
            set
            {
                parameterColumnSize = value;
                NotifyPropertyChanged(nameof(ParameterColumnSize));
            }
        }

        public IolDeviceDatagrid()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged(string aPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));
        }
    }
}
