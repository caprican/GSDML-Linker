using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Properties;

using MahApps.Metro.Controls;

using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace GsdmlLinker.ViewModels;
public class ShellViewModel(INavigationService navigationService,
                            IOptions<Core.Models.AppConfig> appConfig, ISettingsService settingsService,
    Core.PN.Contracts.Services.IDevicesService gsdDevicesService, Core.IOL.Contracts.Services.IDevicesService iodDevicesService) : ObservableObject
{
    private readonly INavigationService _navigationService = navigationService;
    private readonly ISettingsService settingsService = settingsService;
    private readonly Core.PN.Contracts.Services.IDevicesService gsdDevicesService = gsdDevicesService;
    private readonly Core.IOL.Contracts.Services.IDevicesService iodDevicesService = iodDevicesService;
    private readonly Core.Models.AppConfig appConfig = appConfig.Value;
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private HamburgerMenuItem? _selectedMenuItem;
    private HamburgerMenuItem? _selectedOptionsMenuItem;
    private RelayCommand? goBackCommand;
    private RelayCommand? saveCommand;
    private RelayCommand? loadCommand;
    private ICommand? _menuItemInvokedCommand;
    private ICommand? _optionsMenuItemInvokedCommand;
    private ICommand? _loadedCommand;
    private ICommand? _unloadedCommand;

    private ICommand? addDeviceCommand;


    public HamburgerMenuItem? SelectedMenuItem
    {
        get { return _selectedMenuItem; }
        set { SetProperty(ref _selectedMenuItem, value); }
    }

    public HamburgerMenuItem? SelectedOptionsMenuItem
    {
        get { return _selectedOptionsMenuItem; }
        set { SetProperty(ref _selectedOptionsMenuItem, value); }
    }

    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } =
    [
        new HamburgerMenuGlyphItem() { Label = Resources.ShellDevicesPage, Glyph = "\uE7F7", TargetPageType = typeof(DevicesViewModel) },
        //new HamburgerMenuIconItem() {Label = Resources.ShellDevicesPage, Icon = "/Assets/iolink.png" , TargetPageType = typeof(DevicesViewModel)},
        new HamburgerMenuGlyphItem() { Label = Resources.ShellDevicesPage, Glyph = "\uE721", TargetPageType = typeof(IoddfinderViewModel) },
        //new HamburgerMenuGlyphItem() { Label = Resources.ShellProfinetDevicePage, Glyph = "\uE7F7", TargetPageType = typeof(ProfinetDeviceViewModel) },
        //new HamburgerMenuGlyphItem() { Label = Resources.ShellIOLinkDevicePage, Glyph = "\uED10", TargetPageType = typeof(IOLinkDeviceViewModel) },
    ];

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } =
    [
        new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
    ];

    public RelayCommand GoBackCommand => goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);

    public ICommand MenuItemInvokedCommand => _menuItemInvokedCommand ??= new RelayCommand(OnMenuItemInvoked);
    public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ??= new RelayCommand(OnOptionsMenuItemInvoked);
    public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);
    public ICommand UnloadedCommand => _unloadedCommand ??= new RelayCommand(OnUnloaded);

    public ICommand AddDeviceCommand => addDeviceCommand ??= new RelayCommand(OnAddDevice);

    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack() => _navigationService.CanGoBack;

    private void OnGoBack() => _navigationService.GoBack();

    private void OnMenuItemInvoked() => NavigateTo(SelectedMenuItem!.TargetPageType!);

    private void OnOptionsMenuItemInvoked() => NavigateTo(SelectedOptionsMenuItem!.TargetPageType!);

    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel != null)
        {
            _navigationService.NavigateTo(targetViewModel.FullName!);
        }
    }

    private void OnNavigated(object? sender, string? viewModelName)
    {
        var item = MenuItems
                    .OfType<HamburgerMenuItem>()
                    .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        if (item != null)
        {
            SelectedMenuItem = item;
        }
        else
        {
            SelectedOptionsMenuItem = OptionMenuItems
                    .OfType<HamburgerMenuItem>()
                    .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        }

        GoBackCommand.NotifyCanExecuteChanged();
    }

    private void OnAddDevice()
    {
        var openFolder = new OpenFolderDialog
        {
            Multiselect = true,
            InitialDirectory = settingsService.NavigationFolder
        };

        if (openFolder.ShowDialog() == true)
        {
            settingsService.SaveNavigationFolder(Path.GetDirectoryName(openFolder.FolderName)!);

            foreach (var folderName in openFolder.FolderNames)
            {
                foreach(var filePath in Directory.EnumerateFiles(folderName))
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    if(Path.GetExtension(filePath) == ".zip")
                    {
                        var folderPath = Unzip(filePath, appConfig.GSDMLFolder);
                    }
                    else if (Core.PN.Regexs.FileNameRegex().Match(fileName).Success || fileName.Contains("gsdml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        gsdDevicesService.AddDevice(folderName);
                 
                        //_navigationService.NavigateTo(typeof(ProfinetDeviceViewModel).FullName!);
                    }
                    else if (Core.IOL.Regexs.FileNameRegex().Match(fileName).Success || fileName.Contains("iodd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        iodDevicesService.AddDevice(folderName);

                        //_navigationService.NavigateTo(typeof(IOLinkDeviceViewModel).FullName!);
                    }
                    else
                    {
                        //DialogCoordinator.Instance.ShowMessageAsync(this, "unknown file", @$"no GSDML file or IODD file found on {filePath}", MessageDialogStyle.Affirmative);
                    }
                }
            }
        }
    }



    private string Unzip(string filePath, string directory)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (Directory.Exists(Path.Combine(localAppData, directory, fileName)))
        {
            /// TODO: Message Dossier existant
            Directory.Delete(Path.Combine(localAppData, directory, fileName), true);
        }

        var zip = ZipFile.Open(filePath, ZipArchiveMode.Read);

        var content = zip.Entries.SelectMany(s => s.FullName.Split('/')).Where(x => !Path.HasExtension(x)).Distinct();

        var folderPath = content.Any() ?
            Path.Combine(localAppData, directory) : Path.Combine(localAppData, directory, fileName);


        zip.ExtractToDirectory(folderPath);
        return Path.Combine(folderPath, fileName);
    }
}
