using SABIO.ClientApi.Responses;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeMediaImporter.Model;

public class KnowledgeTargetSettings
{
    [Required]
    public string TargetTreeNodeId { get; set; }
    public bool AttachFileToText { get; set; }
    public bool CreateTreeNodesFromStructurePath { get; set; }

    [Required]
    [MinLength(1)]
    public Branch[] TargetBranches { get; set; }

    [Required]
    public Group? Group { get; set; }
}