namespace HMS.Services.Navigation;

/// <summary>
/// The single source of truth for what each role sees in the rail.
/// Adding a role means adding a case here — never a new shell branch in _Layout.
/// Keep these entries in sync with the [Authorize(Roles = ...)] attributes on the
/// controllers; this decides visibility only, the attributes decide access.
/// </summary>
public static class NavigationManifest
{
    public const string BadgeMenuItems     = "menuItems";
    public const string BadgeTables        = "tables";
    public const string BadgeOpenOrders    = "openOrders";
    public const string BadgeKitchenQueue  = "kitchenQueue";
    public const string BadgeUnpaidOrders  = "unpaidOrders";
    public const string BadgeProducts      = "products";
    public const string BadgeLowStock      = "lowStock";
    public const string BadgeStaff         = "staff";

    private static NavItem Dashboard => new()
    {
        Label = "Dashboard", Icon = "grid", Controller = "Home", Action = "Index"
    };

    public static IReadOnlyList<NavItem> For(string? role) => role switch
    {
        "Kitchen"     => Kitchen(),
        "Waiter"      => Waiter(),
        "Cashier"     => Cashier(),
        "ShopCashier" => ShopCashier(),
        "Manager"     => Management(includeStaff: false),
        "Admin" or "SuperAdmin" => Management(includeStaff: true),
        _ => [Dashboard]
    };

    /// <summary>
    /// Kitchen runs a fullscreen display, so its rail is deliberately tiny.
    /// </summary>
    private static List<NavItem> Kitchen() =>
    [
        Dashboard,
        new() { Label = "Kitchen", Icon = "chef", Controller = "Kitchen", BadgeKey = BadgeKitchenQueue }
    ];

    private static List<NavItem> Waiter() =>
    [
        Dashboard,
        new() { Label = "Tables", Icon = "table", Controller = "RestaurantTable", BadgeKey = BadgeTables },
        new() { Label = "Take Order", Icon = "plus-doc", Controller = "RestaurantOrder", Action = "Create", MatchOnAction = true },
        new() { Label = "Orders", Icon = "list", Controller = "RestaurantOrder", Action = "Index", MatchOnAction = true, BadgeKey = BadgeOpenOrders }
    ];

    private static List<NavItem> Cashier() =>
    [
        Dashboard,
        new() { Label = "Register", Icon = "card", Controller = "RestaurantCashier", BadgeKey = BadgeUnpaidOrders },
        new() { Label = "Reports", Icon = "chart", Controller = "RestaurantReport" }
    ];

    private static List<NavItem> ShopCashier() =>
    [
        Dashboard,
        new() { Label = "Shop POS", Icon = "cart", Controller = "ShopCashier" },
        new() { Label = "Reports", Icon = "chart", Controller = "ShopReport" }
    ];

    private static List<NavItem> Management(bool includeStaff)
    {
        var items = new List<NavItem>
        {
            Dashboard,

            new() { GroupLabel = "Restaurant", Label = "Menu", Icon = "book", Controller = "RestaurantMenu", BadgeKey = BadgeMenuItems },
            new() { Label = "Tables", Icon = "table", Controller = "RestaurantTable", BadgeKey = BadgeTables },
            new() { Label = "Take Order", Icon = "plus-doc", Controller = "RestaurantOrder", Action = "Create", MatchOnAction = true },
            new() { Label = "Orders", Icon = "list", Controller = "RestaurantOrder", Action = "Index", MatchOnAction = true, BadgeKey = BadgeOpenOrders },
            new() { Label = "Kitchen", Icon = "chef", Controller = "Kitchen", BadgeKey = BadgeKitchenQueue },
            new() { Label = "Register", Icon = "card", Controller = "RestaurantCashier", BadgeKey = BadgeUnpaidOrders },
            new() { Label = "Reports", Icon = "chart", Controller = "RestaurantReport" },

            new() { GroupLabel = "Shop", Label = "Products", Icon = "box", Controller = "ShopProduct", BadgeKey = BadgeProducts, AlsoMatches = ["ShopProduct"] },
            new() { Label = "Shop POS", Icon = "cart", Controller = "ShopCashier" },
            new() { Label = "Reports", Icon = "bars", Controller = "ShopReport" }
        };

        if (includeStaff)
        {
            items.Add(new NavItem
            {
                GroupLabel = "System", Label = "Staff & Users", Icon = "users",
                Controller = "User", BadgeKey = BadgeStaff
            });
        }

        return items;
    }
}
