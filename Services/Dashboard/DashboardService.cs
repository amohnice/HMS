using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.ViewModels;
using HMS.Services.Ui;

namespace HMS.Services.Dashboard;

/// <summary>
/// Every dashboard query in one place. The old dashboard showed five counts;
/// this one answers "what needs me right now", which is what the counts were
/// standing in for.
/// </summary>
public class DashboardService : IDashboardService
{
    /// <summary>How long an order may sit in a state before the dashboard nags.</summary>
    private const int StaleKitchenMinutes = 15;
    private const int StaleReadyMinutes = 5;
    private const int StaleUnpaidMinutes = 20;

    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context) => _context = context;

    public async Task<DashboardViewModel> BuildAsync(string? role, string userName)
    {
        var isManagement = role is "Admin" or "Manager" or "SuperAdmin";

        var vm = new DashboardViewModel
        {
            Role = role ?? "",
            UserName = userName,
            IsManagement = isManagement,
            IsRestaurant = isManagement || role is "Waiter" or "Kitchen" or "Cashier",
            IsShop = isManagement || role == "ShopCashier"
        };

        if (vm.IsRestaurant) await AddRestaurantAsync(vm, role, isManagement);
        if (vm.IsShop) await AddShopAsync(vm, role, isManagement);

        // Longest wait first — the dashboard is a queue, not a list.
        vm.NeedsAttention = vm.NeedsAttention.OrderByDescending(a => a.Minutes).Take(8).ToList();

        return vm;
    }

    private async Task AddRestaurantAsync(DashboardViewModel vm, string? role, bool isManagement)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var liveOrders = await _context.RestaurantOrders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Items)
            .Where(o => o.Status != RestaurantOrderStatus.Paid && o.Status != RestaurantOrderStatus.Cancelled)
            .OrderBy(o => o.OrderTime)
            .ToListAsync();

        vm.LiveOrders = new OrderQueue
        {
            Orders = liveOrders,
            HrefFormat = role switch
            {
                "Kitchen" => "/Kitchen",
                "Cashier" => "/RestaurantCashier",
                _         => "/RestaurantOrder/Details/{0}"
            }
        };

        var tables = await _context.RestaurantTables.AsNoTracking().ToListAsync();
        vm.BusyTables = tables.Where(t => t.Status != TableStatus.Available).OrderBy(t => t.TableNumber).ToList();

        // RestaurantTableController is Admin/Manager/Waiter only, so kitchen and
        // cashier see the panel without a link they would be denied.
        if (isManagement || role == "Waiter") vm.TablesHref = "/RestaurantTable";

        // --- KPIs, tailored to what this role does about them ---
        if (role == "Kitchen" || isManagement)
        {
            var queue = liveOrders.Count(o => o.Status is RestaurantOrderStatus.Pending or RestaurantOrderStatus.Preparing);
            vm.Kpis.Add(new Kpi { Label = "In the kitchen", Value = queue.ToString(), Sub = "Tickets to cook" });
        }

        if (role == "Waiter" || isManagement)
        {
            var ready = liveOrders.Count(o => o.Status == RestaurantOrderStatus.Ready);
            vm.Kpis.Add(new Kpi { Label = "Ready to serve", Value = ready.ToString(), Sub = "Waiting at the pass" });
        }

        if (role == "Cashier" || isManagement)
        {
            var unpaid = liveOrders.Where(o => o.Status == RestaurantOrderStatus.Served).ToList();
            vm.Kpis.Add(new Kpi
            {
                Label = "Awaiting payment",
                Value = unpaid.Count.ToString(),
                Sub = $"Ksh {unpaid.Sum(o => o.TotalAmount):N0} due"
            });
        }

        vm.Kpis.Add(new Kpi
        {
            Label = "Tables occupied",
            Value = $"{tables.Count(t => t.Status == TableStatus.Occupied)}/{tables.Count}",
            Sub = "On the floor now"
        });

        if (isManagement || role == "Cashier")
        {
            var takings = await _context.RestaurantOrders
                .AsNoTracking()
                .Where(o => o.Status == RestaurantOrderStatus.Paid && o.PaidAt >= today && o.PaidAt < tomorrow)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            vm.Kpis.Add(new Kpi { Label = "Restaurant today", Value = $"Ksh {takings:N0}", Sub = "Settled bills" });
        }

        // --- Things that have waited too long ---
        var now = DateTime.Now;

        // Each role gets sent to the page it can actually act on. Order details is
        // waiter/admin only, so kitchen goes to the board and cashier to the till.
        string HrefFor(int orderId) => role switch
        {
            "Kitchen" => "/Kitchen",
            "Cashier" => "/RestaurantCashier",
            _         => $"/RestaurantOrder/Details/{orderId}"
        };

        foreach (var order in liveOrders)
        {
            var minutes = (int)(now - order.OrderTime).TotalMinutes;

            var threshold = order.Status switch
            {
                RestaurantOrderStatus.Pending or RestaurantOrderStatus.Preparing => StaleKitchenMinutes,
                RestaurantOrderStatus.Ready => StaleReadyMinutes,
                RestaurantOrderStatus.Served => StaleUnpaidMinutes,
                _ => int.MaxValue
            };

            if (minutes < threshold) continue;

            vm.NeedsAttention.Add(new AttentionItem
            {
                Title = $"Order #{order.Id:D4} · Table {order.Table?.TableNumber}",
                Detail = order.Status switch
                {
                    RestaurantOrderStatus.Pending   => "Not started in the kitchen",
                    RestaurantOrderStatus.Preparing => "Still cooking",
                    RestaurantOrderStatus.Ready     => "Ready but not collected",
                    _                               => "Served but not paid"
                },
                Minutes = minutes,
                Pill = StatusStyle.From(order.Status),
                Href = HrefFor(order.Id)
            });
        }
    }

    private async Task AddShopAsync(DashboardViewModel vm, string? role, bool isManagement)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var sales = await _context.ShopSales
            .AsNoTracking()
            .Where(s => s.SaleTime >= today && s.SaleTime < tomorrow)
            .ToListAsync();

        vm.Kpis.Add(new Kpi
        {
            Label = "Shop today",
            Value = $"Ksh {sales.Sum(s => s.TotalAmount):N0}",
            Sub = sales.Count == 1 ? "1 sale" : $"{sales.Count} sales"
        });

        // ShopProductController admits Admin/Manager/ShopCashier — every role that
        // reaches this method — so the link is always safe to show.
        vm.ProductsHref = "/ShopProduct";

        vm.LowStock = await _context.ShopProducts
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity <= 10)
            .OrderBy(p => p.StockQuantity)
            .Take(8)
            .ToListAsync();

        if (vm.LowStock.Count > 0)
        {
            vm.Kpis.Add(new Kpi
            {
                Label = "Needs reordering",
                Value = vm.LowStock.Count.ToString(),
                Sub = vm.LowStock.Count(p => p.StockQuantity <= 0) is var out_ && out_ > 0
                    ? $"{out_} out of stock"
                    : "Running low"
            });
        }
    }
}
