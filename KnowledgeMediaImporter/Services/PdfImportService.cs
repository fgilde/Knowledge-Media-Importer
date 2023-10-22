using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using UglyToad.PdfPig;

namespace KnowledgeMediaImporter.Services;

public class PdfImportService : IImportService
{
    public bool CanHandle(string contentType)
    {
        return contentType == "application/pdf";
    }

    public Task<string> GetKnowledgeTextAsync(byte[] fileData, CancellationToken cancellationToken, IProgressUpdate progress)
    {
        progress.Start();
        if (cancellationToken.IsCancellationRequested) return default;

        progress.Update("Read pdf pages", 10);

        using var memoryStream = new MemoryStream(fileData);
        using var pdf = PdfDocument.Open(memoryStream);
        var textBuilder = new System.Text.StringBuilder();

        for (var page = 1; page <= pdf.NumberOfPages; page++)
        {
            var currentPage = pdf.GetPage(page);
            textBuilder.AppendLine(currentPage.Text);
        }
        progress.Update("Read pdf content", 90);
        progress.Done("Successfully read pdf content");
        return Task.FromResult(textBuilder.ToString());
    }

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult(text);
    }
}