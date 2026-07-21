using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using System.Security.Cryptography;
using System.Text;

namespace HMS.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<bool> RegisterUserAsync(User user, string password)
    {
        user.PasswordHash = HashPassword(password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetAllUsersAsync(string? search = null, string? roleFilter = null)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(searchLower) || u.Email.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All")
        {
            query = query.Where(u => u.Role == roleFilter);
        }

        return await query.OrderByDescending(u => u.RegisteredAt).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
    }

    public async Task<(bool Success, string Password, string ErrorMessage)> CreateUserWithCredentialsAsync(User user, string? customPassword = null)
    {
        var existing = await GetUserByEmailAsync(user.Email);
        if (existing != null)
        {
            return (false, string.Empty, "A user with this email already exists.");
        }

        string initialPassword = string.IsNullOrWhiteSpace(customPassword)
            ? GenerateTemporaryPassword()
            : customPassword.Trim();

        user.RegisteredAt = DateTime.Now;
        user.IsActive = true;
        user.PasswordHash = HashPassword(initialPassword);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, initialPassword, string.Empty);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        var existing = await _context.Users.FindAsync(user.UserId);
        if (existing == null) return false;

        existing.FullName = user.FullName.Trim();
        existing.Email = user.Email.Trim();
        existing.Role = user.Role;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string NewPassword)> ResetPasswordAsync(int userId, string? customPassword = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (false, string.Empty);

        string newPassword = string.IsNullOrWhiteSpace(customPassword)
            ? GenerateTemporaryPassword()
            : customPassword.Trim();

        user.PasswordHash = HashPassword(newPassword);
        await _context.SaveChangesAsync();

        return (true, newPassword);
    }

    public async Task<bool> ToggleUserStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public string GenerateTemporaryPassword(int length = 10)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$";

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var sb = new StringBuilder();
        sb.Append(upper[bytes[0] % upper.Length]);
        sb.Append(lower[bytes[1] % lower.Length]);
        sb.Append(digits[bytes[2] % digits.Length]);
        sb.Append(special[bytes[3] % special.Length]);

        const string allChars = upper + lower + digits + special;
        for (int i = 4; i < length; i++)
        {
            sb.Append(allChars[bytes[i] % allChars.Length]);
        }

        // Shuffle
        return new string(sb.ToString().ToCharArray().OrderBy(_ => bytes[0] % 17).ToArray());
    }

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}

