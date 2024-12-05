using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.ViewModels;
using GsdmlLinker.Core.Contracts.Services;

using Microsoft.Win32;

namespace GsdmlLinker.ViewModels;

public class ProfinetDeviceViewModel(Contracts.Services.ISettingsService settingsService, Core.PN.Contracts.Services.IDevicesService pnDevicesService,
                                     Core.PN.Contracts.Services.IXDocumentService xDocumentService, IZipperService zipperService) : ObservableObject, INavigationAware
{
    private readonly Contracts.Services.ISettingsService settingsService = settingsService;
    private readonly Core.PN.Contracts.Services.IDevicesService pnDevicesService = pnDevicesService;
    private readonly Core.PN.Contracts.Services.IXDocumentService xDocumentService = xDocumentService;
    private readonly IZipperService zipperService = zipperService;

    private ObservableCollection<Models.VendorItem>? masterVendors;
    private Models.DeviceItem? masterDeviceSelected;
    private Visibility saveMasterDeviceVisibility = Visibility.Collapsed;
    private ObservableCollection<Models.ModuleTreeItem>? masterModules;
    private ICommand? exportMasterDeviceCommand;

    public ObservableCollection<Models.VendorItem>? MasterVendors
    {
        get => masterVendors;
        set => SetProperty(ref masterVendors, value);
    }

    public Models.DeviceItem? MasterDeviceSelected
    {
        get => masterDeviceSelected;
        set
        {
            SetProperty(ref masterDeviceSelected, value);
            if (MasterDeviceSelected is not null)
            {
                GetMasterModule();
                SaveMasterDeviceVisibility = Visibility.Visible;
            }
            else
            {
                SaveMasterDeviceVisibility = Visibility.Collapsed;
            }
        }
    }

    public ObservableCollection<Models.ModuleTreeItem>? MasterModules
    {
        get => masterModules;
        set => SetProperty(ref masterModules, value);
    }

    public Visibility SaveMasterDeviceVisibility
    {
        get => saveMasterDeviceVisibility;
        set => SetProperty(ref saveMasterDeviceVisibility, value);
    }

    public ICommand ExportMasterDeviceCommand => exportMasterDeviceCommand ??= new RelayCommand<Models.DeviceItem>(OnExportMasterDevice);

    public void OnNavigatedTo(object parameter)
    {
        pnDevicesService.DeviceAdded += PnDevicesService_DeviceAdded;
        GetMasterDevices();
    }

    private void PnDevicesService_DeviceAdded(object? sender, Core.Models.DeviceEventArgs e)
    {
    }

    public void OnNavigatedFrom()
    {
        pnDevicesService.DeviceAdded -= PnDevicesService_DeviceAdded;
    }

    private void GetMasterDevices()
    {
        MasterVendors = [];

        foreach (var group in pnDevicesService.Devices.GroupBy(g => g.DeviceId))
        {
            var device = group.First();
            if (!MasterVendors.Any(a => a.Id == device.VendorId))
            {
                var vendor = new Models.VendorItem
                {
                    Name = device.ManufacturerName ?? string.Empty,
                    Id = device.VendorId ?? string.Empty,
                    CanEdit = true,
                };

                foreach (var item in group)
                {
                    foreach (var dap in AddDevice(item))
                    {
                        vendor.Devices ??= [];

                        if (vendor.Devices.Any(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS))
                        {
                            vendor.Devices.First(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS).Releases ??= [];
                            dap.ShortDescription = dap.Description.Replace(vendor.Devices.First(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS).Description, string.Empty).Trim();
                            vendor.Devices.First(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS).Releases!.Add(dap);
                        }
                        else
                        {
                            vendor.Devices.Add(dap);
                        }
                    }
                }

                MasterVendors.Add(vendor);
            }
            else
            {
                foreach (var item in group)
                {
                    foreach (var dap in AddDevice(item))
                    {
                        if (MasterVendors.First(a => a.Id == device?.VendorId).Devices!.Any(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS))
                        {
                            MasterVendors.First(a => a.Id == device?.VendorId).Devices!.First(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS).Releases ??= [];
                            dap.ShortDescription = dap.Description.Replace(MasterVendors.First(a => a.Id == device?.VendorId).Devices!.First(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS).Description, string.Empty).Trim();

                            MasterVendors.First(a => a.Id == device?.VendorId).Devices!.First(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS).Releases!.Add(dap);
                        }
                        else
                        {
                            MasterVendors.First(a => a.Id == device?.VendorId).Devices!.Add(dap);
                        }
                    }
                }
            }
        }
    }

    private static List<Models.DeviceItem> AddDevice(Core.PN.Models.Device device)
    {
        var result = new List<Models.DeviceItem>();

        foreach (var deviceAccessPoint in device.DeviceAccessPoints)
        {
            result.Add(new Models.DeviceItem(device)
            {
                Name = deviceAccessPoint.Name ?? string.Empty,
                DeviceId = device.DeviceId,
                VendorId = device.VendorId,
                DNS = deviceAccessPoint.DNS ?? string.Empty,
                DeviceAccessId = deviceAccessPoint.Id,
                Description = deviceAccessPoint.Description ?? string.Empty,
                Icon = deviceAccessPoint?.Icon ?? string.Empty,
                Version = deviceAccessPoint?.Version,
                VendorName = device.VendorName ?? string.Empty,
                DeviceFamily = device.DeviceFamily ?? string.Empty,
                CanEdit = true,
            });
        }
        return result;
    }

    private void GetMasterModule()
    {
        MasterModules = [];
        if (MasterDeviceSelected is not null)
        {
            var modules = pnDevicesService.GetModules(MasterDeviceSelected.VendorId, MasterDeviceSelected.DeviceId, MasterDeviceSelected.DeviceAccessId, MasterDeviceSelected.Version);
            if (modules is not null)
            {
                foreach (var module in modules)
                {
                    var moduleTree = new Models.ModuleTreeItem(module);

                    if (module.Submodules is null || module.Submodules.Count == 0)
                    {
                        if (string.IsNullOrEmpty(module.CategoryRef))
                        {
                            MasterModules.Add(moduleTree);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(module.SubCategoryRef))
                            {
                                if (!MasterModules.Any(a => a.Name == module.CategoryRef))
                                {
                                    MasterModules.Add(new Models.ModuleTreeItem(module.CategoryRef)
                                    {
                                        SubmodulesCaterogies = [new Models.ModuleTreeItem(module)]
                                    });
                                }
                                else
                                {
                                    MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies ??= [];
                                    MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies?.Add(new Models.ModuleTreeItem(module));
                                }
                            }
                            else
                            {
                                if (!MasterModules.Any(a => a.Name == module.CategoryRef))
                                {
                                    MasterModules.Add(new Models.ModuleTreeItem(module.CategoryRef)
                                    {
                                        SubmodulesCaterogies = [new Models.ModuleTreeItem(module.SubCategoryRef)
                                        {
                                            SubmodulesCaterogies = [new Models.ModuleTreeItem(module)]
                                        }]
                                    });
                                }
                                else
                                {
                                    MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies ??= [];
                                    if (!MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies!.Any(a => a.Name == module.SubCategoryRef))
                                    {
                                        MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies!.Add(new Models.ModuleTreeItem(module.SubCategoryRef)
                                        {
                                            SubmodulesCaterogies = [new Models.ModuleTreeItem(module)]
                                        });
                                    }
                                    else
                                    {
                                        MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies!.First(f => f.Name == module.SubCategoryRef).SubmodulesCaterogies ??= [];
                                        MasterModules.First(f => f.Name == module.CategoryRef).SubmodulesCaterogies!.First(f => f.Name == module.SubCategoryRef).SubmodulesCaterogies!.Add(new Models.ModuleTreeItem(module));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var submodule in module.Submodules)
                        {
                            if (moduleTree.SubmodulesCaterogies?.Any(a => a.Name == submodule.CategoryRef) != true)
                            {
                                if (string.IsNullOrEmpty(submodule.CategoryRef))
                                {
                                    moduleTree.SubmodulesCaterogies ??= [];
                                    moduleTree.SubmodulesCaterogies.Add(new Models.ModuleTreeItem(submodule));
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(submodule.SubCategoryRef))
                                    {
                                        moduleTree.SubmodulesCaterogies ??= [];
                                        moduleTree.SubmodulesCaterogies.Add(new Models.ModuleTreeItem(submodule.CategoryRef)
                                        {
                                            SubmodulesCaterogies = [new Models.ModuleTreeItem(submodule)]
                                        });
                                    }
                                    else
                                    {
                                        moduleTree.SubmodulesCaterogies ??= [];
                                        moduleTree.SubmodulesCaterogies.Add(new Models.ModuleTreeItem(submodule.CategoryRef)
                                        {
                                            SubmodulesCaterogies = [new Models.ModuleTreeItem(submodule.SubCategoryRef)
                                            {
                                                SubmodulesCaterogies = [new Models.ModuleTreeItem(submodule)]
                                            }]
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var moduleTreeItem = moduleTree.SubmodulesCaterogies.First(f => f.Name == submodule.CategoryRef);
                                moduleTreeItem.SubmodulesCaterogies ??= [];
                                if (string.IsNullOrEmpty(submodule.SubCategoryRef))
                                {
                                    moduleTreeItem.SubmodulesCaterogies?.Add(new Models.ModuleTreeItem(submodule));
                                }
                                else
                                {
                                    if (!moduleTreeItem.SubmodulesCaterogies.Any(a => a.Name == submodule.SubCategoryRef))
                                    {
                                        moduleTreeItem.SubmodulesCaterogies.Add(new Models.ModuleTreeItem(submodule.SubCategoryRef)
                                        {
                                            SubmodulesCaterogies = [new Models.ModuleTreeItem(submodule)]
                                        });
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(submodule.SubCategoryRef))
                                        {
                                            moduleTreeItem.SubmodulesCaterogies.Add(new Models.ModuleTreeItem(submodule.SubCategoryRef)
                                            {
                                                SubmodulesCaterogies = [new Models.ModuleTreeItem(submodule)]
                                            });
                                        }
                                        else
                                        {
                                            moduleTreeItem.SubmodulesCaterogies.First(f => f.Name == submodule.SubCategoryRef).SubmodulesCaterogies ??= [];
                                            moduleTreeItem.SubmodulesCaterogies.First(f => f.Name == submodule.SubCategoryRef).SubmodulesCaterogies?.Add(new Models.ModuleTreeItem(submodule));
                                        }
                                    }
                                }
                            }
                        }
                        MasterModules.Add(moduleTree);
                    }
                }
            }
        }

        CollectionViewSource.GetDefaultView(MasterModules).Refresh();
    }

    private void OnExportMasterDevice(Models.DeviceItem? item)
    {
        if (item is null) return;

        (var localDirectory, var fileName, var graphicsPath) = xDocumentService.GetDevicePaths(item.Device as Core.PN.Models.Device, "");
        if (!string.IsNullOrEmpty(localDirectory) && !string.IsNullOrEmpty(fileName))
        {
            string directory;
            if (Directory.Exists(settingsService.ExportFolder))
                directory = settingsService.ExportFolder;
            else
                directory = settingsService.DefaultFolder;

            var directoryFolder = new OpenFolderDialog
            {
                //Filter = "GSDML file (*.xml)|*.xml",
                InitialDirectory = directory,
                Multiselect = false
            };

            if (directoryFolder.ShowDialog() == true)
            {
                settingsService.SaveExportFolder(directoryFolder.FolderName);

                zipperService.Zipper(localDirectory, fileName, directoryFolder.FolderName, graphicsPath);
            }
        }
    }
}
