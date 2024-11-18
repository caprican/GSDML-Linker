using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using GsdmlLinker.Contracts.Services;
using GsdmlLinker.ViewModels;
using GsdmlLinker.Views;

namespace GsdmlLinker.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> pages = [];
    private readonly IServiceProvider serviceProvider;

    public PageService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        Configure<DevicesViewModel, DevicesPage>();
        Configure<IoddfinderViewModel, IoddfinderPage>();

        //Configure<ProfinetDeviceViewModel, ProfinetDevicePage>();
        //Configure<IOLinkDeviceViewModel, IOLinkDevicePage>();
        
        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (pages)
        {
            if (!pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    public Page? GetPage(string key)
    {
        var pageType = GetPageType(key);
        return serviceProvider.GetService(pageType) as Page;
    }

    private void Configure<VM, V>() where VM : ObservableObject where V : Page
    {
        lock (pages)
        {
            var key = typeof(VM).FullName;
            if (string.IsNullOrEmpty(key) || pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {pages.First(p => p.Value == type).Key}");
            }

            pages.Add(key, type);
        }
    }
}
