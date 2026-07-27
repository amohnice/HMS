using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Shop;

namespace HMS.Services;

public class ShopSaleService : IShopSaleService
{
    private readonly ApplicationDbContext _context;

    public ShopSaleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<ShopSale>> GetPaginatedSalesAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ShopSales
            .AsNoTracking()
            .Include(s => s.Cashier)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(s => s.SaleTime >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.SaleTime <= endDate.Value);

        query = query.OrderByDescending(s => s.SaleTime);
        return await PagedList<ShopSale>.CreateAsync(query, page, pageSize);
    }

    public async Task<(bool Success, string ErrorMessage, ShopSale? Sale)> CheckoutAsync(
        int cashierId, int[] productIds, int[] quantities, decimal discount, decimal tax, PaymentMethod paymentMethod, decimal amountPaid)
    {
        if (productIds == null || productIds.Length == 0)
            return (false, "Cart is empty.", null);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sale = new ShopSale
            {
                CashierId = cashierId,
                SaleTime = DateTime.Now,
                Discount = discount,
                Tax = tax,
                PaymentMethod = paymentMethod,
                Status = ShopSaleStatus.Completed
            };

            decimal subTotal = 0;
            for (int i = 0; i < productIds.Length; i++)
            {
                if (quantities[i] <= 0) continue;

                var product = await _context.ShopProducts.FindAsync(productIds[i]);
                if (product == null || !product.IsActive || product.StockQuantity < quantities[i])
                {
                    await transaction.RollbackAsync();
                    return (false, $"Insufficient stock for {product?.Name ?? "item"}.", null);
                }

                sale.Items.Add(new ShopSaleItem
                {
                    ProductId = product.Id,
                    Quantity = quantities[i],
                    UnitPrice = product.Price
                });
                subTotal += product.Price * quantities[i];
                product.StockQuantity -= quantities[i];
            }

            if (!sale.Items.Any())
            {
                await transaction.RollbackAsync();
                return (false, "No valid items in cart.", null);
            }

            sale.SubTotal = subTotal;
            sale.TotalAmount = subTotal - discount + tax;
            if (amountPaid < sale.TotalAmount)
            {
                await transaction.RollbackAsync();
                return (false, "Amount paid is less than total.", null);
            }

            sale.AmountPaid = amountPaid;
            sale.ChangeAmount = amountPaid - sale.TotalAmount;

            _context.ShopSales.Add(sale);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, string.Empty, sale);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Checkout failed: {ex.Message}", null);
        }
    }

    public async Task<ShopSale?> GetSaleWithDetailsAsync(int id)
    {
        return await _context.ShopSales
            .AsNoTracking()
            .Include(s => s.Cashier)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
