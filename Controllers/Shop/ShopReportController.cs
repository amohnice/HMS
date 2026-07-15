using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using HMS.Data;
using HMS.Models;
using HMS.Models.Shop;
using HMS.Models.ViewModels;

namespace HMS.Controllers.Shop;

[Authorize(Roles = "Admin,Manager,ShopCashier")]
public class ShopReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int LowStockThreshold = 10;

    public ShopReportController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(ReportFilterViewModel? filter)
    {
        filter ??= new ReportFilterViewModel();
        ApplyPeriod(filter);

        var start = filter.StartDate.Date;
        var end = filter.EndDate.Date.AddDays(1).AddTicks(-1);

        var sales = await _context.ShopSales
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.Status == ShopSaleStatus.Completed && s.SaleTime >= start && s.SaleTime <= end)
            .ToListAsync();

        var model = new ShopReportViewModel
        {
            Filter = filter,
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalSales = sales.Count,
            TopProducts = sales.SelectMany(s => s.Items)
                .GroupBy(i => i.Product.Name)
                .Select(g => new TopItemRow
                {
                    Name = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList(),
            LowStockProducts = await _context.ShopProducts
                .Where(p => p.IsActive && p.StockQuantity <= LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> ExportCsv(ReportFilterViewModel filter)
    {
        ApplyPeriod(filter);
        var start = filter.StartDate.Date;
        var end = filter.EndDate.Date.AddDays(1).AddTicks(-1);

        var sales = await _context.ShopSales
            .Include(s => s.Cashier)
            .Where(s => s.Status == ShopSaleStatus.Completed && s.SaleTime >= start && s.SaleTime <= end)
            .OrderBy(s => s.SaleTime)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("SaleId,Date,Cashier,SubTotal,Discount,Tax,Total,PaymentMethod");
        foreach (var s in sales)
        {
            sb.AppendLine($"{s.Id},{s.SaleTime:yyyy-MM-dd HH:mm},{s.Cashier?.FullName},{s.SubTotal},{s.Discount},{s.Tax},{s.TotalAmount},{s.PaymentMethod}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"shop-sales-{start:yyyyMMdd}-{end:yyyyMMdd}.csv");
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
