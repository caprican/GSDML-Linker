using System.Text.Json.Serialization;

namespace GsdmlLinker.Core.Models.IoddFinder;

public class Vendor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vendorId")]
    public long VendorId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
