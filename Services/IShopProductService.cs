using HMS.Models.Common;
using HMS.Models.Shop;

namespace HMS.Services;

public interface IShopProductService
{
    Task<PagedList<ShopProduct>> GetPaginatedProductsAsync(int page, int pageSize, string? search = null);
    Task<List<ShopProduct>> GetAllActiveProductsAsync();
    Task<ShopProduct?> GetProductByIdAsync(int id);
    Task<ShopProduct?> GetProductByBarcodeAsync(string barcode);
    Task<bool> CreateProductAsync(ShopProduct product);
    Task<bool> UpdateProductAsync(ShopProduct product);
    Task<bool> DeleteProductAsync(int id);
    Task<List<ShopProduct>> GetLowStockProductsAsync(int threshold = 10);
    Task<bool> AdjustStockAsync(int productId, int deltaQuantity, HMS.Models.StockMovementType type, string? reason, int? userId);
    Task<PagedList<StockMovement>> GetPaginatedAuditTrailAsync(int page, int pageSize, int? productId = null, HMS.Models.StockMovementType? typeFilter = null);
}
