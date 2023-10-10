using KnowledgeMediaImporter.Contracts;
using UglyToad.PdfPig;

namespace KnowledgeMediaImporter.Services;

public class PdfImportService : IImportService
{
    public bool CanHandle(string contentType)
    {
        return contentType == "application/pdf";
    }

    public Task<string> GetKnowledgeTextAsync(byte[] fileData, CancellationToken cancellationToken, Action<string, double> progress)
    {
        if (cancellationToken.IsCancellationRequested) return default;

        progress("Read pdf pages", 0.2);

        using var memoryStream = new MemoryStream(fileData);
        using var pdf = PdfDocument.Open(memoryStream);
        var textBuilder = new System.Text.StringBuilder();

        for (var page = 1; page <= pdf.NumberOfPages; page++)
        {
            var currentPage = pdf.GetPage(page);
            textBuilder.AppendLine(currentPage.Text);
        }
        progress("Read pdf content", 0.3);

        return Task.FromResult(textBuilder.ToString());
    }

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult(text);
    }
}