using MudBlazor.Extensions.Components;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeMediaImporter.Model;

public class ImportJobConfiguration
{
    public KnowledgeTargetSettings KnowledgeTargetSettings { get; set; }

    [Required]
    [MinLength(1)]
    public IList<UploadableFile> Files { get; set; }
}