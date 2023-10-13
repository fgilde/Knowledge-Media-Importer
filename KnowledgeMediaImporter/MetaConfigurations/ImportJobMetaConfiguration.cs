using KnowledgeMediaImporter.Model;
using KnowledgeMediaImporter.Shared;
using MudBlazor;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Components.ObjectEdit;
using MudBlazor.Extensions.Components.ObjectEdit.Options;
using SABIO.ClientApi.Responses;

namespace KnowledgeMediaImporter.MetaConfigurations;

public class ImportJobMetaConfiguration : IObjectMetaConfiguration<ImportJobConfiguration>
{
    public Task ConfigureAsync(ObjectEditMeta<ImportJobConfiguration> meta)
    {
        //meta.Property(m => m.TargetTreeNodeId).RenderWith<TreeNodeSelect, string, TreeNode>(s => s.SelectedNode, select => {}, id => new TreeNode() {Id = id}, node => node.Id);
        meta.Property(m => m.KnowledgeTargetSettings.TargetTreeNodeId).RenderWith<TreeNodeIdSelect, string>(s => s.Id, select =>
        {
            select.Variant = Variant.Outlined;
        }).WithLabel("Tree node");

        meta.Property(m => m.Files).RenderWith<MudExUploadEdit<UploadableFile>, IList<UploadableFile>>(uploadEdit => uploadEdit.UploadRequests, uploadEdit =>
        {
            uploadEdit.AllowMultiple = true;
            uploadEdit.MinHeight = 400;
            uploadEdit.MaxHeight = 400;
            uploadEdit.AutoExtractArchive = true;
            uploadEdit.MimeTypes = Array.Empty<string>();
            uploadEdit.MimeRestrictionType = RestrictionType.BlackList;
            uploadEdit.StreamUrlHandling = StreamUrlHandling.BlobUrl;
        }).WithGroup("Files").WithoutLabel();

        string? treeNodeId = null;
        meta.Property(m => m.KnowledgeTargetSettings.TargetBranches)
            .WithLabel("Target views")
            .RenderData.AddCondition<ImportJobConfiguration>(c => !string.IsNullOrEmpty(treeNodeId = c.KnowledgeTargetSettings?.TargetTreeNodeId), 
                data => data.SetAttributes(new Dictionary<string, object> { { nameof(BranchSelect.TreeNodeId), treeNodeId } }), 
                data => data.SetAttributes(new Dictionary<string, object> { { nameof(BranchSelect.TreeNodeId), null } }));

        Branch[]? branches = null;
        meta.Property(m => m.KnowledgeTargetSettings.Group)
            .RenderData.AddCondition<ImportJobConfiguration>(c => (branches = c?.KnowledgeTargetSettings?.TargetBranches)?.Any() == true,
                data => data.SetAttributes(new Dictionary<string, object> { { nameof(GroupSelect.Branches), branches } }),
                data => data.SetAttributes(new Dictionary<string, object> { { nameof(GroupSelect.Branches), null } }));

        //meta.Properties().WrapInMudItem(i => i.xs = 12);
        //meta.Properties<bool>().WrapInMudItem(i =>
        //{
        //    i.xl = 6;
        //    i.lg = 6;
        //    i.xs = 12;
        //});
        meta.Property(m => m.KnowledgeTargetSettings.AttachFileToText).WithLabel("Attach file to text")
            .WithDescription("If checked the file that is used for the creation will attached to text");
        meta.Property(m => m.KnowledgeTargetSettings.CreateTreeNodesFromStructurePath).WithLabel("Create sub tree nodes")
            .WithDescription("When this option is enabled and the file you're using comes from a folder or a ZIP archive, the system will use the file's path structure. This means that the generated text will be represented in the tree following the same path as in the original folder or ZIP archive. This helps in maintaining a consistent hierarchy and organization in the tree, just as in the source file's location");

        return Task.CompletedTask;
    }
}