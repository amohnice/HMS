namespace HMS.Models;

public enum RestaurantMenuCategory
{
    Breakfast,
    Lunch,
    Dinner,
    Drinks
}

public enum TableStatus
{
    Available,
    Occupied,
    Reserved,
    Cleaning
}

public enum RestaurantOrderStatus
{
    Pending,
    Preparing,
    Ready,
    Served,
    Paid,
    Cancelled
}

public enum PaymentMethod
{
    Cash,
    Card,
    MPesa
}

public enum ShopSaleStatus
{
    Completed,
    Refunded
}

public enum StockMovementType
{
    Sale,
    Restock,
    ManualAdjustment,
    DamageSpoilage,
    Return
}
