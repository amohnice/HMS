using HMS.Models;

namespace HMS.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<bool> RegisterUserAsync(User user, string password);
    string HashPassword(string password);
}
