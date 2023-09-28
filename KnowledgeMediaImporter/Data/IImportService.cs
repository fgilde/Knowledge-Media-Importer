namespace KnowledgeMediaImporter.Data;

public interface IImportService
{
    public bool CanHandle(string contentType);

    public Task<string> CreateKnowledgeTextAsync(byte[] fileData);
}