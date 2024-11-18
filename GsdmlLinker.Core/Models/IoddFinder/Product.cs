using System.Text.Json.Serialization;
namespace GsdmlLinker.Core.Models.IoddFinder;

public class Product
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;
}
