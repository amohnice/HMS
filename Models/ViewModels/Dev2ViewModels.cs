using HMS.Models.Shop;

namespace HMS.Models.ViewModels;

public class RestaurantOrderCreateViewModel
{
    public int TableId { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public List<OrderLineItem> Items { get; set; } = new();
}

public class OrderLineItem
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}

public class ShopPosViewModel
{
    public List<ShopPosLineItem> Items { get; set; } = new();
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
}

public class ShopPosLineItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ReportFilterViewModel
{
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today;
    public string Period { get; set; } = "daily";
}

public class RestaurantReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public List<TopItemRow> TopItems { get; set; } = new();
    public List<TopWaiterRow> TopWaiters { get; set; } = new();
    public ReportFilterViewModel Filter { get; set; } = new();
}

public class ShopReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TotalSales { get; set; }
    public List<TopItemRow> TopProducts { get; set; } = new();
    public List<ShopProduct> LowStockProducts { get; set; } = new();
    public ReportFilterViewModel Filter { get; set; } = new();
}

public class TopItemRow
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class TopWaiterRow
{
    public string Name { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}
