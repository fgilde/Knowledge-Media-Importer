using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Contracts;

// Define the progress structure.
public class Progress
{
    public CancellationTokenSource Cancellation { get; }

    public Progress(IUploadableFile file, CancellationTokenSource cancellation)
    {
        File = file;
        Cancellation = cancellation;
    }
    public IUploadableFile File { get; set; }
    public string Text { get; set; }
    public int Value { get; set; }
}