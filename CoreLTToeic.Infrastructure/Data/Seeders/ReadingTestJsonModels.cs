using System.Text.Json.Serialization;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ReadingTestJson
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("targetTestTitle")]
    public string TargetTestTitle { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 120;

    [JsonPropertyName("source")]
    public ListeningTestSourceJson Source { get; set; } = new();

    [JsonPropertyName("parts")]
    public List<ListeningPartJson> Parts { get; set; } = [];
}
