using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Shop;

namespace HMS.Controllers.Shop;

[Authorize(Roles = "Admin,Manager,ShopCashier")]
public class ShopProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShopProductController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.ShopProducts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)));

        ViewBag.Search = search;
        return View(await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShopProduct product)
    {
        if (ModelState.IsValid)
        {
            product.CreatedAt = DateTime.Now;
            _context.ShopProducts.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product added.";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.ShopProducts.FindAsync(id);
        return product == null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ShopProduct product)
    {
        if (id != product.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.ShopProducts.FindAsync(id);
        return product == null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.ShopProducts.FindAsync(id);
        if (product != null)
        {
            _context.ShopProducts.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
