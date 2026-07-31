using HMS.Models.Restaurant;
using HMS.Models.Shop;

namespace HMS.Models.ViewModels;

public class Kpi
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Sub { get; set; }
}

/// <summary>
/// An order that has been sitting in one state too long. Surfaced on the dashboard
/// so nobody has to go looking for it.
/// </summary>
public class AttentionItem
{
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public int Minutes { get; set; }
    public string Href { get; set; } = "#";
    public StatusPill? Pill { get; set; }
}

/// <summary>
/// Orders for the queue strip, plus where a card should take this role.
/// </summary>
public class OrderQueue
{
    public List<RestaurantOrder> Orders { get; set; } = [];

    /// <summary>
    /// Destination for a card. Use "{0}" for the order id, e.g.
    /// "/RestaurantOrder/Details/{0}". A URL with no placeholder sends every card
    /// to the same page — which is what kitchen and cashier need, since order
    /// details is waiter/admin only. Null renders the cards as plain text.
    /// </summary>
    public string? HrefFormat { get; set; }

    public string? HrefFor(int orderId) =>
        HrefFormat is null ? null : string.Format(HrefFormat, orderId);
}

public class DashboardViewModel
{
    public string Role { get; set; } = "";
    public string UserName { get; set; } = "";

    public bool IsRestaurant { get; set; }
    public bool IsShop { get; set; }
    public bool IsManagement { get; set; }

    public List<Kpi> Kpis { get; set; } = [];

    /// <summary>Live floor state for the queue strip.</summary>
    public OrderQueue LiveOrders { get; set; } = new();

    /// <summary>Anything waiting longer than it should be.</summary>
    public List<AttentionItem> NeedsAttention { get; set; } = [];

    public List<ShopProduct> LowStock { get; set; } = [];
    public List<RestaurantTable> BusyTables { get; set; } = [];

    /// <summary>
    /// Null when this role may not open the tables page. The dashboard must never
    /// link somewhere the role would be denied — RestaurantTable is waiter/admin only.
    /// </summary>
    public string? TablesHref { get; set; }

    /// <summary>Likewise: the products page is shop/admin only.</summary>
    public string? ProductsHref { get; set; }

    public string Greeting =>
        DateTime.Now.Hour < 12 ? "Good morning" :
        DateTime.Now.Hour < 18 ? "Good afternoon" : "Good evening";

    /// <summary>
    /// The whole name, deliberately. Taking the first word turns accounts named
    /// after their job ("Restaurant Waiter") into "Good afternoon, Restaurant".
    /// </summary>
    public string DisplayName => string.IsNullOrWhiteSpace(UserName) ? "there" : UserName;
}
