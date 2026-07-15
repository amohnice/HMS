using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HMS.Data;

namespace HMS.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.MenuCount = await _context.RestaurantMenus.CountAsync();
        ViewBag.TableCount = await _context.RestaurantTables.CountAsync();
        ViewBag.PendingOrders = await _context.RestaurantOrders.CountAsync(o => o.Status == Models.RestaurantOrderStatus.Pending);
        ViewBag.ProductCount = await _context.ShopProducts.CountAsync();
        ViewBag.LowStock = await _context.ShopProducts.CountAsync(p => p.StockQuantity <= 10);
        return View();
    }
}
