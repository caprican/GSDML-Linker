using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.ViewModels;

namespace GsdmlLinker.ViewModels;

public class IOLinkDeviceViewModel(Core.IOL.Contracts.Services.IDevicesService iolDevicesService) : ObservableObject, INavigationAware
{
    private readonly Core.IOL.Contracts.Services.IDevicesService iolDevicesService = iolDevicesService;

    private ObservableCollection<Models.VendorItem>? slaveVendors;
    private Models.DeviceItem? slaveDeviceSelected;

    private ICommand? viewSubParametersCommand;

    public ObservableCollection<Models.VendorItem>? SlaveVendors
    {
        get => slaveVendors;
        set => SetProperty(ref slaveVendors, value);
    }

    public ObservableCollection<Core.Models.DeviceParameter> SlaveParameters { get; set; } = [];

    public Models.DeviceItem? SlaveDeviceSelected
    {
        get => slaveDeviceSelected;
        set
        {
            SetProperty(ref slaveDeviceSelected, value);
            if (SlaveDeviceSelected is not null)
            {
                //DeviceRename = SlaveDeviceSelected.Name;
                GetSlaveParameters();
                //SaveMasterDeviceVisibility = Visibility.Collapsed;
                //UpdateSlaveDeviceVisibility = Visibility.Visible;
                //SlaveListVisibility = Visibility.Collapsed;
            }
            else
            {
                //DeviceRename = string.Empty;
                //UpdateSlaveDeviceVisibility = Visibility.Collapsed;
                //SlaveListVisibility = Visibility.Visible;
                //SaveMasterDeviceVisibility = Visibility.Visible;
            }
        }
    }

    public ICommand ViewSubParametersCommand => viewSubParametersCommand ??= new RelayCommand<Core.Models.DeviceParameter>(OnViewSubParameters);

    public void OnNavigatedTo(object parameter)
    {
        iolDevicesService.DeviceAdded += IolDevicesService_DeviceAdded;
        GetSlaveDevices();

    }

    public void OnNavigatedFrom()
    {
        iolDevicesService.DeviceAdded -= IolDevicesService_DeviceAdded;
    }

    private void IolDevicesService_DeviceAdded(object? sender, Core.Models.DeviceEventArgs e)
    {
    }

    private void GetSlaveDevices()
    {
        SlaveVendors = [];
        foreach (var device in iolDevicesService.Devices)
        {
            if (SlaveVendors.Any(a => a.Id == device.VendorId))
            {
                foreach (var item in AddDevice(device))
                {
                    SlaveVendors.First(a => a.Id == device?.VendorId).Devices!.Add(item);
                }
            }
            else
            {
                var vendor = new Models.VendorItem
                {
                    Name = device.ManufacturerName ?? string.Empty,
                    Id = device.VendorId ?? string.Empty,
                    Icon = device.VendorLogo ?? string.Empty,
                };

                foreach (var item in AddDevice(device))
                {
                    vendor.Devices ??= [];
                    vendor.Devices.Add(item);
                }

                SlaveVendors.Add(vendor);
            }
        }
    }

    private static List<Models.DeviceItem> AddDevice(Core.IOL.Models.Device device)
    {
        var result = new List<Models.DeviceItem>();

        if (device.Variants is not null)
        {
            foreach (var variant in device.Variants)
            {
                result.Add(new Models.DeviceItem(device)
                {
                    Name = variant.Name ?? string.Empty,
                    DeviceId = device.DeviceId,
                    VendorId = device.VendorId,
                    Description = variant.Description ?? string.Empty,
                    Icon = variant?.Icon ?? string.Empty,
                    ProfileVersion = device.SchematicVersion ?? string.Empty,
                    //Version = variant?.Version
                });
            }
        }
        return result;
    }

    private void GetSlaveParameters()
    {
        SlaveParameters.Clear();
        if (SlaveDeviceSelected is not null)
        {
            var parameters = iolDevicesService.GetParameters(SlaveDeviceSelected.VendorId, SlaveDeviceSelected.DeviceId);
            if (parameters is not null)
            {
                foreach (var item in parameters)
                {
                    SlaveParameters.Add(item);
                }
            }
                CollectionViewSource.GetDefaultView(SlaveParameters).Refresh();

        }
    }

    private void OnViewSubParameters(Core.Models.DeviceParameter? parameter)
    {
        if (parameter is null) return;

        foreach (var item in SlaveParameters.Where(param => param.Index == parameter.Index && param.Subindex is not null))
        {
            item.IsVisible = !item.IsVisible;
        }
    }
}
