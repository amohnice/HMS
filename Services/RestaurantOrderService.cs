using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Restaurant;

namespace HMS.Services;

public class RestaurantOrderService : IRestaurantOrderService
{
    private readonly ApplicationDbContext _context;

    public RestaurantOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<RestaurantOrder>> GetPaginatedOrdersAsync(int page, int pageSize, RestaurantOrderStatus? statusFilter = null)
    {
        var query = _context.RestaurantOrders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(o => o.Status == statusFilter.Value);
        }

        query = query.OrderByDescending(o => o.OrderTime);
        return await PagedList<RestaurantOrder>.CreateAsync(query, page, pageSize);
    }

    public async Task<Dictionary<RestaurantOrderStatus, int>> GetStatusCountsAsync()
    {
        var counted = await _context.RestaurantOrders
            .AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        foreach (var status in Enum.GetValues<RestaurantOrderStatus>())
            counted.TryAdd(status, 0);

        return counted;
    }

    public async Task<List<RestaurantOrder>> GetActiveKitchenOrdersAsync()
    {
        return await _context.RestaurantOrders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.Status != RestaurantOrderStatus.Paid && o.Status != RestaurantOrderStatus.Cancelled)
            .OrderBy(o => o.OrderTime)
            .ToListAsync();
    }

    public async Task<List<RestaurantOrder>> GetServedOrdersForCashierAsync()
    {
        return await _context.RestaurantOrders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.Status == RestaurantOrderStatus.Served)
            .OrderBy(o => o.OrderTime)
            .ToListAsync();
    }

    public async Task<RestaurantOrder?> GetOrderByIdAsync(int id)
    {
        return await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<(bool Success, string ErrorMessage, RestaurantOrder? Order)> CreateOrderAsync(
        int tableId, int waiterId, string? customerName, string? notes, int[] menuItemIds, int[] quantities)
    {
        if (menuItemIds == null || menuItemIds.Length == 0)
            return (false, "Add at least one menu item.", null);

        var table = await _context.RestaurantTables.FindAsync(tableId);
        if (table == null)
            return (false, "Selected table not found.", null);

        var order = new RestaurantOrder
        {
            TableId = tableId,
            WaiterId = waiterId,
            CustomerName = customerName,
            Notes = notes,
            Status = RestaurantOrderStatus.Pending,
            OrderTime = DateTime.Now
        };

        decimal total = 0;
        for (int i = 0; i < menuItemIds.Length; i++)
        {
            if (quantities[i] <= 0) continue;
            var menuItem = await _context.RestaurantMenus.FindAsync(menuItemIds[i]);
            if (menuItem == null || !menuItem.IsAvailable) continue;

            order.Items.Add(new RestaurantOrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = quantities[i],
                UnitPrice = menuItem.Price
            });
            total += menuItem.Price * quantities[i];
        }

        if (!order.Items.Any())
            return (false, "No valid items in order.", null);

        order.TotalAmount = total;
        table.Status = TableStatus.Occupied;

        _context.RestaurantOrders.Add(order);
        await _context.SaveChangesAsync();

        return (true, string.Empty, order);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, RestaurantOrderStatus status)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return false;

        var allowed = order.Status switch
        {
            RestaurantOrderStatus.Pending => status == RestaurantOrderStatus.Preparing,
            RestaurantOrderStatus.Preparing => status == RestaurantOrderStatus.Ready,
            RestaurantOrderStatus.Ready => status == RestaurantOrderStatus.Served,
            RestaurantOrderStatus.Served => status == RestaurantOrderStatus.Served,
            _ => false
        };

        if (!allowed && status != order.Status) return false;

        order.Status = status;
        if (status == RestaurantOrderStatus.Served && order.Table != null)
            order.Table.Status = TableStatus.Available;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProcessPaymentAsync(int orderId, PaymentMethod paymentMethod, decimal amountPaid)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null || order.Status != RestaurantOrderStatus.Served) return false;

        if (amountPaid < order.TotalAmount) return false;

        order.Status = RestaurantOrderStatus.Paid;
        order.PaymentMethod = paymentMethod;
        order.AmountPaid = amountPaid;
        order.ChangeAmount = amountPaid - order.TotalAmount;
        order.PaidAt = DateTime.Now;

        if (order.Table != null)
            order.Table.Status = TableStatus.Available;

        await _context.SaveChangesAsync();
        return true;
    }
}
