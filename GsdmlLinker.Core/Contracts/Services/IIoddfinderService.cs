using GsdmlLinker.Core.Models.IoddFinder;

namespace GsdmlLinker.Core.Contracts.Services;

public interface IIoddfinderService
{
    public Task<List<string>?> GetVendorsNameAsync();
    public Task<List<Iodd>> GetProductVariantFromVendor(string vendorName);
    public Task<List<Iodd>> GetProductByName(string productName);

    public Task<ProductVariant?> GetProductVariantAsync(long productVariantId);

    public Task<string> GetDeviceIcon(long productVariantId);

    public Task<byte[]?> GetIoddZipAsync(int vendorId, int ioddId);



    public Task<List<Iodd>> GetDriversAsync(string vendorName);

}
