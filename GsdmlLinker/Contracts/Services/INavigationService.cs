using System.Windows.Controls;

namespace GsdmlLinker.Contracts.Services;

public interface INavigationService
{
    event EventHandler<string?>? Navigated;
    event EventHandler<string?>? PageInitialized;

    bool CanGoBack { get; }

    void Initialize(Frame shellFrame);

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    void GoBack();

    void UnsubscribeNavigation();

    void CleanNavigation();
}
