using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using Microsoft.Extensions.Options;
using Nextended.Core;

namespace KnowledgeMediaImporter.Services;


public class VideoImportService : IImportService
{
    private readonly IOptions<ServiceSettings> _serviceSettings;
    private string _videoId;
    private VideoAnalyzer _analyzer;
    public bool CanHandle(string contentType)
    {
        return MimeType.Matches(contentType, MimeType.VideoTypes);
    }

    public VideoImportService(IOptions<ServiceSettings> serviceSettings)
    {
        _serviceSettings = serviceSettings;
    }

    public async Task<string> GetKnowledgeTextAsync(byte[] fileData, CancellationToken cancellationToken, Action<string, double> progress)
    {
        _analyzer = new VideoAnalyzer(_serviceSettings);
        var result = await _analyzer.UploadVideoAsync(fileData, progress, cancellationToken);
        _videoId = result.VideoId;
        return result.Transcript;
    }

    public async Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return default;

        var insightsWidgetUrlAsync = await _analyzer.GetInsightsWidgetUrlAsync(_videoId);
        var playerUri = await _analyzer.GetPlayerWidgetUrlAsync(_videoId);

        var iframe = $"<details><summary>Show Video</summary>" +
                          $"<iframe src=\"{insightsWidgetUrlAsync}\" style=\"width:100%; height:350px; border:none;\" />" +
                          $"<iframe src=\"{playerUri}\" style=\"width:100%; height:450px; border:none;\" />" +
                     $"</details>";

        text += iframe;
        return text;
    }
}