using System.ComponentModel.DataAnnotations;

namespace HMS.Models.Shop;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public virtual ShopProduct Product { get; set; } = null!;

    public int QuantityDelta { get; set; }

    public StockMovementType MovementType { get; set; }

    [StringLength(250)]
    public string? Reason { get; set; }

    public int? PerformedByUserId { get; set; }
    public virtual User? PerformedByUser { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;
}
