using System.ComponentModel.DataAnnotations;

namespace KnowledgeMediaImporter.Configuration;

public class ServiceSettings
{
    public KnowledgeSettings Knowledge { get; set; }
    public VideoIndexerSettings VideoIndexer { get; set; }
    public ChatGptSettings ChatGpt { get; set; }
}

public class KnowledgeSettings
{
    [Required]
    public string Url { get; set; }
    [Required]
    public string Realm { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string ApiKey { get; set; }
}

public class VideoIndexerSettings
{
    [Required]
    public string Url { get; set; }
    [Required]
    public string Location { get; set; }
    [Required]
    public string ApiKey { get; set; }
    [Required]
    public string AccountId { get; set; }
}

public class ChatGptSettings
{
    [Required]
    public string Model { get; set; }
    [Required]
    public string ApiKey { get; set; }
}