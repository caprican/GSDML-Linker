namespace GsdmlLinker.Core.Contracts.Services;

public interface IZipperService
{
    public void Zipper(string localDirectory, string fileName, string destination, IEnumerable<string>? graphicsPath);
}
