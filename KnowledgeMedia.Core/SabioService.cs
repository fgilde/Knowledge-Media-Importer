using KnowledgeMedia.Core.Configuration;
using Microsoft.Extensions.Options;
using SABIO.ClientApi.Core;
using SABIO.ClientApi.Core.Api;
using SABIO.ClientApi.Extensions;
using SABIO.ClientApi.Responses;
using SABIO.ClientApi.Responses.Types;

namespace KnowledgeMedia.Core;

public class SabioService
{
    private SabioClient? client;
    private readonly KnowledgeSettings _knowledge;

    public SabioService(IOptions<ServiceSettings> serviceSettings)
    {
        
        _knowledge = serviceSettings.Value.Knowledge;
    }

    public async Task LoginAsync()
    {
        if (client == null)
        {
            client = new SabioClient(_knowledge.Url, _knowledge.Realm);
            if(!string.IsNullOrEmpty(_knowledge.ApiKey))
                await client.Api<AuthenticationApi>().LoginAsync(_knowledge.ApiKey);
            else
                await client.Api<AuthenticationApi>().LoginAsync(_knowledge.User, _knowledge.Password);
        }
    }

    public async Task<string> CreateArticleAsync(string title, string text, CancellationToken cancellationToken, Action<string, double> progress)
    {

        if (cancellationToken.IsCancellationRequested) return string.Empty;
        await LoginAsync();
        
        var tree = client.Api<TreeApi>().TreeAsync().Result;
        progress("Connecting to knowledge", 0.8);

        var nodes = new[]
        {
            tree.Data.Result.Children.First().Children.First().Children.First(),
            tree.Data.Result.Children[2].Children.First().Children.First()
        };
        User user = await client.Apis.Authentication.GetCurrentUserAsync();
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

        var created = await client.Apis.Texts.CreateAsync(textToCreate);
        if (!created?.Success ?? false)
        {
            
        }
        progress(created?.Success == true ? "Article created successfully" : "Failed to create Article", 0.93);
        return $"https://maestro-anna-knowledge.labs.swops.cloud/sabio5/#!/search/text/_id/{created?.Data?.Result?.Id}";
    }
}