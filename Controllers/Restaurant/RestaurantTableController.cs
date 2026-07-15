using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.Restaurant;

namespace HMS.Controllers.Restaurant;

[Authorize(Roles = "Admin,Manager,Waiter")]
public class RestaurantTableController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantTableController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.RestaurantTables.OrderBy(t => t.TableNumber).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantTable table)
    {
        if (ModelState.IsValid)
        {
            _context.RestaurantTables.Add(table);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Table created.";
            return RedirectToAction(nameof(Index));
        }
        return View(table);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var table = await _context.RestaurantTables.FindAsync(id);
        return table == null ? NotFound() : View(table);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RestaurantTable table)
    {
        if (id != table.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(table);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Table updated.";
            return RedirectToAction(nameof(Index));
        }
        return View(table);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var table = await _context.RestaurantTables.FindAsync(id);
        return table == null ? NotFound() : View(table);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var table = await _context.RestaurantTables.FindAsync(id);
        if (table != null)
        {
            _context.RestaurantTables.Remove(table);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Table deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, TableStatus status)
    {
        var table = await _context.RestaurantTables.FindAsync(id);
        if (table == null) return NotFound();

        table.Status = status;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
