using KnowledgeMedia.Core;
using Nextended.Core;

namespace KnowledgeMediaImporter.Data;


public class VideoImportService: IImportService
{
    private string _videoId;
    private VideoAnalyzer _analyzer;
    public bool CanHandle(string contentType)
    {
        return MimeType.Matches(contentType, MimeType.VideoTypes);
    }

    public async Task<string> GetKnowledgeTextAsync(byte[] fileData, Action<string, double> progress)
    {
        _analyzer= new VideoAnalyzer();
        var result = await _analyzer.UploadVideoAsync(fileData, progress);
        _videoId = result.VideoId;
        return result.Transcript;
    }

    public async Task<string> AfterPrepareAsync(string text)
    {
       var url = await _analyzer.GetInsightsWidgetUrlAsync(_videoId);
       var iframe = $"<iframe src=\"{url}\" style=\"width:100%; height:450px; border:none;\" />";
       text += iframe;
       return text;
    }
}