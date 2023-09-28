namespace KnowledgeMediaImporter.Data;

public interface IImportService
{
    public bool CanHandle(string contentType);

    public Task<string> GetKnowledgeTextAsync(byte[] fileData, Action<string, double> progress);

    public Task<string> AfterPrepareAsync(string text);
}