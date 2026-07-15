using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HMS.Data;
using HMS.Models;
using HMS.Models.Shop;

namespace HMS.Controllers.Shop;

[Authorize(Roles = "Admin,Manager,ShopCashier")]
public class ShopCashierController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShopCashierController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.Products = await _context.ShopProducts
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SearchProduct(string? barcode, string? term)
    {
        ShopProduct? product = null;
        if (!string.IsNullOrWhiteSpace(barcode))
            product = await _context.ShopProducts.FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);
        else if (!string.IsNullOrWhiteSpace(term))
            product = await _context.ShopProducts.FirstOrDefaultAsync(p => p.Name.Contains(term) && p.IsActive);

        if (product == null) return Json(new { success = false });
        return Json(new
        {
            success = true,
            product.Id,
            product.Name,
            product.Price,
            product.StockQuantity,
            product.Barcode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int[] productIds, int[] quantities, decimal discount, decimal tax, PaymentMethod paymentMethod, decimal amountPaid)
    {
        if (productIds == null || productIds.Length == 0)
        {
            TempData["ErrorMessage"] = "Cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        var cashierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sale = new ShopSale
        {
            CashierId = cashierId,
            SaleTime = DateTime.Now,
            Discount = discount,
            Tax = tax,
            PaymentMethod = paymentMethod,
            Status = ShopSaleStatus.Completed
        };

        decimal subTotal = 0;
        for (int i = 0; i < productIds.Length; i++)
        {
            if (quantities[i] <= 0) continue;
            var product = await _context.ShopProducts.FindAsync(productIds[i]);
            if (product == null || !product.IsActive || product.StockQuantity < quantities[i])
            {
                TempData["ErrorMessage"] = $"Insufficient stock for {product?.Name ?? "item"}.";
                return RedirectToAction(nameof(Index));
            }

            sale.Items.Add(new ShopSaleItem
            {
                ProductId = product.Id,
                Quantity = quantities[i],
                UnitPrice = product.Price
            });
            subTotal += product.Price * quantities[i];
            product.StockQuantity -= quantities[i];
        }

        if (!sale.Items.Any())
        {
            TempData["ErrorMessage"] = "No valid items in cart.";
            return RedirectToAction(nameof(Index));
        }

        sale.SubTotal = subTotal;
        sale.TotalAmount = subTotal - discount + tax;
        if (amountPaid < sale.TotalAmount)
        {
            TempData["ErrorMessage"] = "Amount paid is less than total.";
            return RedirectToAction(nameof(Index));
        }

        sale.AmountPaid = amountPaid;
        sale.ChangeAmount = amountPaid - sale.TotalAmount;

        _context.ShopSales.Add(sale);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Receipt), new { id = sale.Id });
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await _context.ShopSales
            .Include(s => s.Cashier)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
        return sale == null ? NotFound() : View(sale);
    }
}
