using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Contracts.Views;
using GsdmlLinker.ViewModels;

using Microsoft.Extensions.Hosting;

namespace GsdmlLinker.Services;

public class ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
                                    IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService,
                                    ISettingsService settingsService,
                                    Core.PN.Contracts.Services.IDevicesService gsdDevicesService, Core.IOL.Contracts.Services.IDevicesService IoddDevicesService
                                    ) : IHostedService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly INavigationService navigationService = navigationService;
    private readonly IPersistAndRestoreService persistAndRestoreService = persistAndRestoreService;
    private readonly IThemeSelectorService themeSelectorService = themeSelectorService;
    private readonly ISettingsService settingsService = settingsService;

    private readonly Core.PN.Contracts.Services.IDevicesService gsdDevicesService = gsdDevicesService;
    private readonly Core.IOL.Contracts.Services.IDevicesService IoddDevicesService = IoddDevicesService;

    private bool _isInitialized;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize services that you need before app activation
        await InitializeAsync();

        await HandleActivationAsync();

        // Tasks after activation
        await StartupAsync();
        _isInitialized = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        persistAndRestoreService.PersistData();
        await Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            persistAndRestoreService.RestoreData();
            themeSelectorService.InitializeTheme();
            settingsService.InitializeSettings();

            await Task.CompletedTask;

            IoddDevicesService.InitializeSettings();
            gsdDevicesService.InitializeSettings();

            await Task.CompletedTask;

        }
    }

    private async Task StartupAsync()
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private async Task HandleActivationAsync()
    {
        if (!System.Windows.Application.Current.Windows.OfType<IShellWindow>().Any())
        {
            // Default activation that navigates to the apps default page
            if(serviceProvider.GetService(typeof(IShellWindow)) is IShellWindow shellWindow)
            {
                navigationService.Initialize(shellWindow.GetNavigationFrame());
                shellWindow.ShowWindow();
                navigationService.NavigateTo(typeof(DevicesViewModel).FullName!);
            }
            
            await Task.CompletedTask;
        }
    }
}
