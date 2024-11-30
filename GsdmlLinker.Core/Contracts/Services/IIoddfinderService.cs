using GsdmlLinker.Core.Models.IoddFinder;

namespace GsdmlLinker.Core.Contracts.Services;

public interface IIoddfinderService
{
    public Task<List<string>?> GetVendorsNameAsync();
    public Task<List<Iodd>> GetProductVariantFromVendor(string vendorName);
    public Task<List<Iodd>> GetProductByName(string productName);

    public Task<ProductVariant?> GetProductVariantAsync(long productVariantId);

    public Task<string> GetDeviceIcon(long productVariantId);

    public Task<byte[]?> GetIoddZipAsync(uint vendorId, long ioddId);

    public Task<ProductVariant> GetProductVariantMenusAsync(string vendorId, string deviceId);

    public Task<List<Iodd>> GetDriversAsync(string vendorName);

}
