using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Restaurant;

namespace HMS.Services;

public class RestaurantMenuService : IRestaurantMenuService
{
    private readonly ApplicationDbContext _context;

    public RestaurantMenuService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<RestaurantMenu>> GetPaginatedMenusAsync(int page, int pageSize, string? search = null, RestaurantMenuCategory? categoryFilter = null)
    {
        var query = _context.RestaurantMenus.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower) || m.Category.ToString().ToLower().Contains(searchLower));
        }

        if (categoryFilter.HasValue)
        {
            query = query.Where(m => m.Category == categoryFilter.Value);
        }

        query = query.OrderBy(m => m.Category).ThenBy(m => m.Name);
        return await PagedList<RestaurantMenu>.CreateAsync(query, page, pageSize);
    }

    public async Task<List<RestaurantMenu>> GetAvailableMenuItemsAsync()
    {
        return await _context.RestaurantMenus
            .AsNoTracking()
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<RestaurantMenu?> GetMenuByIdAsync(int id)
    {
        return await _context.RestaurantMenus.FindAsync(id);
    }

    public async Task<bool> CreateMenuAsync(RestaurantMenu menu)
    {
        menu.CreatedAt = DateTime.Now;
        _context.RestaurantMenus.Add(menu);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMenuAsync(RestaurantMenu menu)
    {
        var existing = await _context.RestaurantMenus.FindAsync(menu.Id);
        if (existing == null) return false;

        existing.Name = menu.Name;
        existing.Category = menu.Category;
        existing.Price = menu.Price;
        existing.Description = menu.Description;
        existing.IsAvailable = menu.IsAvailable;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMenuAsync(int id)
    {
        var menu = await _context.RestaurantMenus.FindAsync(id);
        if (menu == null) return false;

        _context.RestaurantMenus.Remove(menu);
        await _context.SaveChangesAsync();
        return true;
    }
}
