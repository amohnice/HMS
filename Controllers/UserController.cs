using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HMS.Models;
using HMS.Models.ViewModels;
using HMS.Services;
using System.Security.Claims;

namespace HMS.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(string? search, string? roleFilter)
    {
        ViewBag.Search = search;
        ViewBag.RoleFilter = roleFilter ?? "All";
        ViewBag.AvailableRoles = new[] { "All", "SuperAdmin", "Admin", "Manager", "Waiter", "Kitchen", "Cashier", "ShopCashier" };

        var users = await _userService.GetAllUsersAsync(search, roleFilter);
        return View(users);
    }

    public IActionResult Create()
    {
        ViewBag.AvailableRoles = new[] { "Admin", "Manager", "Waiter", "Kitchen", "Cashier", "ShopCashier", "SuperAdmin" };
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        ViewBag.AvailableRoles = new[] { "Admin", "Manager", "Waiter", "Kitchen", "Cashier", "ShopCashier", "SuperAdmin" };

        if (!model.AutoGeneratePassword && string.IsNullOrWhiteSpace(model.CustomPassword))
        {
            ModelState.AddModelError("CustomPassword", "Please provide a password or check auto-generate.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var newUser = new User
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Role = model.Role
        };

        string? customPass = model.AutoGeneratePassword ? null : model.CustomPassword;
        var (success, password, errorMessage) = await _userService.CreateUserWithCredentialsAsync(newUser, customPass);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return View(model);
        }

        TempData["SuccessMessage"] = $"Staff account for '{newUser.FullName}' created successfully.";
        TempData["TempPass"] = password;

        return RedirectToAction(nameof(CreatedCredentials), new { id = newUser.UserId, isNew = true });
    }

    public async Task<IActionResult> CreatedCredentials(int id, bool isNew = true)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        string tempPassword = TempData["TempPass"]?.ToString() ?? "********";

        var scheme = Request.Scheme ?? "http";
        var host = Request.Host.Value ?? "localhost";
        var loginUrl = $"{scheme}://{host}/Account/Login";

        var viewModel = new UserCredentialsViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            TemporaryPassword = tempPassword,
            LoginUrl = loginUrl,
            IsNewUser = isNew
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.AvailableRoles = new[] { "Admin", "Manager", "Waiter", "Kitchen", "Cashier", "ShopCashier", "SuperAdmin" };

        var model = new EditUserViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        ViewBag.AvailableRoles = new[] { "Admin", "Manager", "Waiter", "Kitchen", "Cashier", "ShopCashier", "SuperAdmin" };

        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await _userService.GetUserByIdAsync(model.UserId);
        if (existingUser == null) return NotFound();

        existingUser.FullName = model.FullName;
        existingUser.Email = model.Email;
        existingUser.Role = model.Role;

        var result = await _userService.UpdateUserAsync(existingUser);
        if (result)
        {
            TempData["SuccessMessage"] = "Staff details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Failed to update staff details.";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        var (success, newPassword) = await _userService.ResetPasswordAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to reset password.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"Password reset successfully for {user.FullName}.";
        TempData["TempPass"] = newPassword;

        return RedirectToAction(nameof(CreatedCredentials), new { id = user.UserId, isNew = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(currentUserIdStr, out int currentUserId) && currentUserId == id)
        {
            TempData["ErrorMessage"] = "You cannot deactivate your own logged-in account.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userService.ToggleUserStatusAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Account status updated.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update account status.";
        }

        return RedirectToAction(nameof(Index));
    }
}
