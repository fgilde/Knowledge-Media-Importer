using KnowledgeMedia.Core;
using Nextended.Core;

namespace KnowledgeMediaImporter.Data;


public class VideoImportService: IImportService
{
    public bool CanHandle(string contentType)
    {
        return MimeType.Matches(contentType, MimeType.VideoTypes);
    }

    public async Task<string> CreateKnowledgeTextAsync(byte[] fileData)
    {
        var anna = new VideoAnalyzer();
        await anna.UploadVideoAsync(fileData);

        return "...";
    }
}