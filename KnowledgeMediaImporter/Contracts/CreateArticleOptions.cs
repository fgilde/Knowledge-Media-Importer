using KnowledgeMediaImporter.Model;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Contracts;

public class CreateArticleOptions
{
    public CreateArticleOptions(string title, string content, IUploadableFile file, KnowledgeTargetSettings targetSettings, IProgressUpdate progress, CancellationToken cancellationToken)
    {
        Title = title;
        Content = content;
        File = file;
        TargetSettings = targetSettings;
        CancellationToken = cancellationToken;
        Progress = progress;
    }

    public bool IsCancelled => CancellationToken.IsCancellationRequested;

    public string Title { get; set; }
    public string Content { get; set; }
    public IUploadableFile File { get; set; }
    public KnowledgeTargetSettings TargetSettings { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public IProgressUpdate Progress { get; set; }
}