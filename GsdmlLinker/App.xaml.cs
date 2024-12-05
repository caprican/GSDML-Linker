using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Contracts.Views;
using GsdmlLinker.Core.Contracts.Services;
using GsdmlLinker.Core.Services;
using GsdmlLinker.Services;
using GsdmlLinker.ViewModels;
using GsdmlLinker.Views;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GsdmlLinker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? host;

    public T? GetService<T>() where T : class => host?.Services.GetService(typeof(T)) as T;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);

        //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("none");

        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
        host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(appLocation!);
                })
                .ConfigureServices(ConfigureServices)
                .Build();

        await host.StartAsync();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // App Host
        services.AddHostedService<ApplicationHostService>();

        services.AddSingleton<IDialogCoordinator, DialogCoordinator>();

        // Core Services
        services.AddSingleton<IFileService, FileService>();
        services.AddTransient<IIoddfinderService, IoddfinderService>();
        
        // Profinet core services
        services.AddSingleton<Core.PN.Contracts.Services.IDevicesService, Core.PN.Services.DevicesService>();
        services.AddTransient<Core.PN.Contracts.Services.IXDocumentService, Core.PN.Services.XDocumentService>();

        // IO-Link core services
        services.AddSingleton<Core.IOL.Contracts.Services.IDevicesService, Core.IOL.Services.DevicesService>();

        // Services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<ICultureSelectorService, CultureSelectorService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<ICaretaker, Caretaker>();
        services.AddTransient<IZipperService, ZipperService>();

        // Core Factories
        services.AddTransient<Core.PN.Contracts.Factories.IDevicesFactory, Core.PN.Factories.DevicesFactory>();

        // Builders
        services.AddTransient<Contracts.Builders.IModuleBuilder, Builders.ModuleBuilder>();
        //services.AddTransient<Core.PN.Contracts.Builders.IModuleBuilder, Core.PN.Builders.ModuleBuilder>();

        // Views and ViewModels
        services.AddTransient<IShellWindow, ShellWindow>();
        services.AddTransient<ShellViewModel>();

        services.AddTransient<DevicesViewModel>();
        services.AddTransient<DevicesPage>();

        services.AddTransient<IoddfinderViewModel>();
        services.AddTransient<IoddfinderPage>();

        services.AddTransient<IOLinkDeviceViewModel>();
        services.AddTransient<IOLinkDevicePage>();

        services.AddTransient<ProfinetDeviceViewModel>();
        services.AddTransient<ProfinetDevicePage>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        // Configuration
        services.Configure<Core.Models.AppConfig>(context.Configuration.GetSection(nameof(Core.Models.AppConfig)));
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await host!.StopAsync();
        host.Dispose();
        host = null;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // TODO: Please log and handle the exception as appropriate to your scenario
        // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0
    }
}
