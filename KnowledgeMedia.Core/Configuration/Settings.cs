namespace KnowledgeMedia.Core.Configuration;

public class ServiceSettings
{
    public KnowledgeSettings Knowledge { get; set; }
    public VideoIndexerSettings VideoIndexer { get; set; }
    public ChatGptSettings ChatGpt { get; set; }
}

public class KnowledgeSettings
{
    public string Url { get; set; }
    public string Realm { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string ApiKey { get; set; }
}

public class VideoIndexerSettings
{
    public string Url { get; set; }
    public string Location { get; set; }
    public string ApiKey { get; set; }
    public string AccountId { get; set; }
}

public class ChatGptSettings
{
    public string Model { get; set; }
    public string ApiKey { get; set; }
}