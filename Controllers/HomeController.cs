using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.ViewModels;
using HMS.Services.Dashboard;

namespace HMS.Controllers;

[Authorize]
public class HomeController : Controller
{
    private const int PerKind = 6;

    private readonly ApplicationDbContext _context;
    private readonly IDashboardService _dashboard;

    public HomeController(ApplicationDbContext context, IDashboardService dashboard)
    {
        _context = context;
        _dashboard = dashboard;
    }

    public async Task<IActionResult> Index()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return View(await _dashboard.BuildAsync(role, User.Identity?.Name ?? "there"));
    }

    /// <summary>
    /// Global search behind the top bar. Scoped to what the role may actually open,
    /// so results never advertise a page the user would then be denied.
    /// </summary>
    public async Task<IActionResult> Search(string? q)
    {
        var vm = new SearchResultsViewModel { Query = (q ?? "").Trim() };

        if (vm.Query.Length < 2)
            return View(vm);

        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var isAdminOrManager = role is "Admin" or "Manager" or "SuperAdmin";
        var term = vm.Query;

        if (isAdminOrManager || role is "Waiter" or "Cashier" or "Kitchen")
        {
            var dishes = await _context.RestaurantMenus
                .Where(m => m.Name.Contains(term))
                .OrderBy(m => m.Name)
                .Take(PerKind)
                .ToListAsync();

            vm.Hits.AddRange(dishes.Select(m => new SearchHit
            {
                Kind = "Dish", Icon = "book", Title = m.Name,
                Meta = $"{m.Category} · Ksh {m.Price:N0}",
                Controller = "RestaurantMenu", Action = "Index"
            }));

            var tables = await _context.RestaurantTables
                .Where(t => t.TableNumber.Contains(term))
                .OrderBy(t => t.TableNumber)
                .Take(PerKind)
                .ToListAsync();

            vm.Hits.AddRange(tables.Select(t => new SearchHit
            {
                Kind = "Table", Icon = "table", Title = $"Table {t.TableNumber}",
                Meta = $"{t.Status} · seats {t.Capacity}",
                Controller = "RestaurantTable", Action = "Index"
            }));

            // Orders match on customer name, or on the order number itself.
            var byNumber = int.TryParse(term, out var orderId) ? orderId : (int?)null;

            var orders = await _context.RestaurantOrders
                .Include(o => o.Table)
                .Where(o => (o.CustomerName != null && o.CustomerName.Contains(term))
                            || (byNumber != null && o.Id == byNumber))
                .OrderByDescending(o => o.OrderTime)
                .Take(PerKind)
                .ToListAsync();

            vm.Hits.AddRange(orders.Select(o => new SearchHit
            {
                Kind = "Order", Icon = "list",
                Title = $"Order #{o.Id:D4}" + (string.IsNullOrWhiteSpace(o.CustomerName) ? "" : $" · {o.CustomerName}"),
                Meta = $"Table {o.Table.TableNumber} · {o.Status} · Ksh {o.TotalAmount:N0}",
                Controller = "RestaurantOrder", Action = "Details", Id = o.Id
            }));
        }

        if (isAdminOrManager || role == "ShopCashier")
        {
            var products = await _context.ShopProducts
                .Where(p => p.Name.Contains(term)
                            || p.Category.Contains(term)
                            || (p.Barcode != null && p.Barcode.Contains(term)))
                .OrderBy(p => p.Name)
                .Take(PerKind)
                .ToListAsync();

            vm.Hits.AddRange(products.Select(p => new SearchHit
            {
                Kind = "Product", Icon = "box", Title = p.Name,
                Meta = $"{p.Category} · Ksh {p.Price:N0} · {p.StockQuantity} in stock",
                Controller = "ShopProduct", Action = "Index"
            }));
        }

        if (role is "Admin" or "SuperAdmin")
        {
            var staff = await _context.Users
                .Where(u => u.FullName.Contains(term) || u.Email.Contains(term))
                .OrderBy(u => u.FullName)
                .Take(PerKind)
                .ToListAsync();

            vm.Hits.AddRange(staff.Select(u => new SearchHit
            {
                Kind = "Staff", Icon = "users", Title = u.FullName,
                Meta = $"{u.Role} · {u.Email}",
                Controller = "User", Action = "Index"
            }));
        }

        return View(vm);
    }
}
