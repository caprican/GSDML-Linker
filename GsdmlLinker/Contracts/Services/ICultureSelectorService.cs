using System.Globalization;

namespace GsdmlLinker.Contracts.Services;

public interface ICultureSelectorService
{
    void InitializeCulture();

    void SetCulture(string culture);

    CultureInfo GetCurrentCulture();
}
