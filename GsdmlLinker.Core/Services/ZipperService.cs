using System.IO.Compression;

using GsdmlLinker.Core.Contracts.Services;

using Microsoft.Extensions.Options;

namespace GsdmlLinker.Core.Services;

public class ZipperService(IOptions<Core.Models.AppConfig> appConfig) : IZipperService
{
    private readonly IOptions<Core.Models.AppConfig> appConfig = appConfig;
    private readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public void Zipper(string localDirectory, string fileName, string destination, IEnumerable<string>? graphicsPath)
    {
        var zip = ZipFile.Open(Path.Combine(destination, fileName + ".zip"), ZipArchiveMode.Create);
        zip.CreateEntryFromFile(Path.Combine(localDirectory, fileName + ".xml"), fileName + ".xml", CompressionLevel.Optimal);

        if (graphicsPath is not null)
        {
            foreach (var file in graphicsPath)
            {
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
        }
        zip.Dispose();
    }
}
