using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Microsoft.Extensions.Options;
using SABIO.ClientApi.Core;
using SABIO.ClientApi.Core.Api;
using SABIO.ClientApi.Extensions;
using SABIO.ClientApi.Responses;
using SABIO.ClientApi.Responses.Types;

namespace KnowledgeMediaImporter.Services;

public class SabioService : IServiceSettingsValidation
{
    private SabioClient? _client;
    private readonly KnowledgeSettings _knowledge;

    public SabioService(IOptionsSnapshot<ServiceSettings> serviceSettings)
    {
        _knowledge = serviceSettings.Value.Knowledge;
    }

    public async Task LoginAsync()
    {
        _client ??= await CreateClientAndLoginAsync(_knowledge);
    }

    private async Task<SabioClient> CreateClientAndLoginAsync(KnowledgeSettings settings)
    {
        var client = new SabioClient(settings.Url, settings.Realm);
        if (!string.IsNullOrEmpty(settings.ApiKey))
            await client.Api<AuthenticationApi>().LoginAsync(settings.ApiKey);
        else
            await client.Api<AuthenticationApi>().LoginAsync(settings.User, settings.Password);
        return client;
    }

    public async Task<string> CreateArticleAsync(string title, string text, CancellationToken cancellationToken, Action<string, double> progress)
    {
        if (cancellationToken.IsCancellationRequested) return string.Empty;
        await LoginAsync();
        
        var tree = _client.Api<TreeApi>().TreeAsync().Result;
        progress("Connecting to knowledge", 0.8);

        var nodes = new[]
        {
            tree.Data.Result.Children.First().Children.First().Children.First(),
            tree.Data.Result.Children[2].Children.First().Children.First()
        };
        User user = await _client.Apis.Authentication.GetCurrentUserAsync();
        Text textToCreate = new Text
        {
            Title = title,
            Paths = nodes.ToPathsArray(),
            Branches = nodes.GetUniqueBranches().ToArray(),
            Fragments = new[]
            {
                new Fragment {
                    Content = text,
                    Branches = nodes.GetUniqueBranches().ToArray(),
                }
            },
            CreatedBy = user,
            Group = user.Groups.First()
        };
        progress("Create Article", 0.9);
        if (cancellationToken.IsCancellationRequested) return string.Empty;

        var created = await _client.Apis.Texts.CreateAsync(textToCreate);
        if (!created?.Success ?? false)
        {
            
        }
        progress(created?.Success == true ? "Article created successfully" : "Failed to create Article", 0.93);
        return $"https://maestro-anna-knowledge.labs.swops.cloud/sabio5/#!/search/text/_id/{created?.Data?.Result?.Id}";
    }

    public async Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings)
    {
        if (serviceSettings?.Knowledge is null)
            return ServiceValidationResult.Fail("Settings are null");
        SabioClient client;
        try
        {
            client = await CreateClientAndLoginAsync(serviceSettings.Knowledge);
        }
        catch (Exception e)
        {
            return ServiceValidationResult.Fail(e.Message);
        }
        return client.IsLoggedIn
            ? ServiceValidationResult.Success
            : ServiceValidationResult.Fail("Invalid knowledge settings");
    }
}