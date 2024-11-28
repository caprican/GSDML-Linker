using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.ViewModels;
using GsdmlLinker.Core.Contracts.Services;
using GsdmlLinker.Core.Models.IoddFinder;

using Microsoft.Extensions.Options;

namespace GsdmlLinker.ViewModels;

public class IoddfinderViewModel(IOptions<Core.Models.AppConfig> appConfig, Core.IOL.Contracts.Services.IDevicesService devicesService,
    IIoddfinderService ioddfinderService) : ObservableObject, INavigationAware
{
    private readonly IIoddfinderService ioddfinderService = ioddfinderService;
    private readonly Core.IOL.Contracts.Services.IDevicesService devicesService = devicesService;
    private readonly Core.Models.AppConfig appConfig = appConfig.Value;
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private string? vendorSelected;
    private Core.Models.IoddFinder.Iodd? deviceSelected;
    private Core.Models.IoddFinder.ProductVariant? productDetail;

    private ICommand? getVendorListCommmand;
    private ICommand? getDevicesVendorCommmand;
    private ICommand? deviceLoadCommand;

    public ICommand? GetVendorListCommmand => getVendorListCommmand ??= new AsyncRelayCommand(OnGetVendorList);
    public ICommand? GetDevicesVendorCommmand => getDevicesVendorCommmand ??= new AsyncRelayCommand<string?>(OnGetDevicesVendor);
    public ICommand? DeviceLoadCommand => deviceLoadCommand ??= new AsyncRelayCommand<Core.Models.IoddFinder.Iodd?>(OnLoadDevice);

    public ObservableCollection<string> VendorsName { get; set; } = [];
    public ObservableCollection<Core.Models.IoddFinder.Iodd> VendorDevices { get; set; } = [];

    public string? VendorSelected
    {
        get => vendorSelected;
        set
        {
            SetProperty(ref vendorSelected, value);
            if(!string.IsNullOrEmpty(vendorSelected))
            {
                _ = OnGetDevicesVendor(vendorSelected);
            }
        }
    }
    
    public Core.Models.IoddFinder.Iodd? DeviceSelected
    {
        get => deviceSelected;
        set
        {
            SetProperty(ref  deviceSelected, value);
            if(DeviceSelected?.ProductVariantId is not null)
            {
                _ = OnGetDevice(DeviceSelected.ProductVariantId);
            }
        }
    }

    public Core.Models.IoddFinder.ProductVariant? ProductDetail 
    { 
        get => productDetail;
        set
        {
            SetProperty(ref productDetail, value);
        }
    }

    public void OnNavigatedFrom()
    {
        
    }

    public async void OnNavigatedTo(object parameter)
    {
        await Initialize();
    }

    private async Task Initialize()
    {
        await OnGetVendorList();

    }

    private async Task OnGetVendorList()
    {
        var vendorList = await ioddfinderService.GetVendorsNameAsync();
        if(vendorList is not null)
        {
            foreach(var vendor in vendorList.Order())
            {
                VendorsName.Add(vendor);
            }
        }
    }

    private async Task OnGetDriversVendor(string vendorName)
    {
        var driverList = await ioddfinderService.GetDriversAsync(vendorName);
        if (driverList is not null) 
        {

        }
    }

    private async Task OnGetDevicesVendor(string? vendorName)
    {
        if (vendorName is null) return;
        var devices = await ioddfinderService.GetProductVariantFromVendor(vendorName);
        if (devices is not null)
        {
            VendorDevices.Clear();
            foreach (var device in devices)
            {
                VendorDevices.Add(device);
            }
        }
    }

    private async Task OnGetDevice(long productvariantId)
    {
        ProductDetail = await ioddfinderService.GetProductVariantAsync(productvariantId);
    }



    private async Task OnLoadDevice(Iodd? iodd)
    {
        if(iodd is null) return;

        var zipStream = await ioddfinderService.GetIoddZipAsync(iodd.VendorId, iodd.IoddId);
        if (zipStream is null) return;
        using var zip = new ZipArchive(new MemoryStream(zipStream), ZipArchiveMode.Read);

        devicesService.AddDevice(zip);
    }
}
