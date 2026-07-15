using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models.Restaurant;

public class RestaurantOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TableId { get; set; }

    [Required]
    public int WaiterId { get; set; }

    [StringLength(150)]
    public string? CustomerName { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime OrderTime { get; set; } = DateTime.Now;

    [Required]
    public RestaurantOrderStatus Status { get; set; } = RestaurantOrderStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime? PaidAt { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ChangeAmount { get; set; }

    public virtual RestaurantTable Table { get; set; } = null!;
    public virtual User Waiter { get; set; } = null!;
    public virtual ICollection<RestaurantOrderItem> Items { get; set; } = new List<RestaurantOrderItem>();
}
