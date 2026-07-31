namespace HMS.Models.ViewModels;

/// <summary>
/// A button in a surface header or empty state. Declarative rather than a markup
/// slot, because Razor partials cannot take one — every page's action fits this shape.
/// </summary>
public class SurfaceAction
{
    public string Label { get; set; } = "";
    public string? Icon { get; set; }
    public string Href { get; set; } = "#";
    public string Css { get; set; } = "btn btn-primary btn-sm";

    /// <summary>Set to open the target in the side drawer instead of navigating.</summary>
    public bool OpensDrawer { get; set; }
    public string? DrawerTitle { get; set; }
}

/// <summary>Inline search that replaces the full-width search card each list page had.</summary>
public class SurfaceSearch
{
    public string Href { get; set; } = "";
    public string ParamName { get; set; } = "search";
    public string? Value { get; set; }
    public string Placeholder { get; set; } = "Search…";

    /// <summary>Query keys to carry through so searching does not drop the active filter.</summary>
    public Dictionary<string, string?> Preserve { get; set; } = [];
}

/// <summary>
/// Title · count · search · actions on one row. Replaces the heading + prose
/// subtitle + separate search card that each list page repeated.
/// </summary>
public class SurfaceHead
{
    public string Title { get; set; } = "";
    public string? Count { get; set; }
    public SurfaceSearch? Search { get; set; }
    public List<SurfaceAction> Actions { get; set; } = [];

    /// <summary>Renders the grid/list segmented control. Requires a [data-view-host] on the page.</summary>
    public bool ShowViewToggle { get; set; }
}

/// <summary>One row in the context column — a category, queue or preset, with its count.</summary>
public class ContextItem
{
    public string Label { get; set; } = "";
    public int? Count { get; set; }
    public string Href { get; set; } = "#";
    public bool IsActive { get; set; }
    public string? Icon { get; set; }
}

public class ContextColumn
{
    public string Title { get; set; } = "";
    public List<ContextItem> Items { get; set; } = [];

    /// <summary>Pinned action at the bottom, e.g. "Add New Category".</summary>
    public SurfaceAction? Foot { get; set; }
}

/// <summary>Segmented filter with a count, e.g. All 78 · Dine in 04 · Wait List 03.</summary>
public class FilterChip
{
    public string Label { get; set; } = "";
    public int? Count { get; set; }
    public string Href { get; set; } = "#";
    public bool IsActive { get; set; }
}

public class EmptyState
{
    public string Icon { get; set; } = "list";
    public string Title { get; set; } = "Nothing here yet";
    public string? Text { get; set; }
    public SurfaceAction? Action { get; set; }
}

/// <summary>Text plus the pill class that carries its meaning. Built by StatusPill.From.</summary>
public class StatusPill
{
    public string Text { get; set; } = "";
    public string Css { get; set; } = "";

    public StatusPill() { }

    public StatusPill(string text, string css)
    {
        Text = text;
        Css = css;
    }
}
