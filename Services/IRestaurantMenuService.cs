using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Restaurant;

namespace HMS.Services;

public interface IRestaurantMenuService
{
    Task<PagedList<RestaurantMenu>> GetPaginatedMenusAsync(int page, int pageSize, string? search = null, RestaurantMenuCategory? categoryFilter = null);

    /// <summary>
    /// Item count per category, for the context column. Honours the search term so the
    /// counts describe what filtering would actually show, and includes empty categories.
    /// </summary>
    Task<Dictionary<RestaurantMenuCategory, int>> GetCategoryCountsAsync(string? search = null);
    Task<List<RestaurantMenu>> GetAvailableMenuItemsAsync();
    Task<RestaurantMenu?> GetMenuByIdAsync(int id);
    Task<bool> CreateMenuAsync(RestaurantMenu menu);
    Task<bool> UpdateMenuAsync(RestaurantMenu menu);
    Task<bool> DeleteMenuAsync(int id);
}
