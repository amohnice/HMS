using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models.Restaurant;

public class RestaurantOrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int MenuItemId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal => UnitPrice * Quantity;

    public virtual RestaurantOrder Order { get; set; } = null!;
    public virtual RestaurantMenu MenuItem { get; set; } = null!;
}
