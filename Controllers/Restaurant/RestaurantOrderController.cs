using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HMS.Data;
using HMS.Models;
using HMS.Models.Restaurant;
using HMS.Models.ViewModels;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager,Waiter")]
public class RestaurantOrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantOrderController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var orders = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .OrderByDescending(o => o.OrderTime)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Tables = await _context.RestaurantTables
            .Where(t => t.Status == TableStatus.Available || t.Status == TableStatus.Occupied)
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
        ViewBag.MenuItems = await _context.RestaurantMenus
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int tableId, string? customerName, string? notes, int[] menuItemIds, int[] quantities)
    {
        if (menuItemIds == null || menuItemIds.Length == 0)
        {
            TempData["ErrorMessage"] = "Add at least one menu item.";
            return RedirectToAction(nameof(Create));
        }

        var table = await _context.RestaurantTables.FindAsync(tableId);
        if (table == null) return NotFound();

        var waiterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
        {
            TempData["ErrorMessage"] = "No valid items in order.";
            return RedirectToAction(nameof(Create));
        }

        order.TotalAmount = total;
        table.Status = TableStatus.Occupied;

        _context.RestaurantOrders.Add(order);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Order #{order.Id} submitted to kitchen.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order == null ? NotFound() : View(order);
    }
}
