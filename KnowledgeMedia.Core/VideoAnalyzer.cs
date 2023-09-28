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

    public async Task<string> UploadVideoAsync(byte[] data)
    {
        using (var client = CreateHttpClient())
        {
            var accountAccessToken = await GetAccountAccessTokenAsync(client);

           // var videoId = await UploadVideoDataAsync(client, data, accountAccessToken);

            // await WaitForVideoProcessingToCompleteAsync(client, videoId, accountAccessToken);

            var videoId = "7346e6835d";
            var url = $"https://api.videoindexer.ai/{Location}/Accounts/{AccountId}/Videos/{videoId}/Index?accessToken={accountAccessToken}";
            var result = await client.GetAsync(url);
            var content = await result.Content.ReadFromJsonAsync<IndexResult>();
            var text = string.Join(Environment.NewLine, content.videos.First().insights.transcript.Select(t => t.text));
            var playerUrl = await GetInsightsWidgetUrlAsync(client, videoId, accountAccessToken);
            // You can add the other API calls (like fetching widget URLs) similarly if needed
            return videoId;
        }
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
    
    public async Task<Uri> GetInsightsWidgetUrlAsync(HttpClient client, string videoId, string accessToken)
    {
        var insightsWidgetRequestResult = await client.GetAsync($"{ApiUrl}/{Location}/Accounts/{AccountId}/Videos/{videoId}/InsightsWidget?accessToken={accessToken}&widgetType=Keywords&allowEdit=true");
        return insightsWidgetRequestResult.Headers.Location;
    }

    public async Task<Uri> GetPlayerWidgetUrlAsync(HttpClient client, string videoId, string accessToken)
    {
        var playerWidgetRequestResult = await client.GetAsync($"{ApiUrl}/{Location}/Accounts/{AccountId}/Videos/{videoId}/PlayerWidget?accessToken={accessToken}");
        return playerWidgetRequestResult.Headers.Location;
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
