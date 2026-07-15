using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HMS.Data;
using HMS.Models;
using HMS.Models.Restaurant;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager,Cashier")]
public class RestaurantCashierController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantCashierController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var orders = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.Status == RestaurantOrderStatus.Served)
            .OrderBy(o => o.OrderTime)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Pay(int id)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id && o.Status == RestaurantOrderStatus.Served);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id, PaymentMethod paymentMethod, decimal amountPaid)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null || order.Status != RestaurantOrderStatus.Served)
            return NotFound();

        if (amountPaid < order.TotalAmount)
        {
            TempData["ErrorMessage"] = "Amount paid is less than total.";
            return RedirectToAction(nameof(Pay), new { id });
        }

        order.Status = RestaurantOrderStatus.Paid;
        order.PaymentMethod = paymentMethod;
        order.AmountPaid = amountPaid;
        order.ChangeAmount = amountPaid - order.TotalAmount;
        order.PaidAt = DateTime.Now;

        if (order.Table != null)
            order.Table.Status = TableStatus.Available;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Receipt), new { id });
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var order = await _context.RestaurantOrders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id && o.Status == RestaurantOrderStatus.Paid);
        return order == null ? NotFound() : View(order);
    }
}
