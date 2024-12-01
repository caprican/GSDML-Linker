﻿using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.ViewModels;
using GsdmlLinker.Core.Contracts.Services;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.Win32;

namespace GsdmlLinker.ViewModels;

public class DevicesViewModel(Contracts.Services.ISettingsService settingsService, IDialogCoordinator dialogCoordinator,
                              Core.IOL.Contracts.Services.IDevicesService iolDevicesService, Core.PN.Contracts.Services.IDevicesService pnDevicesService,
                              Contracts.Builders.IModuleBuilder moduleBuilder, Contracts.Services.ICaretaker caretaker,
                              Core.PN.Contracts.Services.IXDocumentService xDocumentService, IZipperService zipperService,
                              IIoddfinderService ioddfinderService) : ObservableObject, INavigationAware
{
    private readonly Contracts.Services.ISettingsService settingsService = settingsService;
    private readonly IDialogCoordinator dialogCoordinator = dialogCoordinator;
    private readonly Core.PN.Contracts.Services.IDevicesService pnDevicesService = pnDevicesService;
    private readonly Core.IOL.Contracts.Services.IDevicesService iolDevicesService = iolDevicesService;
    
    private readonly Contracts.Builders.IModuleBuilder moduleBuilder = moduleBuilder;
    private readonly Contracts.Services.ICaretaker caretaker = caretaker;
    private readonly Core.PN.Contracts.Services.IXDocumentService xDocumentService = xDocumentService;
    private readonly IZipperService zipperService = zipperService;
    private readonly IIoddfinderService ioddfinderService = ioddfinderService;

    private ICommand? addSlaveDeviceCommand;
    private ICommand? closeSlaveDeviceCommand;
    private ICommand? saveSlaveDeviceCommand;
    private ICommand? deleteSlaveDeviceCommand;
    private ICommand? renameDeviceCommand;
    private ICommand? saveMasterDeviceCommand;
    private ICommand? cancelMasterDeviceCommand;
    private ICommand? viewSubParametersCommand;
    private ICommand? exportMasterDeviceCommand;
    private ICommand? saveExportMasterDeviceCommand;

    private ObservableCollection<Models.VendorItem>? masterVendors;
    private ObservableCollection<Models.VendorItem>? slaveVendors;
    private ObservableCollection<Models.ModuleTreeItem>? masterModules;
    private Models.DeviceItem? masterDeviceSelected;
    private Models.DeviceItem? slaveDeviceSelected;
    private Models.ModuleTreeItem? masterModuleSelected;
    private Visibility slaveListVisibility = Visibility.Collapsed;
    private double? firstColumnSize = null;
    private double? thirdColumnSize = 0;
    private Visibility slaveDeviceListVisibility = Visibility.Visible;
    private Visibility updateSlaveDeviceVisibility = Visibility.Collapsed;
    private Visibility saveMasterDeviceVisibility = Visibility.Collapsed;
    private bool canSaveMasterDevice = false;
    private bool? selectAll = true;

    private string deviceRename = string.Empty;

    public event EventHandler<string?>? PageInitialized;


    public ObservableCollection<Models.VendorItem>? MasterVendors
    {
        get => masterVendors;
        set => SetProperty(ref masterVendors, value);
    }

    public ObservableCollection<Models.VendorItem>? SlaveVendors
    {
        get => slaveVendors;
        set => SetProperty(ref slaveVendors, value);
    }

    public ObservableCollection<Models.ModuleTreeItem>? MasterModules 
    {
        get => masterModules;
        set => SetProperty(ref masterModules, value);
    }

    public ObservableCollection<Core.Models.DeviceParameter> SlaveParameters { get; set; } = [];

    public Models.DeviceItem? MasterDeviceSelected
    {
        get => masterDeviceSelected;
        set
        {
            SetProperty(ref masterDeviceSelected, value);
            if (MasterDeviceSelected is not null)
            {
                GetMasterModule();
                FirstColumnSize = 0;
                SaveMasterDeviceVisibility = Visibility.Visible;
            }
            else
            {
                FirstColumnSize = null;
                ThirdColumnSize = 0;
                SaveMasterDeviceVisibility = Visibility.Collapsed;
                CanSaveMasterDevice = false;
            }
        }
    }

    public Models.DeviceItem? SlaveDeviceSelected
    {
        get => slaveDeviceSelected;
        set
        {
            SetProperty(ref slaveDeviceSelected, value);
            if (SlaveDeviceSelected is not null)
            {
                DeviceRename = SlaveDeviceSelected.Name;
                GetSlaveParameters();
                SaveMasterDeviceVisibility = Visibility.Collapsed;
                UpdateSlaveDeviceVisibility = Visibility.Visible;
                SlaveListVisibility = Visibility.Collapsed;

            }
            else
            {
                DeviceRename = string.Empty;
                UpdateSlaveDeviceVisibility = Visibility.Collapsed;
                SlaveListVisibility = Visibility.Visible;
                SaveMasterDeviceVisibility = Visibility.Visible;
            }
        }
    }

    public Models.ModuleTreeItem? MasterModuleSelected
    {
        get => masterModuleSelected;
        set
        {
            SetProperty(ref masterModuleSelected, value);

            if (MasterModuleSelected is not null)
            {
                SlaveDeviceSelected = null;
                ThirdColumnSize = null;
                GetSlaveParameters();
            }
            else
            {
                UpdateSlaveDeviceVisibility = Visibility.Collapsed;
                SlaveListVisibility = Visibility.Visible;
            }
        }
    }

    public Visibility SlaveListVisibility
    { 
        get => slaveListVisibility;
        set => SetProperty(ref slaveListVisibility, value);
    }

    public double? FirstColumnSize
    {
        get => firstColumnSize;
        set => SetProperty(ref firstColumnSize, value);
    }

    public double? ThirdColumnSize
    {
        get => thirdColumnSize;
        set => SetProperty(ref thirdColumnSize, value);
    }

    public Visibility SlaveDeviceListVisibility
    {
        get => slaveDeviceListVisibility;
        set => SetProperty(ref slaveDeviceListVisibility, value);
    }

    public Visibility UpdateSlaveDeviceVisibility
    {
        get => updateSlaveDeviceVisibility;
        set => SetProperty(ref updateSlaveDeviceVisibility, value);
    }

    public Visibility SaveMasterDeviceVisibility
    {
        get => saveMasterDeviceVisibility;
        set => SetProperty(ref saveMasterDeviceVisibility, value);
    }

    public bool CanSaveMasterDevice
    {
        get => canSaveMasterDevice;
        set => SetProperty(ref canSaveMasterDevice, value);
    }

    public string DeviceRename
    {
        get => deviceRename;
        set => SetProperty(ref deviceRename, value);
    }

    public bool? SelectAll
    {
        get => selectAll;
        set
        {
            SetProperty(ref selectAll, value);
            SelectParameterChange();
        }
    }

    public ICommand AddSlaveDeviceCommand => addSlaveDeviceCommand ??= new RelayCommand(OnAddSlaveDevice);
    public ICommand CloseSlaveDeviceCommand => closeSlaveDeviceCommand ??= new RelayCommand(OnCloseSlaveDevice);
    public ICommand SaveSlaveDeviceCommand => saveSlaveDeviceCommand ??= new RelayCommand(OnSaveSlaveDevice);
    public ICommand DeletSlaveDeviceCommand => deleteSlaveDeviceCommand ??= new AsyncRelayCommand<Models.ModuleTreeItem>(OnDeletSlaveDevice);
    public ICommand RenameDeviceCommand => renameDeviceCommand ??= new AsyncRelayCommand(OnRenameDevice);
    public ICommand SaveMasterDeviceCommand => saveMasterDeviceCommand ??= new RelayCommand(OnSaveMasterDevice);
    public ICommand CancelMasterDeviceCommand => cancelMasterDeviceCommand ??= new RelayCommand(OnCancelMasterDevice);
    public ICommand ViewSubParametersCommand => viewSubParametersCommand ??= new RelayCommand<Core.Models.DeviceParameter>(OnViewSubParameters);
    public ICommand ExportMasterDeviceCommand => exportMasterDeviceCommand ??= new RelayCommand<Models.DeviceItem>(OnExportMasterDevice);
    public ICommand SaveExportMasterDeviceCommand => saveExportMasterDeviceCommand ??= new RelayCommand(OnSaveExportMasterDevice);

    public async void OnNavigatedTo(object parameter)
    {
        pnDevicesService.DeviceAdded += PnDevicesService_DeviceAdded;
        iolDevicesService.DeviceAdded += IolDevicesService_DeviceAdded;

        GetMasterDevices();

        GetSlaveDevices();
    }

    public void OnNavigatedFrom()
    {
        pnDevicesService.DeviceAdded -= PnDevicesService_DeviceAdded;
        iolDevicesService.DeviceAdded -= IolDevicesService_DeviceAdded;
    }

    private void PnDevicesService_DeviceAdded(object? sender, Core.Models.DeviceEventArgs e)
    {
        DialogCoordinator.Instance.ShowMessageAsync(this, "GSDML added", $"New GSDML added {e.Device?.Name}", MessageDialogStyle.Affirmative);
        GetMasterDevices();
    }

    private void IolDevicesService_DeviceAdded(object? sender, Core.Models.DeviceEventArgs e)
    {
        DialogCoordinator.Instance.ShowMessageAsync(this, "IODD added", $"New IODD added {e.Device?.Name}", MessageDialogStyle.Affirmative);
        GetSlaveDevices();
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
                };

                foreach (var item in group)
                {
                    foreach (var dap in AddDevice(item))
                    {
                        vendor.Devices ??= [];

                        if(vendor.Devices.Any(a => a.DeviceId == dap.DeviceId && a.DNS == dap.DNS))
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
            });
        }
        return result;
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

    private void GetMasterModule()
    {
        MasterModules = [];
        if(MasterDeviceSelected is not null)
        {
            var modules = pnDevicesService.GetModules(MasterDeviceSelected.VendorId, MasterDeviceSelected.DeviceId, MasterDeviceSelected.DeviceAccessId, MasterDeviceSelected.Version);
            if(modules is not null)
            {
                foreach (var module in modules)
                {
                    var moduleTree = new Models.ModuleTreeItem(module);

                    if(module.Submodules is null)
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

    private void GetSlaveParameters()
    {
        SlaveParameters.Clear();
        SelectAll = true;
        if (SlaveDeviceSelected is not null)
        {
            var parameters = iolDevicesService.GetParameters(SlaveDeviceSelected.VendorId, SlaveDeviceSelected.DeviceId);
            if(parameters is not null)
            {
                foreach (var item in parameters)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                    SlaveParameters.Add(item);
                }
            }
            if(MasterDeviceSelected is not null && MasterModuleSelected is not null && !string.IsNullOrEmpty(MasterModuleSelected.ProfinetDeviceId))
            {
                var pnParameters = pnDevicesService.GetRecordParameters(MasterDeviceSelected.VendorId, MasterDeviceSelected.DeviceId, MasterDeviceSelected.Version, MasterModuleSelected.ProfinetDeviceId);
                if (pnParameters is not null)
                {
                    foreach (var parameter in SlaveParameters)
                    {
                        if(pnParameters.SingleOrDefault(s => s.Index == parameter.Index && s.Subindex == parameter.Subindex) is Core.Models.DeviceParameter deviceSubparameter)
                        {
                            parameter.DefaultValue = deviceSubparameter.DefaultValue;
                        }
                        else if(pnParameters.FirstOrDefault(s => s.Index == parameter.Index) is Core.Models.DeviceParameter deviceParameter)
                        {
                            parameter.DefaultValue = deviceParameter.DefaultValue;
                        }
                        else
                        {
                            parameter.IsSelected = false;
                        }
                    }
                }

                CollectionViewSource.GetDefaultView(SlaveParameters).Refresh();
            }
        }
        else if (MasterModuleSelected is not null)
        {
            SlaveDeviceSelected = SlaveVendors.FirstOrDefault(f => f.Id == MasterModuleSelected.VendorId.ToString())?.Devices?.FirstOrDefault(f => f.DeviceId == MasterModuleSelected.DeviceId.ToString());

            if(SlaveDeviceSelected is null)
            {
                try
                {
                //ioddfinderService.

                }
                catch
                {

                }
            }
            /// TODO Request
        }
    }

    private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "IsSelected":
                if(sender is Core.Models.DeviceParameter deviceParameter)
                {
                    if (SelectAll == true && deviceParameter.IsSelected == false)
                    {
                        SelectAll = null;
                    }
                    else if (SelectAll == false && deviceParameter.IsSelected == true)
                    {
                        SelectAll = null;
                    }
                }
                break;
        }
    }

    private void OnAddSlaveDevice()
    {
        SlaveListVisibility = Visibility.Visible;
        ThirdColumnSize = null;

        SlaveDeviceListVisibility = Visibility.Collapsed;
    }

    private async Task OnRenameDevice()
    {
        var diag = await DialogCoordinator.Instance.ShowInputAsync(this, "Change name", "rename device", new MetroDialogSettings { DefaultText = DeviceRename });
        if (!string.IsNullOrEmpty(diag))
        {
            DeviceRename = diag;
        }
    }

    private void SelectParameterChange()
    {
        if (SelectAll == true)
        {
            foreach (var parameter in SlaveParameters.Where(p => !p.IsReadOnly))
            {
                parameter.IsSelected = true;
            }
        }
        else if (selectAll == false)
        {
            foreach (var parameter in SlaveParameters.Where(p => !p.IsReadOnly))
            {
                parameter.IsSelected = false;
            }
        }
    }

    private void OnCloseSlaveDevice()
    {
        SlaveDeviceSelected = null;
    }

    private void OnSaveSlaveDevice()
    {
        if (MasterDeviceSelected is not null && SlaveDeviceSelected is not null)
        {
            var moduleParameters = SlaveParameters.OrderBy(param => param.Index).OrderBy(param => param.Subindex).GroupBy(group => group.Index);
            if(string.IsNullOrEmpty(MasterModuleSelected?.ProfinetDeviceId))
            {
                moduleBuilder.CreateModule(MasterDeviceSelected, SlaveDeviceSelected, moduleParameters.Where(w => w.First().IsSelected));
            }
            else
            {
                moduleBuilder.ModifiedModule(MasterDeviceSelected, SlaveDeviceSelected, moduleParameters.Where(w => w.First().IsSelected), MasterModuleSelected.ProfinetDeviceId);
            }
            //caretaker.Backup(MasterDeviceSelected.device);
        }

        SlaveDeviceSelected = null;
        GetSlaveDevices();
        int masterIndex = -1;
        if (MasterModules is not null && MasterModuleSelected is not null)
        {
            masterIndex = MasterModules.IndexOf(MasterModuleSelected);
        }

        GetMasterModule();

        if (MasterModules is not null && masterIndex >= 0)
        {
            MasterModuleSelected = MasterModules[masterIndex];
        }
        CanSaveMasterDevice = /*MasterDeviceSelected?.device?.Editing ==*/ true;
    }

    private async Task OnDeletSlaveDevice(Models.ModuleTreeItem? module)
    {
        if (module is null) return;

        switch(module.State)
        {
            case Core.Models.ItemState.None:
            case Core.Models.ItemState.Original:
                module.State = Core.Models.ItemState.Deleted;

                CanSaveMasterDevice = /*MasterDeviceSelected?.device?.Editing ==*/ true;
                break;
            case Core.Models.ItemState.Created:
                var diag = await DialogCoordinator.Instance.ShowMessageAsync(this, "Delet module", "Vraiment ?", MessageDialogStyle.AffirmativeAndNegative);
                if (diag == MessageDialogResult.Affirmative)
                {
                    if (MasterDeviceSelected is not null)
                    {
                        moduleBuilder.DeletedModule(MasterDeviceSelected, module.ProfinetDeviceId ?? string.Empty);
                    }

                    CanSaveMasterDevice = true;

                    SlaveDeviceSelected = null;
                    MasterModuleSelected = null;
                    //GetMasterModule();
                }
                break;
            case Core.Models.ItemState.Editing:
            case Core.Models.ItemState.Modified:
                break;
            case Core.Models.ItemState.Deleted:
                module.State = module.OriginalState;
                break;
        }
    }

    private void OnSaveMasterDevice()
    {
        if(MasterDeviceSelected is not null)
        {
            //MasterDeviceSelected.device.EndEdit();
            foreach (var dap in MasterDeviceSelected.Device!.DeviceAccessPoints) 
            {
                if(dap.Id == MasterDeviceSelected.DeviceAccessId)
                {
                    dap.Description = MasterDeviceSelected.Description;
                }
            }
            //MasterDeviceSelected.device!.Description = MasterDeviceSelected.Description;
            (var localDirectory, var fileName, var graphicsPath) = xDocumentService.Create(MasterDeviceSelected.Device as Core.PN.Models.Device, "");
        }

        MasterDeviceSelected = null;
    }

    private void OnCancelMasterDevice()
    {
        if(MasterDeviceSelected is not null)
        { 
            MasterDeviceSelected.Editing = false;
            //MasterDeviceSelected.device.CancelEdit();
            MasterDeviceSelected = null;

            GetMasterDevices();
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

    private void OnExportMasterDevice(Models.DeviceItem? item)
    {
        if(item is null) return;

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

    private void OnSaveExportMasterDevice()
    {
        if (MasterDeviceSelected is not null)
        {
            //MasterDeviceSelected.device.EndEdit();
            foreach (var dap in MasterDeviceSelected.Device!.DeviceAccessPoints)
            {
                if (dap.Id == MasterDeviceSelected.DeviceAccessId)
                {
                    dap.Description = MasterDeviceSelected.Description;
                }
            }
            //MasterDeviceSelected.device!.Description = MasterDeviceSelected.Description;
            (var localDirectory, var fileName, var graphicsPath) = xDocumentService.Create(MasterDeviceSelected.Device as Core.PN.Models.Device, "");

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

        MasterDeviceSelected = null;
    }

    public void OnLoadCompleted()
    {
        
    }
}
