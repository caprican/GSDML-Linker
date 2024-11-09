namespace GsdmlLinker.Contracts.Services;

public interface IXDocumentService
{
    bool Load(string filePath);

    (string?, List<string>?) Create();

    bool SaveAs(string path);
}
