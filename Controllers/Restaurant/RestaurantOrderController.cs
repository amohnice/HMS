using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HMS.Data;
using HMS.Models;
using HMS.Models.Restaurant;
using HMS.Services;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager,Waiter")]
public class RestaurantOrderController : Controller
{
    private readonly IRestaurantOrderService _orderService;
    private readonly IRestaurantMenuService _menuService;
    private readonly ApplicationDbContext _context;

    public RestaurantOrderController(
        IRestaurantOrderService orderService,
        IRestaurantMenuService menuService,
        ApplicationDbContext context)
    {
        _orderService = orderService;
        _menuService = menuService;
        _context = context;
    }

    public async Task<IActionResult> Index(RestaurantOrderStatus? statusFilter, int page = 1, int pageSize = 10)
    {
        ViewBag.StatusFilter = statusFilter;
        var orders = await _orderService.GetPaginatedOrdersAsync(page, pageSize, statusFilter);
        return View(orders);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Tables = await _context.RestaurantTables
            .Where(t => t.Status == TableStatus.Available || t.Status == TableStatus.Occupied)
            .OrderBy(t => t.TableNumber)
            .ToListAsync();

        ViewBag.MenuItems = await _menuService.GetAvailableMenuItemsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int tableId, string? customerName, string? notes, int[] menuItemIds, int[] quantities)
    {
        var waiterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage, order) = await _orderService.CreateOrderAsync(tableId, waiterId, customerName, notes, menuItemIds, quantities);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Create));
        }

        TempData["SuccessMessage"] = $"Order #{order!.Id} submitted to kitchen.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return order == null ? NotFound() : View(order);
    }
}
