using System.Text.Json.Serialization;

namespace GsdmlLinker.Core.Models.IoddFinder;

public class Md
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    [JsonPropertyName("revision")]
    public string? Revision { get; set; }

    [JsonPropertyName("releasedAt")]
    public long? ReleasedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public long? UpdatedAt { get; set; }
}
