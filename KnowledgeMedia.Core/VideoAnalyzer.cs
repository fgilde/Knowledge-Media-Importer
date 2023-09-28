using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace KnowledgeMedia.Core;

public class VideoAnalyzer
{
    private const string ApiUrl = "https://api.videoindexer.ai";
    private const string AccountId = "4de3fc17-7d80-454d-88dd-6db6ef8f422c";
    private const string Location = "trial"; // Consider fetching from config
    private const string ApiKey = "fe29425dfdce47b79e7b2613e33d33f3";

    static VideoAnalyzer()
    {
        System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
    }

    public async Task<(string VideoId, string Transcript)> UploadVideoAsync(byte[] data, Action<string, double> progress, CancellationToken cancellationToken)
    {
        using (var client = CreateHttpClient())
        {
            progress("Create connection", 0.1);
            var accountAccessToken = await GetAccountAccessTokenAsync(client);
            if (cancellationToken.IsCancellationRequested) return default;

            progress("Upload video", 0.2);

           // var videoId = await UploadVideoDataAsync(client, data, accountAccessToken);
           progress("Analyzing video", 0.3);
           if (cancellationToken.IsCancellationRequested) return default;

            // await WaitForVideoProcessingToCompleteAsync(client, videoId, accountAccessToken);
            if (cancellationToken.IsCancellationRequested) return default;

            var videoId = "7346e6835d";
            progress("Read video info and generate transcription", 0.4);

            var content = await ReadVideoInfoAsync(client, videoId, accountAccessToken);
            var text = string.Join(Environment.NewLine, content.videos.First().insights.transcript.Select(t => t.text));
            
            if (cancellationToken.IsCancellationRequested) return default;

            // You can add the other API calls (like fetching widget URLs) similarly if needed
            return (videoId,text);
            //return videoId;
        }
    }

    private static async Task<IndexResult?> ReadVideoInfoAsync(HttpClient client, string videoId, string accountAccessToken)
    {
        var url = $"https://api.videoindexer.ai/{Location}/Accounts/{AccountId}/Videos/{videoId}/Index?accessToken={accountAccessToken}";
        var result = await client.GetAsync(url);
        var content = await result.Content.ReadFromJsonAsync<IndexResult>();
        return content;
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);

        return client;
    }

    private async Task<string> GetAccountAccessTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync($"{ApiUrl}/auth/{Location}/Accounts/{AccountId}/AccessToken?allowEdit=true");
        var token = await response.Content.ReadAsStringAsync();

        return token.Replace("\"", "");
    }

    private async Task<string> UploadVideoDataAsync(HttpClient client, byte[] data, string accountAccessToken)
    {
        client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");

        var content = new MultipartFormDataContent();
        var byteArrayContent = new ByteArrayContent(data);
        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        content.Add(byteArrayContent, "file", $"{Guid.NewGuid().ToString()}.mp4");

        var response = await client.PostAsync($"{ApiUrl}/{Location}/Accounts/{AccountId}/Videos?accessToken={accountAccessToken}&name=some_name&description=some_description&privacy=private&partition=some_partition", content);
        var uploadResult = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<dynamic>(uploadResult)["id"];
    }
    
    public async Task<Uri> GetInsightsWidgetUrlAsync(string videoId, HttpClient? client = null)
    {
        client ??= CreateHttpClient();
        var videoAccessToken = await GetVideoAccessTokenAsync(client, videoId);
        var insightsWidgetRequestResult = await client.GetAsync($"{ApiUrl}/{Location}/Accounts/{AccountId}/Videos/{videoId}/InsightsWidget?accessToken={videoAccessToken}&widgetType=Keywords&allowEdit=true");
        return insightsWidgetRequestResult.Headers.Location;
    }

    public async Task<Uri> GetPlayerWidgetUrlAsync(string videoId, HttpClient? client = null)
    {
        client ??= CreateHttpClient();
        var videoAccessToken = await GetVideoAccessTokenAsync(client, videoId);
        var playerWidgetRequestResult = await client.GetAsync($"{ApiUrl}/{Location}/Accounts/{AccountId}/Videos/{videoId}/PlayerWidget?accessToken={videoAccessToken}");
        return playerWidgetRequestResult.Headers.Location;
    }
    
    private async Task<string> GetVideoAccessTokenAsync(HttpClient client, string videoId)
    {
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);
        var videoTokenRequestResult = await client.GetAsync($"{ApiUrl}/auth/{Location}/Accounts/{AccountId}/Videos/{videoId}/AccessToken?allowEdit=true");
        client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
    
        var videoAccessToken = await videoTokenRequestResult.Content.ReadAsStringAsync();
        return videoAccessToken.Replace("\"", "");
    }


    private async Task WaitForVideoProcessingToCompleteAsync(HttpClient client, string videoId, string accountAccessToken)
    {
        while (true)
        {
            await Task.Delay(10000);

            var videoGetIndexResponse = await client.GetAsync($"{ApiUrl}/{Location}/Accounts/{AccountId}/Videos/{videoId}/Index?accessToken={accountAccessToken}&language=English");
            var videoGetIndexResult = await videoGetIndexResponse.Content.ReadAsStringAsync();
            var processingState = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult)["state"];

            if (processingState != "Uploaded" && processingState != "Processing")
            {
                break;
            }
        }
    }
}
