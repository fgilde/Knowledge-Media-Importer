using Aspose.Pdf;
using Aspose.Pdf.Text;
using Nextended.Core;

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

        using var stream = new MemoryStream(fileData);
        stream.Position = 0;
        Document pdfDocument = new Document(stream);
        TextAbsorber textAbsorber = new TextAbsorber();
        pdfDocument.Pages.Accept(textAbsorber);
        string extractedText = textAbsorber.Text;
        return Task.FromResult(extractedText);
    }

    public Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }
}