using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using HMS.Data;
using HMS.Models;

namespace HMS.Services.Navigation;

/// <summary>
/// Supplies the counts shown on rail items. These replaced the dashboard stat cards,
/// so they render on every page — results are cached briefly to keep the rail cheap.
/// </summary>
public class NavBadgeService : INavBadgeService
{
    private static readonly TimeSpan CacheWindow = TimeSpan.FromSeconds(15);

    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public NavBadgeService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IReadOnlyDictionary<string, int>> GetBadgesAsync(string? role)
    {
        var wanted = NavigationManifest.For(role)
            .Where(i => i.BadgeKey is not null)
            .Select(i => i.BadgeKey!)
            .Distinct()
            .ToArray();

        if (wanted.Length == 0)
            return new Dictionary<string, int>();

        var cacheKey = $"navbadges:{role}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyDictionary<string, int>? cached) && cached is not null)
            return cached;

        var badges = new Dictionary<string, int>();

        foreach (var key in wanted)
            badges[key] = await CountAsync(key);

        _cache.Set(cacheKey, (IReadOnlyDictionary<string, int>)badges, CacheWindow);
        return badges;
    }

    private Task<int> CountAsync(string key) => key switch
    {
        NavigationManifest.BadgeMenuItems =>
            _context.RestaurantMenus.CountAsync(),

        NavigationManifest.BadgeTables =>
            _context.RestaurantTables.CountAsync(),

        // "Open" is anything still on the floor — not yet settled or abandoned.
        NavigationManifest.BadgeOpenOrders =>
            _context.RestaurantOrders.CountAsync(o =>
                o.Status != RestaurantOrderStatus.Paid &&
                o.Status != RestaurantOrderStatus.Cancelled),

        NavigationManifest.BadgeKitchenQueue =>
            _context.RestaurantOrders.CountAsync(o =>
                o.Status == RestaurantOrderStatus.Pending ||
                o.Status == RestaurantOrderStatus.Preparing),

        // What the cashier owes attention to: food is out, money is not in.
        NavigationManifest.BadgeUnpaidOrders =>
            _context.RestaurantOrders.CountAsync(o =>
                o.Status == RestaurantOrderStatus.Served ||
                o.Status == RestaurantOrderStatus.Ready),

        NavigationManifest.BadgeProducts =>
            _context.ShopProducts.CountAsync(),

        NavigationManifest.BadgeLowStock =>
            _context.ShopProducts.CountAsync(p => p.StockQuantity <= 10),

        NavigationManifest.BadgeStaff =>
            _context.Users.CountAsync(),

        _ => Task.FromResult(0)
    };
}
