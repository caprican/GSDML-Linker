using System.Collections;
using System.IO;

using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Core.Contracts.Services;

using Microsoft.Extensions.Options;

namespace GsdmlLinker.Services;

public class PersistAndRestoreService(IFileService fileService, IOptions<Core.Models.AppConfig> appConfig) : IPersistAndRestoreService
{
    private readonly IFileService _fileService = fileService;
    private readonly Core.Models.AppConfig _appConfig = appConfig.Value;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public void PersistData()
    {
        if (App.Current.Properties is not null)
        {
            var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
            var fileName = _appConfig.AppPropertiesFileName;
            _fileService.Save(folderPath, fileName, App.Current.Properties);
        }
    }

    public void RestoreData()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationsFolder);
        var fileName = _appConfig.AppPropertiesFileName;
        var properties = _fileService.Read<IDictionary>(folderPath, fileName);
        if (properties is not null)
        {
            foreach (DictionaryEntry property in properties)
            {
                App.Current.Properties.Add(property.Key, property.Value);
            }
        }
    }
}
