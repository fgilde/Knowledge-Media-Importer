using GroupDocs.Parser;
using KnowledgeMediaImporter.Contracts;
using Nextended.Core;

namespace KnowledgeMediaImporter.Services;

public class WordImportService : IImportService
{
    public bool CanHandle(string contentType)
    {
        return contentType != "application/pdf" && MimeType.Matches(contentType, MimeType.DocumentTypes);
    }

    public async Task<string> GetKnowledgeTextAsync(byte[] fileData, CancellationToken cancellationToken, Action<string, double> progress)
    {
        if (cancellationToken.IsCancellationRequested) return default;
        progress("Read word content", 0.2);
        using var stream = new MemoryStream(fileData);
        stream.Position = 0;
        Parser parser = new Parser(stream);

        progress("Read word content", 0.3);
        using TextReader reader = parser.GetText();
        return await reader.ReadToEndAsync();
    }

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult(text);
    }
}