using KnowledgeMediaImporter.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace KnowledgeMediaImporter.Services;

public class GptService
{
    private OpenAIClient Api;
    private readonly ChatGptSettings _gptSettings;

    public GptService(IOptions<ServiceSettings> serviceSettings)
    {
        _gptSettings = serviceSettings.Value.ChatGpt;
        Api = new OpenAIClient(_gptSettings.ApiKey);
    }

    public async Task<(string Title, string Content)> PrepareContentAsync(string text, CancellationToken cancellationToken, Action<string, double> progress)
    {
        //return ("TEST", text);
        progress("Prepare HTML Content", 0.5);
        if (cancellationToken.IsCancellationRequested) return default;
        
        var model = new OpenAI.Models.Model(_gptSettings.Model);

        var contentString = await GenerateArticleContentAsync(text, model);
        if (cancellationToken.IsCancellationRequested) return default;
        progress("Generate title", 0.7);
        var titleString = await GenerateArticleTitleAsync(text, model);
        return (titleString, contentString);
    }

    private async Task<string> GenerateArticleTitleAsync(string text, OpenAI.Models.Model model)
    {
        var prompts = new[]
        {
            new ChatPrompt("system", "I have a video transcription and want just a short title for it with max of 30 characters without quotes."),
            new ChatPrompt("user", text)
        }.ToList();
        ChatRequest chatRequest = new ChatRequest(prompts, model);
        var response = await Api.ChatEndpoint.GetCompletionAsync(chatRequest);
        return response.FirstChoice.ToString().Replace("\"", "");
    }

    private async Task<string> GenerateArticleContentAsync(string text, OpenAI.Models.Model model)
    {
        var prompts = new[]
        {
            new ChatPrompt("user", "I have a transcription and want you to summarize the content into an article for a knowledge base. " +
                                     "Please keep it short and leave out irrelevant information and avoid colloquial language and provide a nice html if possible with bullet points, subheader, tables etc" +
                                     "but please without the surrounding body tags." +
                                     "Please also leave out any intros and outros that do not focus on the main topic as well as references to other content"),

            new ChatPrompt("user", text)
        }.ToList();
        ChatRequest chatRequest = new ChatRequest(prompts, model);
        var response = await Api.ChatEndpoint.GetCompletionAsync(chatRequest);
        return response.FirstChoice.ToString();
    }
}