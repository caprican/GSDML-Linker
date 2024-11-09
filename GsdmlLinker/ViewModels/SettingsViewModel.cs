
using System.IO;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Contracts.ViewModels;
using GsdmlLinker.Models;

using Microsoft.Extensions.Options;

namespace GsdmlLinker.ViewModels;

public class SettingsViewModel(IOptions<Core.Models.AppConfig> appConfig, IThemeSelectorService themeSelectorService, ISystemService systemService, IApplicationInfoService applicationInfoService) : ObservableObject, INavigationAware
{
    private readonly Core.Models.AppConfig appConfig = appConfig.Value;
    private readonly IThemeSelectorService themeSelectorService = themeSelectorService;
    private readonly ISystemService systemService = systemService;
    private readonly IApplicationInfoService applicationInfoService = applicationInfoService;
    private AppTheme theme;
    private string? versionDescription;
    private string? gsdmlFolder;
    private string? ioddFolder;
    private ICommand? setThemeCommand;
    private ICommand? privacyStatementCommand;
    private ICommand? gotoFolderCommand;
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public AppTheme Theme
    {
        get => theme;
        set { SetProperty(ref theme, value); }
    }

    public string? VersionDescription
    {
        get => versionDescription;
        set { SetProperty(ref versionDescription, value); }
    }

    public string? GsdmlFolder
    {
        get => gsdmlFolder;
        set { SetProperty(ref gsdmlFolder, value); }
    }

    public string? IoddFolder
    {
        get => ioddFolder;
        set { SetProperty(ref ioddFolder, value); }
    }

    public ICommand SetThemeCommand => setThemeCommand ??= new RelayCommand<string>(OnSetTheme);

    public ICommand PrivacyStatementCommand => privacyStatementCommand ??= new RelayCommand(OnPrivacyStatement);

    public ICommand GotoFolderCommand => gotoFolderCommand ??= new RelayCommand<string>(OnGotoFolder);

    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {applicationInfoService.GetVersion()}";
        Theme = themeSelectorService.GetCurrentTheme();
        GsdmlFolder = Path.Combine(localAppData, appConfig.GSDMLFolder);
        IoddFolder = Path.Combine(localAppData, appConfig.IODDFolder);
    }

    public void OnNavigatedFrom()
    {
    }

    private void OnSetTheme(string? themeName)
    {
        if (!string.IsNullOrEmpty(themeName))
        {
            var theme = (AppTheme)Enum.Parse(typeof(AppTheme), themeName);
            themeSelectorService.SetTheme(theme);
        }
    }

    private void OnPrivacyStatement() => systemService.OpenInWebBrowser(appConfig.PrivacyStatement);

    private void OnGotoFolder(string? path)
    {
        System.Diagnostics.Process.Start("explorer.exe", @$"""{path}""");
    }
}
