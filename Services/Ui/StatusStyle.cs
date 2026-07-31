using HMS.Models;
using HMS.Models.ViewModels;

namespace HMS.Services.Ui;

/// <summary>
/// The single mapping from domain state to pill styling. These switches used to be
/// copy-pasted into five views, which is how a status ends up styled two ways.
/// Hue is meaningful here — this is the one place a second colour is allowed.
/// </summary>
public static class StatusStyle
{
    public static StatusPill From(RestaurantOrderStatus status) => status switch
    {
        RestaurantOrderStatus.Pending   => new("Pending",   "order-pending"),
        RestaurantOrderStatus.Preparing => new("Preparing", "order-preparing"),
        RestaurantOrderStatus.Ready     => new("Ready",     "order-ready"),
        RestaurantOrderStatus.Served    => new("Served",    "order-served"),
        RestaurantOrderStatus.Paid      => new("Paid",      "order-paid"),
        RestaurantOrderStatus.Cancelled => new("Cancelled", "order-cancelled"),
        _                               => new(status.ToString(), "order-cancelled")
    };

    public static StatusPill From(TableStatus status) => status switch
    {
        TableStatus.Available => new("Available", "status-available"),
        TableStatus.Occupied  => new("Occupied",  "status-occupied"),
        TableStatus.Reserved  => new("Reserved",  "status-reserved"),
        TableStatus.Cleaning  => new("Cleaning",  "status-cleaning"),
        _                     => new(status.ToString(), "status-cleaning")
    };

    public static StatusPill From(StockMovementType type) => type switch
    {
        StockMovementType.Sale             => new("Sale",       "order-served"),
        StockMovementType.Restock          => new("Restock",    "status-available"),
        StockMovementType.ManualAdjustment => new("Adjustment", "order-preparing"),
        StockMovementType.DamageSpoilage   => new("Damage",     "status-occupied"),
        StockMovementType.Return           => new("Return",     "order-pending"),
        _                                  => new(type.ToString(), "order-cancelled")
    };

    public static StatusPill Availability(bool isAvailable, string yes = "Available", string no = "Unavailable") =>
        isAvailable ? new(yes, "status-available") : new(no, "status-occupied");

    /// <summary>Stock reads as three states, so it gets three colours rather than a number alone.</summary>
    public static StatusPill Stock(int quantity, int lowThreshold = 10) => quantity switch
    {
        <= 0                        => new("Out of stock", "status-occupied"),
        var q when q <= lowThreshold => new($"Low · {quantity}", "order-pending"),
        _                            => new($"{quantity} in stock", "status-available")
    };
}
