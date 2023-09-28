using Aspose.Pdf;
using Aspose.Pdf.Text;
using Nextended.Core;
using UglyToad.PdfPig;

namespace KnowledgeMediaImporter.Data;

public class PdfImportService: IImportService
{
    public bool CanHandle(string contentType)
    {
        return contentType == "application/pdf";
    }

    public Task<string> GetKnowledgeTextAsync(byte[] fileData,CancellationToken cancellationToken, Action<string, double> progress)
    {
        if (cancellationToken.IsCancellationRequested) return default;

        using var memoryStream = new MemoryStream(fileData);
        using var pdf = PdfDocument.Open(memoryStream);
        var textBuilder = new System.Text.StringBuilder();

        for (var page = 1; page <= pdf.NumberOfPages; page++)
        {
            var currentPage = pdf.GetPage(page);
            textBuilder.AppendLine(currentPage.Text);
        }

        return Task.FromResult(textBuilder.ToString());
    }

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult(text);
    }
}