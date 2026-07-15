using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HMS.Data;
using HMS.Models;
using HMS.Services;

namespace HMS.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public AccountController(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            TempData["ErrorMessage"] = "Email and password are required.";
            return View();
        }

        var user = await _userService.AuthenticateAsync(email, password);
        if (user == null || !user.IsActive)
        {
            TempData["ErrorMessage"] = "Invalid email or password.";
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("CookieAuth", principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();
}
