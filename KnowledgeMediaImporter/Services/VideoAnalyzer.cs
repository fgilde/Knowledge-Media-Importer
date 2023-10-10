using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KnowledgeMediaImporter.Services;

public class VideoAnalyzer
{
    private VideoIndexerSettings _settings;
    private HttpClient _httpClient;

    static VideoAnalyzer()
    {
        System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
    }

    public VideoAnalyzer(IOptionsSnapshot<ServiceSettings> serviceSettings)
    {
        _settings = serviceSettings.Value.VideoIndexer;
        _httpClient = new HttpClient(); // TODO: Inject
        ConfigureHttpClient();
    }

    public async Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings)
    {
        var result = new ServiceValidationResult();

        if (serviceSettings is null)
        {
            result.Errors.Add("ServiceSettings is null.");
            return result;
        }

        if (serviceSettings.VideoIndexer is null)
        {
            result.Errors.Add("VideoIndexer settings are null.");
            return result;
        }

        var videoIndexerSettings = serviceSettings.VideoIndexer;
        if (string.IsNullOrWhiteSpace(videoIndexerSettings.Url))
            result.Errors.Add("VideoIndexer URL is not configured.");

        if (string.IsNullOrWhiteSpace(videoIndexerSettings.ApiKey))
            result.Errors.Add("VideoIndexer ApiKey is not configured.");

        if (string.IsNullOrWhiteSpace(videoIndexerSettings.AccountId))
            result.Errors.Add("VideoIndexer AccountId is not configured.");

        if (string.IsNullOrWhiteSpace(videoIndexerSettings.Location))
            result.Errors.Add("VideoIndexer Location is not configured.");
        

        // Live call validation
        _settings = serviceSettings.VideoIndexer;
        _httpClient = new HttpClient();
        ConfigureHttpClient();
        
        try
        {
            var accountAccessToken = await GetAccountAccessTokenAsync();
            if (string.IsNullOrEmpty(accountAccessToken))
            {
                result.Errors.Add("Failed to retrieve a valid account access token. Please check API key, account ID, and location.");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Live validation failed. Reason: {ex.Message}");
        }

        return result;
    }

    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
        _httpClient.BaseAddress = new Uri(_settings.Url);
    }

    public async Task<Uri> GetInsightsWidgetUrlAsync(string videoId)
    {
        var videoAccessToken = await GetVideoAccessTokenAsync(videoId);
        var insightsWidgetRequestResult = await _httpClient.GetAsync($"{_settings.Url}/{_settings.Location}/Accounts/{_settings.AccountId}/Videos/{videoId}/InsightsWidget?accessToken={videoAccessToken}&widgetType=Keywords&allowEdit=true");
        return insightsWidgetRequestResult.Headers.Location;
    }

    public async Task<Uri> GetPlayerWidgetUrlAsync(string videoId)
    {
        var videoAccessToken = await GetVideoAccessTokenAsync(videoId);
        var playerWidgetRequestResult = await _httpClient.GetAsync($"{_settings.Url}/{_settings.Location}/Accounts/{_settings.AccountId}/Videos/{videoId}/PlayerWidget?accessToken={videoAccessToken}");
        return playerWidgetRequestResult.Headers.Location;
    }

    public async Task<(string VideoId, string Transcript)> UploadVideoAsync(byte[] data, Action<string, double> progress, CancellationToken cancellationToken)
    {
        progress("Create connection", 0.1);
        var accountAccessToken = await GetAccountAccessTokenAsync();
        if (cancellationToken.IsCancellationRequested) return default;

        var videoId = "7346e6835d";
        //progress("Upload video", 0.2);
        //var videoId = await UploadVideoDataAsync(data, accountAccessToken, cancellationToken);
        //if (cancellationToken.IsCancellationRequested) return default;

        //progress("Analyzing video", 0.3);
        //await WaitForVideoProcessingToCompleteAsync(videoId, accountAccessToken, cancellationToken);
        //if (cancellationToken.IsCancellationRequested) return default;

        progress("Read video info and generate transcription", 0.4);
        var content = await ReadVideoInfoAsync(videoId, accountAccessToken);
        var text = string.Join(Environment.NewLine, content.videos.First().insights.transcript.Select(t => t.text));
        if (cancellationToken.IsCancellationRequested) return default;

        return (videoId, text);
    }

    private async Task<string> GetAccountAccessTokenAsync() =>
        await GetAccessTokenAsync($"/auth/{_settings.Location}/Accounts/{_settings.AccountId}/AccessToken?allowEdit=true");

    private async Task<string> GetVideoAccessTokenAsync(string videoId) =>
        await GetAccessTokenAsync($"/auth/{_settings.Location}/Accounts/{_settings.AccountId}/Videos/{videoId}/AccessToken?allowEdit=true");

    private async Task<string> GetAccessTokenAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
            throw new Exception((await response.Content.ReadFromJsonAsync<IndexerError>()).Message);
        var token = await response.Content.ReadAsStringAsync();
        return token.Replace("\"", "");
    }

    private async Task<string> UploadVideoDataAsync(byte[] data, string accountAccessToken, CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(data) { Headers = { ContentType = new MediaTypeHeaderValue("video/mp4") } }, "file", $"{Guid.NewGuid()}.mp4" }
        };

        var response = await _httpClient.PostAsync(BuildVideoEndpoint(_settings.AccountId, accountAccessToken), content, cancellationToken);
        var uploadResult = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<dynamic>(uploadResult)["id"];
    }

    private async Task<IndexResult?> ReadVideoInfoAsync(string videoId, string accountAccessToken)
    {
        var url = BuildVideoInfoEndpoint(videoId, accountAccessToken);
        var result = await _httpClient.GetAsync(url);
        return await result.Content.ReadFromJsonAsync<IndexResult>();
    }

    private async Task WaitForVideoProcessingToCompleteAsync(string videoId, string accountAccessToken, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(10000);

            var videoGetIndexResponse = await _httpClient.GetAsync(BuildVideoProcessingStatusEndpoint(videoId, accountAccessToken));
            var videoGetIndexResult = await videoGetIndexResponse.Content.ReadAsStringAsync();
            var processingState = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult)["state"];

            if (processingState != "Uploaded" && processingState != "Processing") break;
        }
    }

    private string BuildVideoEndpoint(string accountId, string accessToken) =>
        $"{_settings.Location}/Accounts/{accountId}/Videos?accessToken={accessToken}&name=some_name&description=some_description&privacy=private&partition=some_partition";

    private string BuildVideoInfoEndpoint(string videoId, string accessToken) =>
        $"{_settings.Location}/Accounts/{_settings.AccountId}/Videos/{videoId}/Index?accessToken={accessToken}";

    private string BuildVideoProcessingStatusEndpoint(string videoId, string accessToken) =>
        $"{_settings.Location}/Accounts/{_settings.AccountId}/Videos/{videoId}/Index?accessToken={accessToken}&language=English";
}
