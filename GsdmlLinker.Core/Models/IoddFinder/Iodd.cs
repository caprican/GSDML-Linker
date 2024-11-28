using System.Text.Json.Serialization;

namespace GsdmlLinker.Core.Models.IoddFinder;

public class Iodd
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("hasMoreVersions")]
    public bool HasMoreVersions { get; set; } = false;

    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("indicationOfSource")]
    public string IndicationOfSource { get; set; } = string.Empty;

    [JsonPropertyName("driverName")]
    public string DriverName { get; set; } = string.Empty;

    [JsonPropertyName("uploadDate")]
    public long UploadDate { get; set; }
    [JsonPropertyName("releaseDate")]
    public long ReleaseDate { get; set; }


    
    [JsonPropertyName("vendor")]
    public Vendor? Vendor { get; set; }
    [JsonPropertyName("vendorName")]
    public string? VendorName { get; set; }
    [JsonPropertyName("vendorId")]
    public uint VendorId { get; set; }

    [JsonPropertyName("deviceId")]
    public uint DeviceId { get; set; }
    [JsonPropertyName("deviceFamily")]
    public string? DeviceFamily { get; set; }
    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("ioLinkRev")]
    public string IoLinkRev { get; set; } = string.Empty;
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
    [JsonPropertyName("versionString")]
    private string VersionString { set { Version = value; } }

    [JsonPropertyName("ioddId")]
    public long IoddId { get; set; }
    [JsonPropertyName("ioddStatus")]
    public string? IoddStatus { get; set; }

    [JsonPropertyName("productId")]
    public string? ProductId { get; set; }
    [JsonPropertyName("productVariantId")]
    public long ProductVariantId { get; set; }
    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }



    [JsonPropertyName("hasMd")]
    public bool HasMd { get; set; } = false;
    [JsonPropertyName("md")]
    public Md? Md { get; set; }
}
