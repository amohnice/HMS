using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Restaurant;

namespace HMS.Services;

public interface IRestaurantMenuService
{
    Task<PagedList<RestaurantMenu>> GetPaginatedMenusAsync(int page, int pageSize, string? search = null, RestaurantMenuCategory? categoryFilter = null);
    Task<List<RestaurantMenu>> GetAvailableMenuItemsAsync();
    Task<RestaurantMenu?> GetMenuByIdAsync(int id);
    Task<bool> CreateMenuAsync(RestaurantMenu menu);
    Task<bool> UpdateMenuAsync(RestaurantMenu menu);
    Task<bool> DeleteMenuAsync(int id);
}
