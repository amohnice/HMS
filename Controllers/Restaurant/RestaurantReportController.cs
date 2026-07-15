using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using HMS.Data;
using HMS.Models;
using HMS.Models.ViewModels;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager,Cashier")]
public class RestaurantReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantReportController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(ReportFilterViewModel? filter)
    {
        filter ??= new ReportFilterViewModel();
        ApplyPeriod(filter);

        var start = filter.StartDate.Date;
        var end = filter.EndDate.Date.AddDays(1).AddTicks(-1);

        var orders = await _context.RestaurantOrders
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.Status == RestaurantOrderStatus.Paid && o.PaidAt >= start && o.PaidAt <= end)
            .ToListAsync();

        var model = new RestaurantReportViewModel
        {
            Filter = filter,
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            TotalOrders = orders.Count,
            TopItems = orders.SelectMany(o => o.Items)
                .GroupBy(i => i.MenuItem.Name)
                .Select(g => new TopItemRow
                {
                    Name = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList(),
            TopWaiters = orders.GroupBy(o => o.Waiter.FullName)
                .Select(g => new TopWaiterRow
                {
                    Name = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> ExportCsv(ReportFilterViewModel filter)
    {
        ApplyPeriod(filter);
        var start = filter.StartDate.Date;
        var end = filter.EndDate.Date.AddDays(1).AddTicks(-1);

        var orders = await _context.RestaurantOrders
            .Include(o => o.Waiter)
            .Include(o => o.Table)
            .Where(o => o.Status == RestaurantOrderStatus.Paid && o.PaidAt >= start && o.PaidAt <= end)
            .OrderBy(o => o.PaidAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Table,Waiter,Total,PaymentMethod");
        foreach (var o in orders)
        {
            sb.AppendLine($"{o.Id},{o.PaidAt:yyyy-MM-dd HH:mm},{o.Table?.TableNumber},{o.Waiter?.FullName},{o.TotalAmount},{o.PaymentMethod}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"restaurant-sales-{start:yyyyMMdd}-{end:yyyyMMdd}.csv");
    }

    private static void ApplyPeriod(ReportFilterViewModel filter)
    {
        var today = DateTime.Today;
        switch (filter.Period?.ToLower())
        {
            case "weekly":
                filter.StartDate = today.AddDays(-(int)today.DayOfWeek);
                filter.EndDate = today;
                break;
            case "monthly":
                filter.StartDate = new DateTime(today.Year, today.Month, 1);
                filter.EndDate = today;
                break;
            default:
                filter.StartDate = today;
                filter.EndDate = today;
                break;
        }
    }
}
