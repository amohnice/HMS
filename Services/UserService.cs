using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;

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
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
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

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
