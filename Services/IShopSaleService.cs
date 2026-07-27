using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Shop;

namespace HMS.Services;

public interface IShopSaleService
{
    Task<PagedList<ShopSale>> GetPaginatedSalesAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null);
    Task<(bool Success, string ErrorMessage, ShopSale? Sale)> CheckoutAsync(int cashierId, int[] productIds, int[] quantities, decimal discount, decimal tax, PaymentMethod paymentMethod, decimal amountPaid);
    Task<ShopSale?> GetSaleWithDetailsAsync(int id);
}
