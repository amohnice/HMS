using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Restaurant;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager")]
public class RestaurantMenuController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantMenuController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.RestaurantMenus.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search) || m.Category.ToString().Contains(search));

        ViewBag.Search = search;
        return View(await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantMenu menu)
    {
        if (ModelState.IsValid)
        {
            menu.CreatedAt = DateTime.Now;
            _context.RestaurantMenus.Add(menu);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Menu item added.";
            return RedirectToAction(nameof(Index));
        }
        return View(menu);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var menu = await _context.RestaurantMenus.FindAsync(id);
        return menu == null ? NotFound() : View(menu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantMenu menu)
    {
        if (id != menu.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(menu);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Menu item updated.";
            return RedirectToAction(nameof(Index));
        }
        return View(menu);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var menu = await _context.RestaurantMenus.FindAsync(id);
        return menu == null ? NotFound() : View(menu);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var menu = await _context.RestaurantMenus.FindAsync(id);
        if (menu != null)
        {
            _context.RestaurantMenus.Remove(menu);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Menu item deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
