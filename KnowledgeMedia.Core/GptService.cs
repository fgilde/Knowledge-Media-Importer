using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

namespace KnowledgeMedia.Core;

public class GptService
{
    private OpenAIClient Api;
    
    public GptService()
    {
        Api = new OpenAIClient("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");
    }

    public async Task<(string Title, string Content)> PrepareContentAsync(string text, Action<string, double> progress)
    {
        progress("Prepare HTML Content", 0.5);
        var contentString = await GenerateArticleContentAsync(text, Model.GPT4);
        progress("Generate title", 0.7);
        var titleString = await GenerateArticleTitleAsync(text, Model.GPT4);
        return (titleString, contentString);
    }

    private async Task<string> GenerateArticleTitleAsync(string text, Model model)
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
    
    private async Task<string> GenerateArticleContentAsync(string text, Model model)
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