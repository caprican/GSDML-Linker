using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GsdmlLinker.Core.IOL;

public class App
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<Contracts.Services.IDevicesService, Services.DevicesService>();
    }
}
