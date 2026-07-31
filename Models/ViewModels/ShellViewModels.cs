using HMS.Services.Navigation;

namespace HMS.Models.ViewModels;

/// <summary>
/// Everything the shell partials need, resolved once in _Layout and passed down
/// so no partial reaches back into the service or the claims principal.
/// </summary>
public class ShellViewModel
{
    public string Role { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Initial => string.IsNullOrEmpty(UserName) ? "?" : UserName[..1].ToUpper();

    public string? Controller { get; set; }
    public string? Action { get; set; }

    public IReadOnlyList<NavItem> Items { get; set; } = [];
    public IReadOnlyDictionary<string, int> Badges { get; set; } = new Dictionary<string, int>();

    /// <summary>Kitchen runs a dark fullscreen display — the one deliberate palette exception.</summary>
    public bool IsKds => Role == "Kitchen";

    public int? BadgeFor(NavItem item) =>
        item.BadgeKey is not null && Badges.TryGetValue(item.BadgeKey, out var n) && n > 0 ? n : null;

    public bool IsActive(NavItem item) => item.Matches(Controller, Action);
}
