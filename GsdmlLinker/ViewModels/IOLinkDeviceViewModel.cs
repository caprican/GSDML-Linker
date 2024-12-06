using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.ViewModels;
using GsdmlLinker.Core.Models;
using GsdmlLinker.Models;

using MahApps.Metro.Controls.Dialogs;

using Windows.UI;

namespace GsdmlLinker.ViewModels;

public class IOLinkDeviceViewModel(Core.IOL.Contracts.Services.IDevicesService iolDevicesService, IDialogCoordinator dialogCoordinator) : ObservableObject, INavigationAware
{
    private readonly Core.IOL.Contracts.Services.IDevicesService iolDevicesService = iolDevicesService;
    private readonly IDialogCoordinator dialogCoordinator = dialogCoordinator;

    private ObservableCollection<Models.VendorItem>? slaveVendors;
    private Models.DeviceItem? slaveDeviceSelected;

    private ICommand? viewSubParametersCommand;
    private ICommand? processDataViewCommand;

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
    public ICommand ProcessDataViewCommand => processDataViewCommand ??= new RelayCommand(OnProcessDataView);

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
                    CanEdit = true,
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
                    CanEdit = true,
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

    private void OnProcessDataView()
    {
        if (SlaveDeviceSelected is null) return;
        var processData = iolDevicesService.GetProcessData(SlaveDeviceSelected.VendorId, SlaveDeviceSelected.DeviceId);

        List<ProcessDataBase>? processDataIn = null;
        List<ProcessDataBase>? processDataOut = null;

        if (processData?.Where(data => data.Any(a => a.Condition is null)) is IEnumerable<IGrouping<string?, Core.Models.DeviceProcessData>> processDatasIO)
        {
            foreach (var processDataIO in processDatasIO)
            {
                foreach (var item in processDataIO)
                {
                    if (item.ProcessDataIn?.ProcessData is not null)
                    {
                        processDataIn ??= [];

                        //item.ProcessDataIn.ProcessData.Reverse();
                        foreach (var dtIn in item.ProcessDataIn.ProcessData.Where(w => w.DataType != DeviceDatatypes.RecordT).GroupBy(g => g.Index))
                        {
                            for (int i = 0; i < dtIn.Count(); i++)
                            {
                                var processDataItem = dtIn.ToArray()[i];

                                var random = new Random();
                                var color = string.Format("#{0:X6}", random.Next(0x1000000));

                                for (var j = 0; j < processDataItem.BitLength; j++)
                                {
                                    processDataIn.Add(new ProcessDataItem
                                    {
                                        Name = processDataItem.Name,
                                        Datatype = processDataItem.DataType,
                                        //Color = $"#{(128 + i * 8):X2}8{(255 - (int)processDataItem.BitOffset! * 8):X2}8"
                                        //Color = $"#{(128 + (processDataItem.BitOffset ?? 0) * 8):X3}{(255 - (i+1) * 8):X2}8"
                                        Color = color,
                                    });
                                }
                            }
                        }
                        
                        processDataIn.Add(new ProcessDataColumn { Header = "1.7" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.6" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.5" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.4" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.3" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.2" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.1" });
                        processDataIn.Add(new ProcessDataColumn { Header = "1.0" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.7" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.6" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.5" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.4" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.3" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.2" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.1" });
                        processDataIn.Add(new ProcessDataColumn { Header = "0.0" });

                        processDataIn.Reverse();
                    }

                    if (item.ProcessDataOut?.ProcessData is not null)
                    {
                        processDataOut ??= [];

                        item.ProcessDataOut.ProcessData.Reverse();
                        foreach (var dtOut in item.ProcessDataOut.ProcessData.Where(w => w.DataType != DeviceDatatypes.RecordT).GroupBy(g => g.Index))
                        {
                            for (int i = 0; i < dtOut.Count(); i++)
                            {
                                var processDataItem = dtOut.ToArray()[i];
                                for (var j = 0; j < processDataItem.BitLength; j++)
                                {
                                    processDataOut.Add(new ProcessDataItem
                                    {
                                        Name = processDataItem.Name,
                                        Datatype = processDataItem.DataType,
                                        //Color = $"#{(128 + i * 8):X2}8{(255 - (int)processDataItem.BitOffset! * 8):X2}8"
                                        Color = $"#{(128 + (processDataItem.BitOffset ?? 0) * 8):X3}{(255 - (i + 1) * 8):X2}8"
                                    });
                                }
                            }
                        }
                        
                        processDataOut.Add(new ProcessDataColumn { Header = "1.7" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.6" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.5" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.4" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.3" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.2" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.1" });
                        processDataOut.Add(new ProcessDataColumn { Header = "1.0" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.7" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.6" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.5" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.4" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.3" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.2" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.1" });
                        processDataOut.Add(new ProcessDataColumn { Header = "0.0" });

                        processDataOut.Reverse();
                    }
                }
            }
        }

        
        var a = processData?.FirstOrDefault(f => f.Key == null);
        var b = a?.Where(w => w.Condition is null);

        var customDialog = new CustomDialog { Title = Properties.Resources.IOLinkDeviceMappingTitle };

        var dataContext = new Dialogs.IOLinkDeviceMappingViewModel(instance =>
        {
            dialogCoordinator.HideMetroDialogAsync(App.Current.MainWindow.DataContext, customDialog);

        }, processDataIn?.ToArray(), processDataOut?.ToArray());
        customDialog.Content = new Views.Dialogs.IOLinkDeviceMappingDialog { DataContext = dataContext };

        dialogCoordinator.ShowMetroDialogAsync(App.Current.MainWindow.DataContext, customDialog);

    }
}
