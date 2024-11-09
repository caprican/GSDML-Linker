namespace GsdmlLinker.Contracts.Services;

public interface ISettingsService
{
    public string? NavigationFolder { get; }
    public string? ExportFolder { get; }
    void InitializeSettings();

    public void SaveNavigationFolder(string navigationFolder);

    public void SaveExportFolder(string exportFolder);
}
