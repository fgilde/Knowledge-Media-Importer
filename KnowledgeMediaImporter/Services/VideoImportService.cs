using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Nextended.Core;

namespace KnowledgeMediaImporter.Services;


public class VideoImportService : IImportService, IServiceSettingsValidation
{
    private string _videoId;
    private readonly VideoAnalyzer _analyzer;
    public bool CanHandle(string contentType)
    {
        return MimeType.Matches(contentType, MimeType.VideoTypes);
    }

    public VideoImportService(VideoAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public async Task<string> GetKnowledgeTextAsync(byte[] fileData, KnowledgeTargetSettings targetSettings, CancellationToken cancellationToken, IProgressUpdate progress)
    {
        var result = await _analyzer.UploadVideoAsync(fileData, progress, cancellationToken);
        _videoId = result.VideoId;
        return result.Transcript;
    }

    public async Task<string> AfterPrepareAsync(string text, CancellationToken cancellationToken)
    {
        return text;
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

    public Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings)
    {
        return _analyzer.ValidateServiceSettingsAsync(serviceSettings);
    }
}