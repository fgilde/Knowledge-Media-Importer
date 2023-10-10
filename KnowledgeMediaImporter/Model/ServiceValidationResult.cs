namespace KnowledgeMediaImporter.Model;

public class ServiceValidationResult
{
    public static ServiceValidationResult Success => new();
    public static ServiceValidationResult Fail(string error) => new() {Errors = { error }};
    public bool IsValid => Errors.Count == 0;

    public List<string> Errors { get; } = new List<string>();
}