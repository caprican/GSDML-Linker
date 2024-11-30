using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Properties;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace GsdmlLinker.ViewModels;
public class ShellViewModel(INavigationService navigationService, IDialogCoordinator dialogCoordinator,
                            IOptions<Core.Models.AppConfig> appConfig, ISettingsService settingsService,
    Core.PN.Contracts.Services.IDevicesService gsdDevicesService, Core.IOL.Contracts.Services.IDevicesService iodDevicesService) : ObservableObject
{
    private readonly INavigationService navigationService = navigationService;
    private readonly IDialogCoordinator dialogCoordinator = dialogCoordinator;
    private readonly ISettingsService settingsService = settingsService;
    private readonly Core.PN.Contracts.Services.IDevicesService gsdDevicesService = gsdDevicesService;
    private readonly Core.IOL.Contracts.Services.IDevicesService iodDevicesService = iodDevicesService;
    private readonly Core.Models.AppConfig appConfig = appConfig.Value;
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private HamburgerMenuItem? selectedMenuItem;
    private HamburgerMenuItem? selectedOptionsMenuItem;
    private ProgressDialogController? progressController;
    private RelayCommand? goBackCommand;
    private RelayCommand? saveCommand;
    private RelayCommand? loadCommand;
    private ICommand? menuItemInvokedCommand;
    private ICommand? optionsMenuItemInvokedCommand;
    private ICommand? loadedCommand;
    private ICommand? unloadedCommand;

    private ICommand? addDeviceCommand;

    public HamburgerMenuItem? SelectedMenuItem
    {
        get { return selectedMenuItem; }
        set { SetProperty(ref selectedMenuItem, value); }
    }

    public HamburgerMenuItem? SelectedOptionsMenuItem
    {
        get { return selectedOptionsMenuItem; }
        set { SetProperty(ref selectedOptionsMenuItem, value); }
    }

    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } =
    [
        new HamburgerMenuGlyphItem() { Label = Resources.ShellDevicesPage, Glyph = "\uE968", TargetPageType = typeof(DevicesViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellIoddfinderPage, Glyph = "\uE721", TargetPageType = typeof(IoddfinderViewModel) },
        new HamburgerMenuIconItem() { Label = Resources.ShellProfinetDevicePage, Icon = "/Assets/profinet.png", TargetPageType = typeof(ProfinetDeviceViewModel) },
        new HamburgerMenuIconItem() { Label = Resources.ShellIOLinkDevicesPage, Icon = "/Assets/io-link.png" , TargetPageType = typeof(IOLinkDeviceViewModel) },
    ];

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } =
    [
        new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
    ];

    public RelayCommand GoBackCommand => goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);

    public ICommand MenuItemInvokedCommand => menuItemInvokedCommand ??= new RelayCommand(OnMenuItemInvoked);
    public ICommand OptionsMenuItemInvokedCommand => optionsMenuItemInvokedCommand ??= new RelayCommand(OnOptionsMenuItemInvoked);
    public ICommand LoadedCommand => loadedCommand ??= new RelayCommand(OnLoaded);
    public ICommand UnloadedCommand => unloadedCommand ??= new RelayCommand(OnUnloaded);

    public ICommand AddDeviceCommand => addDeviceCommand ??= new RelayCommand(OnAddDevice);

    private void OnLoaded()
    {
        navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded()
    {
        navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack() => navigationService.CanGoBack;

    private void OnGoBack() => navigationService.GoBack();

    private void OnMenuItemInvoked() => NavigateTo(SelectedMenuItem!.TargetPageType!);

    private void OnOptionsMenuItemInvoked() => NavigateTo(SelectedOptionsMenuItem!.TargetPageType!);

    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel is not null)
        {
            navigationService.NavigateTo(targetViewModel.FullName!);
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
