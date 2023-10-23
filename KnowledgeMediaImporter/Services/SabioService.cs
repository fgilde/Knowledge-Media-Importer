using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Extensions;
using KnowledgeMediaImporter.Model;
using Microsoft.Extensions.Options;
using SABIO.ClientApi.Core;
using SABIO.ClientApi.Extensions;
using SABIO.ClientApi.Responses;
using SABIO.ClientApi.Responses.Types;
using File = SABIO.ClientApi.Responses.Types.File;
using Group = SABIO.ClientApi.Responses.Group;

namespace KnowledgeMediaImporter.Services
{
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

        public async Task CreateArticleAsync(CreateArticleOptions options)
        {
            options.Progress.Start();
            if (options.IsCancelled) return;
            await EnsureLoggedIn();
            options.Progress.Update("Connecting to knowledge", 10);

            User user = await _client.Apis.Authentication.GetCurrentUserAsync();
            var node = await EnsureTreeNodeStructure(options, await _client.Apis.Tree.FindNodeAsync(options.TargetSettings.TargetTreeNodeId), user);

            File? fileToAttach = null;
            if (options.TargetSettings.AttachFileToText)
            {
                fileToAttach = await HandleFileUpload(options, user, node);
            }
            
            await CreateArticle(options, node, user, fileToAttach);
        }


        private async Task<File?> HandleFileUpload(CreateArticleOptions options,  User user, TreeNode node)
        {
            if (await _client.Apis.FileManagement.CanWorkAsync())
            {
                options.Progress.Update("Uploading file", 30);
                return await UploadFile(options, user, node.Branches);
            }

            options.Progress.WriteLog("Upload skipped. FileManagement is not enabled");
            return null;
        }

        private async Task<File> UploadFile(CreateArticleOptions options, User user, Branch[] branches)
        {
            var file = options.File;
            string parentFolderId = "root";
            if (options.TargetSettings.CreateFileStructureFromPath && !string.IsNullOrEmpty(file.Path) && file.Path != "/")
            {
                var folders = await _client.Apis.FileManagement.CreateFolderStructureAsync(file.Path);
                parentFolderId = folders.LastOrDefault()?.Id ?? parentFolderId;
            }
            var toUpload = new File
            {
                Title = file.FileName,
                ParentFolderId = parentFolderId,
                Filename = file.FileName,
                MimeType = file.ContentType,
                Owner = user,
                OwnerGroup = options.TargetSettings.Group,
                TargetGroups = (await _client.Apis.Texts.GetGroupsAsync(branches)).Data.Result
            };

            var uploadResponse = await _client.Apis.FileManagement.CreateFileAsync(toUpload.ToUploadableFile(file.Data));
            if (uploadResponse.Success)
                options.Progress.Update("Successfully uploaded file", 50);
            else
                options.Progress.WriteLog("Upload failed");
            return uploadResponse.Data.Result;
        }
        
        private async Task<TreeNode> EnsureTreeNodeStructure(CreateArticleOptions options, TreeNode node, User user)
        {
            var file = options.File;
            if (options.TargetSettings.CreateTreeNodeStructureFromPath && !string.IsNullOrEmpty(file.Path) && file.Path != "/")
            {
                foreach (var segment in file.Path.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)))
                    node = node?.Children?.FirstOrDefault(n => n.Title == segment) ?? await CreateNodeAsync(options.Progress, node, node.Branches, segment, user, options.TargetSettings.Group);
            }

            return node;
        }

        private async Task CreateArticle(CreateArticleOptions options, TreeNode node, User user, File fileToAttach)
        {
            var textToCreate = new Text
            {
                Title = options.Title,
                Paths = new[] { node }.ToPathsArray(),
                Branches = node.Branches,
                Fragments = new[]
                {
                    new Fragment {
                        Content = options.Content,
                        Branches = node.Branches,
                    }
                },
                CreatedBy = user,
                Group = options.TargetSettings.Group
            };

            options.Progress.Update("Create Article", 70);
            if (options.IsCancelled) return;

            var created = await _client.Apis.Texts.CreateAsync(textToCreate);

            if (created?.Success == true)
            {
                options.Progress.Update("Article created successfully", 90);
                options.Progress.WriteLog($"{_knowledge.Url.Replace("sabio-web/services", "")}sabio5/#!/search/text/_id/{created?.Data?.Result?.Id}");
                options.Progress.Done("Successfully created knowledge article");
            }
            else
            {
                options.Progress.Failed("Failed to create Article");
            }
        }

        private async Task<TreeNode> CreateNodeAsync(IProgressUpdate log, TreeNode parentNode, Branch[] branches, string title, User user, Group group)
        {
            try
            {
                var res = await _client.Apis.Tree.CreateNodeAsync(new TreeNode { Title = title, Group = group, CreatedBy = user, Branches = branches }, parentNode);
                if (res.Success)
                {
                    //await Task.Delay(1000); // wait for the node to be created
                    return await _client.Apis.Tree.FindNodeAsync(res.Data.Result.Id);
                }
            }
            catch(Exception e)
            {
                log.WriteLog(e.Message);
            }

            return parentNode;
        }

        public async Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings)
        {
            if (serviceSettings?.Knowledge is null)
                return ServiceValidationResult.Fail("Settings are null");

            return await IsValidLogin(serviceSettings);
        }

        private async Task<ServiceValidationResult> IsValidLogin(ServiceSettings serviceSettings)
        {
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

            return client.IsLoggedIn ? ServiceValidationResult.Success : ServiceValidationResult.Fail("Invalid knowledge settings");
        }
    }
}