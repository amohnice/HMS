using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Common;
using HMS.Models.Shop;

namespace HMS.Services;

public class ShopProductService : IShopProductService
{
    private readonly ApplicationDbContext _context;

    public ShopProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<ShopProduct>> GetPaginatedProductsAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.ShopProducts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) || (p.Barcode != null && p.Barcode.ToLower().Contains(searchLower)));
        }

        query = query.OrderBy(p => p.Category).ThenBy(p => p.Name);
        return await PagedList<ShopProduct>.CreateAsync(query, page, pageSize);
    }

    public async Task<List<ShopProduct>> GetAllActiveProductsAsync()
    {
        return await _context.ShopProducts
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ShopProduct?> GetProductByIdAsync(int id)
    {
        return await _context.ShopProducts.FindAsync(id);
    }

    public async Task<ShopProduct?> GetProductByBarcodeAsync(string barcode)
    {
        return await _context.ShopProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);
    }

    public async Task<bool> CreateProductAsync(ShopProduct product)
    {
        product.CreatedAt = DateTime.Now;
        _context.ShopProducts.Add(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProductAsync(ShopProduct product)
    {
        var existing = await _context.ShopProducts.FindAsync(product.Id);
        if (existing == null) return false;

        existing.Name = product.Name;
        existing.Category = product.Category;
        existing.Barcode = product.Barcode;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.IsActive = product.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.ShopProducts.FindAsync(id);
        if (product == null) return false;

        _context.ShopProducts.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ShopProduct>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _context.ShopProducts
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }
}
