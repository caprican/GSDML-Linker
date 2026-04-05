
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GsdmlLinker.Core.PN;

public class App
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<Contracts.Services.IDevicesService, Services.DevicesService>();
        services.AddTransient<Contracts.Services.IXDocumentService, Services.XDocumentService>();
        services.AddTransient<Contracts.Services.ISimaticService, Services.SimaticService>();

        // Core Factories
        services.AddTransient<Contracts.Factories.IDevicesFactory, Factories.DevicesFactory>();
    }
}