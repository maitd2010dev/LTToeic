using System.Text.Json.Serialization;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ToeicTestVariantSeedJson
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("sourceTestTitle")]
    public string SourceTestTitle { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public ToeicTestVariantSourceJson Source { get; set; } = new();

    [JsonPropertyName("variants")]
    public List<ToeicTestVariantJson> Variants { get; set; } = [];
}

public class ToeicTestVariantSourceJson
{
    [JsonPropertyName("listening")]
    public string Listening { get; set; } = string.Empty;

    [JsonPropertyName("reading")]
    public string Reading { get; set; } = string.Empty;
}

public class ToeicTestVariantJson
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 120;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active";
}
