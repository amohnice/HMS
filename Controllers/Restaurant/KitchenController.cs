using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.Restaurant;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager,Kitchen")]
public class KitchenController : Controller
{
    private readonly ApplicationDbContext _context;

    public KitchenController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var orders = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.Status != RestaurantOrderStatus.Paid && o.Status != RestaurantOrderStatus.Cancelled)
            .OrderBy(o => o.OrderTime)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, RestaurantOrderStatus status)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        var allowed = order.Status switch
        {
            RestaurantOrderStatus.Pending => status == RestaurantOrderStatus.Preparing,
            RestaurantOrderStatus.Preparing => status == RestaurantOrderStatus.Ready,
            RestaurantOrderStatus.Ready => status == RestaurantOrderStatus.Served,
            RestaurantOrderStatus.Served => status == RestaurantOrderStatus.Served,
            _ => false
        };

        if (!allowed && status != order.Status)
        {
            TempData["ErrorMessage"] = "Invalid status transition.";
            return RedirectToAction(nameof(Index));
        }

        order.Status = status;
        if (status == RestaurantOrderStatus.Served && order.Table != null)
            order.Table.Status = TableStatus.Available;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Order #{id} marked as {status}.";
        return RedirectToAction(nameof(Index));
    }
}
