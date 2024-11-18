using System.Text.Json.Serialization;

namespace GsdmlLinker.Core.Models.IoddFinder;

public class ApiResponse<T>
{
    [JsonPropertyName("content")]
    public List<T>? Content { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("numberOfElements")]
    public long NumberOfElements { get; set; }
    
    [JsonPropertyName("sort")]
    public List<object>? Sort { get; set; }

    [JsonPropertyName("first")]
    public bool First { get; set; }
    [JsonPropertyName("last")]
    public bool Last { get; set; }

    [JsonPropertyName("totalPages")]
    public long TotalPages { get; set; }
    [JsonPropertyName("totalElements")]
    public long TotalElements { get; set; }
}
