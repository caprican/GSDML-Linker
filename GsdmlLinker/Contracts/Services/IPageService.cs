using System.Windows.Controls;

namespace GsdmlLinker.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page? GetPage(string key);
}
