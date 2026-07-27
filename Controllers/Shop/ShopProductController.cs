using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HMS.Models.Shop;
using HMS.Services;

namespace HMS.Controllers.Shop;

[Authorize(Roles = "Admin,Manager,ShopCashier")]
public class ShopProductController : Controller
{
    private readonly IShopProductService _productService;

    public ShopProductController(IShopProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        ViewBag.Search = search;
        var products = await _productService.GetPaginatedProductsAsync(page, pageSize, search);
        return View(products);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShopProduct product)
    {
        if (ModelState.IsValid)
        {
            await _productService.CreateProductAsync(product);
            TempData["SuccessMessage"] = "Product added.";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return product == null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ShopProduct product)
    {
        if (id != product.Id) return NotFound();
        if (ModelState.IsValid)
        {
            var success = await _productService.UpdateProductAsync(product);
            if (!success) return NotFound();

            TempData["SuccessMessage"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return product == null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _productService.DeleteProductAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Product deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
