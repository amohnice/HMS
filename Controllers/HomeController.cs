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
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var isAdminOrManager = userRole == "Admin" || userRole == "Manager" || userRole == "SuperAdmin";
        var isRestaurantRole = isAdminOrManager || userRole == "Waiter" || userRole == "Kitchen" || userRole == "Cashier";
        var isShopRole = isAdminOrManager || userRole == "ShopCashier";

        // Restaurant stats - only for restaurant roles
        if (isRestaurantRole)
        {
            ViewBag.MenuCount = await _context.RestaurantMenus.CountAsync();
            ViewBag.TableCount = await _context.RestaurantTables.CountAsync();
            ViewBag.PendingOrders = await _context.RestaurantOrders.CountAsync(o => o.Status == Models.RestaurantOrderStatus.Pending);
        }

        // Shop stats - only for shop roles
        if (isShopRole)
        {
            ViewBag.ProductCount = await _context.ShopProducts.CountAsync();
            ViewBag.LowStock = await _context.ShopProducts.CountAsync(p => p.StockQuantity <= 10);
        }

        // Pass role flags to view for conditional rendering
        ViewBag.IsRestaurantRole = isRestaurantRole;
        ViewBag.IsShopRole = isShopRole;

        return View();
    }
}
