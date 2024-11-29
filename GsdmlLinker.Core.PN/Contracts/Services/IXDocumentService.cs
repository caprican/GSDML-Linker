namespace GsdmlLinker.Core.PN.Contracts.Services;

public interface IXDocumentService
{
    //bool Load(string filePath);

    public (string?, string?, List<string>?) Create(Models.Device? device, string path);

    public (string?, string?, List<string>?) GetDevicePaths(Models.Device? device, string path);
    
    //public bool SaveAs(string path);
}
