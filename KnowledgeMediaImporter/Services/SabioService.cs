using Aspose.Pdf;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Extensions;
using KnowledgeMediaImporter.Model;
using Microsoft.Extensions.Options;
using SABIO.ClientApi.Core;
using SABIO.ClientApi.Core.Api;
using SABIO.ClientApi.Extensions;
using SABIO.ClientApi.Responses;
using SABIO.ClientApi.Responses.Types;
using Group = SABIO.ClientApi.Responses.Group;

namespace KnowledgeMediaImporter.Services;

public class SabioService : IServiceSettingsValidation
{
    private readonly SabioClient _client;
    private readonly KnowledgeSettings _knowledge;

    public SabioService(SabioClient client, IOptionsSnapshot<ServiceSettings> settings)
    {
        _client = client;
        _knowledge = settings.Value.Knowledge;
    }

    private async Task EnsureLoggedIn()
    {
        if (!_client.IsLoggedIn)
            await _client.LoginAsync(_knowledge);
    }

    public async Task<string> CreateArticleAsync(string title, string text, string path, KnowledgeTargetSettings targetSettings, CancellationToken cancellationToken, Action<string, double> progress)
    {

        if (cancellationToken.IsCancellationRequested) return string.Empty;
        await EnsureLoggedIn();
        
        progress("Connecting to knowledge", 0.8);
        User user = await _client.Apis.Authentication.GetCurrentUserAsync();
        

        var node = await _client.Apis.Tree.FindNodeAsync(targetSettings.TargetTreeNodeId);
        var branches = node.Branches.Where(b => targetSettings.TargetBranches.Any(tb => tb.Id == b.Id)).ToArray();
        var group = targetSettings.Group;
        
        if (targetSettings.CreateTreeNodesFromStructurePath && !string.IsNullOrEmpty(path) && path != "/")
        {
            foreach (var segment in path.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)))
                node = node?.Children?.FirstOrDefault(n => n.Title == segment) ?? await CreateNodeAsync(node, branches, segment, user, group);
        }


        var nodes = new[] { node };

        Text textToCreate = new Text
        {
            Title = title,
            Paths = nodes.ToPathsArray(),
            Branches = branches,
            Fragments = new[]
            {
                new Fragment {
                    Content = text,
                    Branches = branches,
                }
            },
            CreatedBy = user,
            Group = group
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


    private async Task<TreeNode> CreateNodeAsync(TreeNode parentNode, Branch[] branches, string title, User user,
        Group group)
    {
        try
        {
            var res = await _client.Apis.Tree.CreateNodeAsync(new TreeNode {Title = title, Group = group, CreatedBy = user, Branches = branches }, parentNode);
            if (res.Success)
                return await _client.Apis.Tree.FindNodeAsync(res.Data.Result.Id);
            //return res.Data.Result;
        }
        catch (Exception e)
        { }

        return parentNode;
    }

    public async Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings)
    {
        if (serviceSettings?.Knowledge is null)
            return ServiceValidationResult.Fail("Settings are null");
        SabioClient client;
        try
        {
            client = new SabioClient(serviceSettings.Knowledge.Url, serviceSettings.Knowledge.Realm);
            await client.LoginAsync(serviceSettings.Knowledge);
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