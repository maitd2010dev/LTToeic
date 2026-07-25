using System.Text.Json.Serialization;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ListeningTestJson
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active";

    [JsonPropertyName("source")]
    public ListeningTestSourceJson Source { get; set; } = new();

    [JsonPropertyName("parts")]
    public List<ListeningPartJson> Parts { get; set; } = [];
}

public class ListeningTestSourceJson
{
    [JsonPropertyName("questions")]
    public string Questions { get; set; } = string.Empty;

    [JsonPropertyName("transcript")]
    public string Transcript { get; set; } = string.Empty;
}

public class ListeningPartJson
{
    [JsonPropertyName("partNum")]
    public int PartNum { get; set; }

    [JsonPropertyName("directions")]
    public string? Directions { get; set; }

    [JsonPropertyName("questions")]
    public List<ListeningQuestionJson> Questions { get; set; } = [];

    [JsonPropertyName("groups")]
    public List<ListeningQuestionGroupJson> Groups { get; set; } = [];
}

public class ListeningQuestionGroupJson
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];

    [JsonPropertyName("questions")]
    public List<ListeningQuestionJson> Questions { get; set; } = [];
}

public class ListeningQuestionJson
{
    [JsonPropertyName("orderNumber")]
    public int OrderNumber { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("answer1")]
    public string? Answer1 { get; set; }

    [JsonPropertyName("answer2")]
    public string? Answer2 { get; set; }

    [JsonPropertyName("answer3")]
    public string? Answer3 { get; set; }

    [JsonPropertyName("answer4")]
    public string? Answer4 { get; set; }

    [JsonPropertyName("correctAnswer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }
}
