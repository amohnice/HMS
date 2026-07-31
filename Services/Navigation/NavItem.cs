namespace HMS.Services.Navigation;

/// <summary>
/// One entry in the rail. Icon is a key into _Icon.cshtml, not raw markup,
/// so each SVG path lives in exactly one place.
/// </summary>
public class NavItem
{
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "Index";

    /// <summary>Key into the badge dictionary. Null means no count is shown.</summary>
    public string? BadgeKey { get; set; }

    /// <summary>Section heading rendered above this item. Null continues the current group.</summary>
    public string? GroupLabel { get; set; }

    /// <summary>
    /// Extra controllers that should light this item up, e.g. Kitchen and RestaurantCashier
    /// both sit under the Order Line entry for admins.
    /// </summary>
    public string[] AlsoMatches { get; set; } = [];

    /// <summary>
    /// Set when two entries share a controller (Take Order vs All Orders) and only the
    /// action distinguishes them. Otherwise any action on the controller matches.
    /// </summary>
    public bool MatchOnAction { get; set; }

    public bool Matches(string? controller, string? action)
    {
        if (string.IsNullOrEmpty(controller)) return false;

        if (AlsoMatches.Contains(controller, StringComparer.OrdinalIgnoreCase))
            return true;

        if (!controller.Equals(Controller, StringComparison.OrdinalIgnoreCase))
            return false;

        return !MatchOnAction
            || string.Equals(action, Action, StringComparison.OrdinalIgnoreCase);
    }
}
