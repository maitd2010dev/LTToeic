using System.Text.Json.Serialization;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class CourseCatalogSeedJson
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("sourceCourseTitle")]
    public string SourceCourseTitle { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("courses")]
    public List<CourseCatalogItemJson> Courses { get; set; } = [];
}

public class CourseCatalogItemJson
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("objective")]
    public string Objective { get; set; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("previewVideoUrl")]
    public string PreviewVideoUrl { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Published";

    [JsonPropertyName("sectionTitles")]
    public List<string> SectionTitles { get; set; } = [];
}
