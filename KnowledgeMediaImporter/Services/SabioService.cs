using Aspose.Pdf;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Extensions;
using KnowledgeMediaImporter.Model;
using Microsoft.Extensions.Options;
using Nextended.Core.Contracts;
using SABIO.ClientApi.Core;
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

    public async Task CreateArticleAsync(string title, string text, IUploadableFile file, KnowledgeTargetSettings targetSettings, CancellationToken cancellationToken, IProgressUpdate progress)
    {
        progress.Start();
        if (cancellationToken.IsCancellationRequested) return;
        await EnsureLoggedIn();
        
        progress.Update("Connecting to knowledge", 10);
        User user = await _client.Apis.Authentication.GetCurrentUserAsync();
        

        var node = await _client.Apis.Tree.FindNodeAsync(targetSettings.TargetTreeNodeId);
        var branches = node.Branches.Where(b => targetSettings.TargetBranches.Any(tb => tb.Id == b.Id)).ToArray();
        var group = targetSettings.Group;
        progress.Update("Prepare structure and nodes", 20);

        if (targetSettings.AttachFileToText)
        {
            if (await _client.Apis.FileManagement.CanWorkAsync())
            {
                progress.Update("Uploading file", 30);
                string parentFolderId = "root";
                if (targetSettings.CreateFileStructureFromPath && !string.IsNullOrEmpty(file.Path) && file.Path != "/")
                {
                    var folders = await _client.Apis.FileManagement.CreateFolderStructureAsync(file.Path);
                    parentFolderId = folders.LastOrDefault()?.Id ?? parentFolderId;
                }

                var toUpload = new SABIO.ClientApi.Responses.Types.File
                {
                    Title = file.FileName,
                    ParentFolderId = parentFolderId,
                    Filename = file.FileName,
                    MimeType = file.ContentType,
                    Owner = await _client.Apis.Authentication.GetCurrentUserAsync(),
                    OwnerGroup = group,
                    TargetGroups = (await _client.Apis.Texts.GetGroupsAsync(branches)).Data.Result
                };
                var uploadResponse = await _client.Apis.FileManagement.CreateFileAsync(toUpload.ToUploadableFile(file.Data));
                if(uploadResponse.Success)
                    progress.Update("Successfully uploaded file", 50);
                else
                    progress.WriteLog("Upload failed");
            }
            else
            {
                // TODO: Maybe use document storage instead
                progress.WriteLog("Upload skipped. FileManagement is not enabled");
            }
        }

        
        if (targetSettings.CreateTreeNodeStructureFromPath && !string.IsNullOrEmpty(file.Path) && file.Path != "/")
        {
            foreach (var segment in file.Path.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)))
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
        progress.Update("Create Article", 70);
        if (cancellationToken.IsCancellationRequested) return;

        var created = await _client.Apis.Texts.CreateAsync(textToCreate);
 
        if (created?.Success == true)
        {
            progress.Update("Article created successfully", 90);
            progress.WriteLog($"{_knowledge.Url.Replace("sabio-web/services", "")}sabio5/#!/search/text/_id/{created?.Data?.Result?.Id}");
            progress.Done("Successfully created knowledge article");
        }
        else
        {
            progress.Failed("Failed to create Article");
        }
    }


    private async Task<TreeNode> CreateNodeAsync(TreeNode parentNode, Branch[] branches, string title, User user,
        Group group)
    {
        try
        {
            var res = await _client.Apis.Tree.CreateNodeAsync(new TreeNode {Title = title, Group = group, CreatedBy = user, Branches = branches }, parentNode);
            if (res.Success)
                return await _client.Apis.Tree.FindNodeAsync(res.Data.Result.Id);
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