using System.Text.Json.Serialization;

namespace GsdmlLinker.Core.Models.IoddFinder;

public class ProductVariant
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("productDescription")]
    public string ProductDescription { get; set; } = string.Empty;

    [JsonPropertyName("product")]
    public Product? Product { get; set; }
    
    [JsonPropertyName("iodd")]
    public Iodd? Iodd { get; set; }


    [JsonPropertyName("vendor")]
    public Vendor? Vendor { get; set; }

    //public string? Icon { get; set; }
}
