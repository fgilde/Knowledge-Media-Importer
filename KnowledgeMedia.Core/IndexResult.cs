namespace KnowledgeMedia.Core;

public class IndexResult
{
    public string partition { get; set; }
    public string description { get; set; }
    public string privacyMode { get; set; }
    public string state { get; set; }
    public string accountId { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public string userName { get; set; }
    public string created { get; set; }
    public bool isOwned { get; set; }
    public bool isEditable { get; set; }
    public bool isBase { get; set; }
    public int durationInSeconds { get; set; }
    public string duration { get; set; }
    public SummarizedInsights summarizedInsights { get; set; }
    public Videos[] videos { get; set; }
    public VideosRanges[] videosRanges { get; set; }
}

public class SummarizedInsights
{
    public string name { get; set; }
    public string id { get; set; }
    public string privacyMode { get; set; }
    public Duration duration { get; set; }
    public string thumbnailVideoId { get; set; }
    public string thumbnailId { get; set; }
    public object[] faces { get; set; }
    public Keywords[] keywords { get; set; }
    public Sentiments[] sentiments { get; set; }
    public Emotions[] emotions { get; set; }
    public AudioEffects[] audioEffects { get; set; }
    public Labels[] labels { get; set; }
    public object[] framePatterns { get; set; }
    public Brands[] brands { get; set; }
    public NamedLocations[] namedLocations { get; set; }
    public object[] namedPeople { get; set; }
    public Statistics statistics { get; set; }
    public Topics[] topics { get; set; }
}

public class Duration
{
    public string time { get; set; }
    public double seconds { get; set; }
}

public class Keywords
{
    public bool isTranscript { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public Appearances[] appearances { get; set; }
}

public class Appearances
{
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class Sentiments
{
    public string sentimentKey { get; set; }
    public double seenDurationRatio { get; set; }
    public Appearances1[] appearances { get; set; }
}

public class Appearances1
{
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class Emotions
{
    public string type { get; set; }
    public double seenDurationRatio { get; set; }
    public Appearances2[] appearances { get; set; }
}

public class Appearances2
{
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class AudioEffects
{
    public string audioEffectKey { get; set; }
    public double seenDurationRatio { get; set; }
    public int seenDuration { get; set; }
    public Appearances3[] appearances { get; set; }
}

public class Appearances3
{
    public double confidence { get; set; }
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class Labels
{
    public int id { get; set; }
    public string name { get; set; }
    public Appearances4[] appearances { get; set; }
}

public class Appearances4
{
    public double confidence { get; set; }
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class Brands
{
    public string referenceId { get; set; }
    public string referenceUrl { get; set; }
    public double confidence { get; set; }
    public string description { get; set; }
    public double seenDuration { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public Appearances5[] appearances { get; set; }
}

public class Appearances5
{
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class NamedLocations
{
    public object referenceId { get; set; }
    public object referenceUrl { get; set; }
    public double confidence { get; set; }
    public object description { get; set; }
    public double seenDuration { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public Appearances6[] appearances { get; set; }
}

public class Appearances6
{
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class Statistics
{
    public int correspondenceCount { get; set; }
    public SpeakerTalkToListenRatio speakerTalkToListenRatio { get; set; }
    public SpeakerLongestMonolog speakerLongestMonolog { get; set; }
    public SpeakerNumberOfFragments speakerNumberOfFragments { get; set; }
    public SpeakerWordCount speakerWordCount { get; set; }
}

public class SpeakerTalkToListenRatio
{
    public int _ { get; set; }
}

public class SpeakerLongestMonolog
{
    public int _ { get; set; }
}

public class SpeakerNumberOfFragments
{
    public int _ { get; set; }
}

public class SpeakerWordCount
{
    public int _ { get; set; }
}

public class Topics
{
    public string referenceUrl { get; set; }
    public string iptcName { get; set; }
    public string iabName { get; set; }
    public double confidence { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public Appearances7[] appearances { get; set; }
}

public class Appearances7
{
    public string startTime { get; set; }
    public string endTime { get; set; }
    public double startSeconds { get; set; }
    public double endSeconds { get; set; }
}

public class Videos
{
    public string accountId { get; set; }
    public string id { get; set; }
    public string state { get; set; }
    public string moderationState { get; set; }
    public string reviewState { get; set; }
    public string privacyMode { get; set; }
    public string processingProgress { get; set; }
    public string failureMessage { get; set; }
    public object externalId { get; set; }
    public object externalUrl { get; set; }
    public object metadata { get; set; }
    public Insights insights { get; set; }
    public string thumbnailId { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public bool detectSourceLanguage { get; set; }
    public string languageAutoDetectMode { get; set; }
    public string sourceLanguage { get; set; }
    public string[] sourceLanguages { get; set; }
    public string language { get; set; }
    public string[] languages { get; set; }
    public string indexingPreset { get; set; }
    public string streamingPreset { get; set; }
    public string linguisticModelId { get; set; }
    public string personModelId { get; set; }
    public object logoGroupId { get; set; }
    public bool isAdult { get; set; }
    public string publishedUrl { get; set; }
    public object publishedProxyUrl { get; set; }
    public string viewToken { get; set; }
}

public class Insights
{
    public string version { get; set; }
    public string duration { get; set; }
    public string sourceLanguage { get; set; }
    public string[] sourceLanguages { get; set; }
    public string language { get; set; }
    public string[] languages { get; set; }
    public Transcript[] transcript { get; set; }
    public Ocr[] ocr { get; set; }
    public Keywords1[] keywords { get; set; }
    public Topics1[] topics { get; set; }
    public Labels1[] labels { get; set; }
    public Scenes[] scenes { get; set; }
    public Shots[] shots { get; set; }
    public Brands1[] brands { get; set; }
    public NamedLocations1[] namedLocations { get; set; }
    public AudioEffects1[] audioEffects { get; set; }
    public DetectedObjects[] detectedObjects { get; set; }
    public Sentiments1[] sentiments { get; set; }
    public Emotions1[] emotions { get; set; }
    public VisualContentModeration[] visualContentModeration { get; set; }
    public Blocks[] blocks { get; set; }
    public Speakers[] speakers { get; set; }
    public TextualContentModeration textualContentModeration { get; set; }
    public Statistics1 statistics { get; set; }
}

public class Transcript
{
    public int id { get; set; }
    public string text { get; set; }
    public double confidence { get; set; }
    public int speakerId { get; set; }
    public string language { get; set; }
    public Instances[] instances { get; set; }
}

public class Instances
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Ocr
{
    public int id { get; set; }
    public string text { get; set; }
    public double confidence { get; set; }
    public int left { get; set; }
    public int top { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int angle { get; set; }
    public string language { get; set; }
    public Instances1[] instances { get; set; }
}

public class Instances1
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Keywords1
{
    public int id { get; set; }
    public string text { get; set; }
    public double confidence { get; set; }
    public string language { get; set; }
    public Instances2[] instances { get; set; }
}

public class Instances2
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Topics1
{
    public int id { get; set; }
    public string name { get; set; }
    public string referenceId { get; set; }
    public string referenceType { get; set; }
    public string iptcName { get; set; }
    public double confidence { get; set; }
    public string iabName { get; set; }
    public string language { get; set; }
    public Instances3[] instances { get; set; }
    public string referenceUrl { get; set; }
}

public class Instances3
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Labels1
{
    public int id { get; set; }
    public string name { get; set; }
    public string language { get; set; }
    public Instances4[] instances { get; set; }
    public string referenceId { get; set; }
}

public class Instances4
{
    public double confidence { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Scenes
{
    public int id { get; set; }
    public Instances5[] instances { get; set; }
}

public class Instances5
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Shots
{
    public int id { get; set; }
    public KeyFrames[] keyFrames { get; set; }
    public Instances6[] instances { get; set; }
}

public class KeyFrames
{
    public int id { get; set; }
    public Instances7[] instances { get; set; }
}

public class Instances7
{
    public string thumbnailId { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Instances6
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Brands1
{
    public int id { get; set; }
    public string referenceType { get; set; }
    public string name { get; set; }
    public string referenceId { get; set; }
    public string referenceUrl { get; set; }
    public string description { get; set; }
    public object[] tags { get; set; }
    public double confidence { get; set; }
    public bool isCustom { get; set; }
    public Instances8[] instances { get; set; }
}

public class Instances8
{
    public string brandType { get; set; }
    public string instanceSource { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class NamedLocations1
{
    public int id { get; set; }
    public string name { get; set; }
    public object referenceId { get; set; }
    public object referenceUrl { get; set; }
    public object description { get; set; }
    public object[] tags { get; set; }
    public double confidence { get; set; }
    public bool isCustom { get; set; }
    public Instances9[] instances { get; set; }
}

public class Instances9
{
    public string instanceSource { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class AudioEffects1
{
    public int id { get; set; }
    public string type { get; set; }
    public Instances10[] instances { get; set; }
}

public class Instances10
{
    public double confidence { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class DetectedObjects
{
    public int id { get; set; }
    public string type { get; set; }
    public string thumbnailId { get; set; }
    public string displayName { get; set; }
    public string wikiDataId { get; set; }
    public Instances11[] instances { get; set; }
}

public class Instances11
{
    public double confidence { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Sentiments1
{
    public int id { get; set; }
    public double averageScore { get; set; }
    public string sentimentType { get; set; }
    public Instances12[] instances { get; set; }
}

public class Instances12
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Emotions1
{
    public int id { get; set; }
    public string type { get; set; }
    public Instances13[] instances { get; set; }
}

public class Instances13
{
    public double confidence { get; set; }
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class VisualContentModeration
{
    public int id { get; set; }
    public double adultScore { get; set; }
    public double racyScore { get; set; }
    public Instances14[] instances { get; set; }
}

public class Instances14
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Blocks
{
    public int id { get; set; }
    public Instances15[] instances { get; set; }
}

public class Instances15
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class Speakers
{
    public int id { get; set; }
    public string name { get; set; }
    public Instances16[] instances { get; set; }
}

public class Instances16
{
    public string adjustedStart { get; set; }
    public string adjustedEnd { get; set; }
    public string start { get; set; }
    public string end { get; set; }
}

public class TextualContentModeration
{
    public int id { get; set; }
    public int bannedWordsCount { get; set; }
    public int bannedWordsRatio { get; set; }
    public object[] instances { get; set; }
}

public class Statistics1
{
    public int correspondenceCount { get; set; }
    public SpeakerTalkToListenRatio1 speakerTalkToListenRatio { get; set; }
    public SpeakerLongestMonolog1 speakerLongestMonolog { get; set; }
    public SpeakerNumberOfFragments1 speakerNumberOfFragments { get; set; }
    public SpeakerWordCount1 speakerWordCount { get; set; }
}

public class SpeakerTalkToListenRatio1
{
    public int _ { get; set; }
}

public class SpeakerLongestMonolog1
{
    public int _ { get; set; }
}

public class SpeakerNumberOfFragments1
{
    public int _ { get; set; }
}

public class SpeakerWordCount1
{
    public int _ { get; set; }
}

public class VideosRanges
{
    public string videoId { get; set; }
    public Range range { get; set; }
}

public class Range
{
    public string start { get; set; }
    public string end { get; set; }
}

