
using KnowledgeMediaImporter.Model;

namespace KnowledgeMediaImporter.Contracts;

public interface IImportService
{
    public bool CanHandle(string contentType);

    public Task<string> GetKnowledgeTextAsync(byte[] fileData,KnowledgeTargetSettings targetSettings, CancellationToken cancellationToken, IProgressUpdate progress);

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken);
}