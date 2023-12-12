using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace KnowledgeMediaImporter.Services;

public class GptService : IServiceSettingsValidation
{
    private readonly IConfiguration _configuration;
    private OpenAIClient Api;
    private readonly ChatGptSettings _gptSettings;

    public GptService(IOptionsSnapshot<ServiceSettings> serviceSettings, IConfiguration configuration)
    {
        _configuration = configuration;
        _gptSettings = serviceSettings.Value.ChatGpt;
        Api = new OpenAIClient(_gptSettings.ApiKey);
    }

    public async Task<(string Title, string Content)> PrepareContentAsync(string text, KnowledgeTargetSettings targetSettings, CancellationToken cancellationToken, IProgressUpdate progress)
    {
        progress.Start();
        progress.Update("Prepare HTML Content", 20);

        if (_configuration.GetValue<bool>("GptFake"))
        {
            await Task.Delay(1000, cancellationToken);
            progress.Update("Generate title", 70);
            await Task.Delay(2000, cancellationToken);
            progress.Done("Successfully summarized content");
            return ("TEST", text);
        }
        
        if (cancellationToken.IsCancellationRequested) return default;

        try
        {
            var model = new OpenAI.Models.Model(_gptSettings.Model);

            var contentString = await GenerateArticleContentAsync(text, targetSettings.TargetLanguage, model, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return default;
            progress.Update("Generate title", 70);
            var titleString = await GenerateArticleTitleAsync(text, targetSettings.TargetLanguage, model, cancellationToken);
            progress.Done("Successfully summarized content");
            return (titleString, contentString);
        }
        catch (Exception e)
        {
            progress.Failed(e.Message);
            return default;
        }
    }

    private async Task<string> GenerateArticleTitleAsync(string text, string language, OpenAI.Models.Model model, CancellationToken cancellation = default)
    {
        var prompts = new[]
        {
            new ChatPrompt("system", "I have a video transcription and want just a short title for it with max of 30 characters without quotes. Please ensure result is in " + language),
            new ChatPrompt("user", text)
        }.ToList();
        ChatRequest chatRequest = new ChatRequest(prompts, model);
        var response = await Api.ChatEndpoint.GetCompletionAsync(chatRequest, cancellation);
        return response.FirstChoice.ToString().Replace("\"", "");
    }

    private async Task<string> GenerateArticleContentAsync(string text, string language, OpenAI.Models.Model model, CancellationToken cancellation = default)
    {
        var prompts = new[]
        {
            new ChatPrompt("user", "I have a transcription and want you to summarize the content into an article for a knowledge base. " +
                                     "Please keep it short and leave out irrelevant information and avoid colloquial language and provide a nice html if possible with bullet points, subheader, tables etc" +
                                     "but please without the surrounding body tags." +
                                     "Please also leave out any intros and outros that do not focus on the main topic as well as references to other content" +
                                     "If the text is not in " + language + " then please translate it to " + language
                                     ),

            new ChatPrompt("user", text)
        }.ToList();
        ChatRequest chatRequest = new ChatRequest(prompts, model);
        var response = await Api.ChatEndpoint.GetCompletionAsync(chatRequest, cancellation);
        return response.FirstChoice.ToString();
    }

    public async Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings)
    {
        if (serviceSettings?.ChatGpt is null)
            return ServiceValidationResult.Fail("Settings are null");
        if (string.IsNullOrWhiteSpace(serviceSettings?.ChatGpt.ApiKey))
            return ServiceValidationResult.Fail("ApiKey empty or null");
        if (string.IsNullOrWhiteSpace(serviceSettings?.ChatGpt.Model))
            return ServiceValidationResult.Fail("No Model specified");

        if(_configuration.GetValue<bool>("Dummy") || _configuration.GetValue<bool>("GptFake"))
            return ServiceValidationResult.Success;
        
        var api = new OpenAIClient(serviceSettings.ChatGpt.ApiKey);
        try
        {
            var res = await api.ChatEndpoint.GetCompletionAsync(new ChatRequest(new[] { new ChatPrompt("user", "are you available") }, new OpenAI.Models.Model(serviceSettings.ChatGpt.Model)));
            return !string.IsNullOrEmpty(res?.FirstChoice?.ToString())
                ? ServiceValidationResult.Success
                : ServiceValidationResult.Fail("Please check api and model");
        }
        catch (Exception e)
        {
           return ServiceValidationResult.Fail(e.Message);
        }
        
    }
}