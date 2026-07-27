using HMS.Models;

namespace HMS.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<bool> RegisterUserAsync(User user, string password);
    string HashPassword(string password);

    Task<List<User>> GetAllUsersAsync(string? search = null, string? roleFilter = null);
    Task<HMS.Models.Common.PagedList<User>> GetPaginatedUsersAsync(int page, int pageSize, string? search = null, string? roleFilter = null);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<(bool Success, string Password, string ErrorMessage)> CreateUserWithCredentialsAsync(User user, string? customPassword = null);
    Task<bool> UpdateUserAsync(User user);
    Task<(bool Success, string NewPassword)> ResetPasswordAsync(int userId, string? customPassword = null);
    Task<bool> ToggleUserStatusAsync(int userId);
    string GenerateTemporaryPassword(int length = 10);
}

