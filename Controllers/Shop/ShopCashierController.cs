using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HMS.Models;
using HMS.Models.Shop;
using HMS.Services;

namespace HMS.Controllers.Shop;

[Authorize(Roles = "Admin,Manager,ShopCashier")]
public class ShopCashierController : Controller
{
    private readonly IShopSaleService _saleService;
    private readonly IShopProductService _productService;

    public ShopCashierController(IShopSaleService saleService, IShopProductService productService)
    {
        _saleService = saleService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Products = await _productService.GetAllActiveProductsAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SearchProduct(string? barcode, string? term)
    {
        ShopProduct? product = null;
        if (!string.IsNullOrWhiteSpace(barcode))
            product = await _productService.GetProductByBarcodeAsync(barcode);
        else if (!string.IsNullOrWhiteSpace(term))
        {
            var products = await _productService.GetAllActiveProductsAsync();
            product = products.FirstOrDefault(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

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
        var cashierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, errorMessage, sale) = await _saleService.CheckoutAsync(cashierId, productIds, quantities, discount, tax, paymentMethod, amountPaid);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Receipt), new { id = sale!.Id });
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await _saleService.GetSaleWithDetailsAsync(id);
        return sale == null ? NotFound() : View(sale);
    }
}
