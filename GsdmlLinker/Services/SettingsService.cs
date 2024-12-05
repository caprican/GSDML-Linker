using GsdmlLinker.Contracts.Services;

namespace GsdmlLinker.Services;

public class SettingsService : ISettingsService
{
    public string? Theme => App.Current.Properties[nameof(Theme)]?.ToString();

    public string? CurrentCulture => App.Current.Properties[nameof(CurrentCulture)]?.ToString();

    public string? NavigationFolder => App.Current.Properties[nameof(NavigationFolder)]?.ToString();
    public string? ExportFolder => App.Current.Properties[nameof(ExportFolder)]?.ToString();
    public string DefaultFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


    public void InitializeSettings()
    {
        if (!App.Current.Properties.Contains(nameof(NavigationFolder)) || string.IsNullOrEmpty(App.Current.Properties[nameof(NavigationFolder)]?.ToString()))
        {
            App.Current.Properties[nameof(NavigationFolder)] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        if (!App.Current.Properties.Contains(nameof(ExportFolder)) || string.IsNullOrEmpty(App.Current.Properties[nameof(ExportFolder)]?.ToString()))
        {
            App.Current.Properties[nameof(ExportFolder)] = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }


    }

    public void SaveNavigationFolder(string navigationFolder)
    {
        App.Current.Properties[nameof(NavigationFolder)] = navigationFolder;
    }

    public void SaveExportFolder(string exportFolder)
    {
        App.Current.Properties[nameof(ExportFolder)] = exportFolder;
    }
}
