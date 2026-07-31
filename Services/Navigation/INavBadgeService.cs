namespace HMS.Services.Navigation;

public interface INavBadgeService
{
    /// <summary>
    /// Counts for the rail badges, restricted to the keys the given role actually displays.
    /// Missing keys simply render no badge.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> GetBadgesAsync(string? role);
}
