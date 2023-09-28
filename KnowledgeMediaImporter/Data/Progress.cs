namespace KnowledgeMediaImporter.Data;

public class Progress
{
    public CancellationTokenSource Cancellation { get; }

    public Progress(CancellationTokenSource cancellation)
    {
        Cancellation = cancellation;
    }

    public string Text { get; set; }
    public int Value { get; set; }
}