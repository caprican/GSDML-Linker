using System.Text.Json;

using GsdmlLinker.Core.Contracts.Services;
using GsdmlLinker.Core.Models.IoddFinder;

namespace GsdmlLinker.Services;

public class IoddfinderService : IIoddfinderService
{
    private readonly HttpClient httpClient = new() 
    {
        BaseAddress = new Uri("https://ioddfinder.io-link.com/api/"),
        Timeout = TimeSpan.FromSeconds(5)
    };
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task<List<string>?> GetVendorsNameAsync()
    {
        var httpResponseMessage = await httpClient.GetAsync("drivers/vendors");
        var response = await httpResponseMessage.Content.ReadAsStringAsync();
        var vendors = JsonSerializer.Deserialize<List<string>>(response, serializerOptions);

        return vendors;
    }

    public async Task<List<Iodd>> GetProductVariantFromVendor(string vendorName)
    {
        var httpResponseMessage = await httpClient.GetAsync($"productvariants?size=2000&vendorName={vendorName}");
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<Iodd>>(json, serializerOptions);

        var result = new List<Iodd>();
        if(response is not null)
        {
            if (response.Content is not null)
            {
                result.AddRange(response.Content);
            }
            if(response.TotalElements > response.NumberOfElements)
            {
                var pages = response!.TotalPages;
                for (int page = 1; page < pages; page++)
                {
                    httpResponseMessage = await httpClient.GetAsync($"productvariants?page={page}&size=2000&vendorName={vendorName}");
                    json = await httpResponseMessage.Content.ReadAsStringAsync();
                    response = JsonSerializer.Deserialize<ApiResponse<Iodd>>(json, serializerOptions);

                    if(response?.Content is not null)
                    {
                        result.AddRange(response.Content);
                    }
                }
            }
        }

        return result;
    }

    public async Task<List<Iodd>> GetProductByName(string productName)
    {
        var httpResponseMessage = await httpClient.GetAsync($"productvariants?size=2000&productName={productName}");
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<Iodd>>(json, serializerOptions);

        var result = new List<Iodd>();
        if (response is not null)
        {
            if (response.Content is not null)
            {
                result.AddRange(response.Content);
            }
            if (response.TotalElements > response.NumberOfElements)
            {
                var pages = response!.TotalPages;
                for (int page = 1; page < pages; page++)
                {
                    httpResponseMessage = await httpClient.GetAsync($"productvariants?page={page}&size=2000&productName={productName}");
                    json = await httpResponseMessage.Content.ReadAsStringAsync();
                    response = JsonSerializer.Deserialize<ApiResponse<Iodd>>(json, serializerOptions);

                    if (response?.Content is not null)
                    {
                        result.AddRange(response.Content);
                    }
                }
            }
        }

        return result;
    }

    public async Task<ProductVariant?> GetProductVariantAsync(long productVariantId)
    {
        var httpResponseMessage = await httpClient.GetAsync($"productvariants/{productVariantId}");
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ProductVariant?>(json, serializerOptions);

        //if(response is not null)
        //{
        //    response.Icon = await GetDeviceIcon(productVariantId);
        //}

        return response;
    }
    public async Task<string> GetDeviceIcon(long productVariantId)
    {
        var httpResponseMessage = await httpClient.GetAsync($"productvariants/{productVariantId}/files/icon");
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        if(httpResponseMessage.IsSuccessStatusCode)
        { 
            var response = JsonSerializer.Deserialize<string>(json, serializerOptions);

            return response!;
        }
        else
            return string.Empty;
    }

    public async Task<List<Iodd>> GetDriversAsync(string vendorName)
    {
        //var httpResponseMessage = await httpClient.GetAsync($"drivers?size=2000&status=APPROVED&status=UPLOADED&vendorName={vendorName}");
        var httpResponseMessage = await httpClient.GetAsync($"drivers?size=2000&vendorName={vendorName}");
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<Iodd>>(json, serializerOptions);

        var result = new List<Iodd>();
        if (response is not null)
        {
            if (response.Content is not null)
            {
                result.AddRange(response.Content);
            }
            if (response.TotalElements > response.NumberOfElements)
            {
                var pages = response!.TotalPages;
                for (int page = 1; page < pages; page++)
                {
                    httpResponseMessage = await httpClient.GetAsync($"drivers?page={page}&size=2000&vendorName={vendorName}");
                    json = await httpResponseMessage.Content.ReadAsStringAsync();
                    response = JsonSerializer.Deserialize<ApiResponse<Iodd>>(json, serializerOptions);

                    if (response?.Content is not null)
                    {
                        result.AddRange(response.Content);
                    }
                }
            }
        }

        return result;
    }





    public async Task<ProductVariant> GetProductVariantMenusAsync(string vendorId, string deviceId)
    {
        var httpResponseMessage = await httpClient.GetAsync($"productvariants/{vendorId}/{deviceId}/viewer?version=1.1");
        var json = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ProductVariant>(json, serializerOptions);

        return response!;
    }

    public async Task<byte[]?> GetIoddZipAsync(uint vendorId, long ioddId)
    {
        var httpResponseMessage = await httpClient.GetAsync($"vendors/{vendorId}/iodds/{ioddId}/files/zip/rated");
        return await httpResponseMessage.Content.ReadAsByteArrayAsync();
    }
}
