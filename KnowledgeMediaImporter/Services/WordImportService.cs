using GroupDocs.Parser;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Nextended.Core;

namespace KnowledgeMediaImporter.Services;

public class WordImportService : IImportService
{
    public bool CanHandle(string contentType)
    {
        return contentType != "application/pdf" && MimeType.Matches(contentType, MimeType.DocumentTypes);
    }

    public async Task<string> GetKnowledgeTextAsync(byte[] fileData, CancellationToken cancellationToken, IProgressUpdate progress)
    {
        if (cancellationToken.IsCancellationRequested) return default;
        progress.Update("Read word content", 10);
        using var stream = new MemoryStream(fileData);
        stream.Position = 0;
        Parser parser = new Parser(stream);

        progress.Update("Read word content", 50);
        using TextReader reader = parser.GetText();
        progress.Done("Successfully read word content");
        return await reader.ReadToEndAsync();
    }

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult(text);
    }
    
}