using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HMS.Models;
using HMS.Models.Restaurant;
using HMS.Services;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager")]
public class RestaurantMenuController : Controller
{
    private readonly IRestaurantMenuService _menuService;

    public RestaurantMenuController(IRestaurantMenuService menuService)
    {
        _menuService = menuService;
    }

    public async Task<IActionResult> Index(string? search, RestaurantMenuCategory? category, int page = 1, int pageSize = 10)
    {
        ViewBag.Search = search;
        ViewBag.CategoryFilter = category;
        ViewBag.CategoryCounts = await _menuService.GetCategoryCountsAsync(search);

        var menus = await _menuService.GetPaginatedMenusAsync(page, pageSize, search, category);
        return View(menus);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantMenu menu)
    {
        if (ModelState.IsValid)
        {
            await _menuService.CreateMenuAsync(menu);
            TempData["SuccessMessage"] = "Menu item added.";
            return RedirectToAction(nameof(Index));
        }
        return View(menu);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var menu = await _menuService.GetMenuByIdAsync(id);
        return menu == null ? NotFound() : View(menu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantMenu menu)
    {
        if (id != menu.Id) return NotFound();
        if (ModelState.IsValid)
        {
            var success = await _menuService.UpdateMenuAsync(menu);
            if (!success) return NotFound();

            TempData["SuccessMessage"] = "Menu item updated.";
            return RedirectToAction(nameof(Index));
        }
        return View(menu);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var menu = await _menuService.GetMenuByIdAsync(id);
        return menu == null ? NotFound() : View(menu);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _menuService.DeleteMenuAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Menu item deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
