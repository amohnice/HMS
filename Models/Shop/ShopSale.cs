using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models.Shop;

public class ShopSale
{
    [Key]
    public int Id { get; set; }

    public DateTime SaleTime { get; set; } = DateTime.Now;

    [Required]
    public int CashierId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Tax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ChangeAmount { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    public ShopSaleStatus Status { get; set; } = ShopSaleStatus.Completed;

    public virtual User Cashier { get; set; } = null!;
    public virtual ICollection<ShopSaleItem> Items { get; set; } = new List<ShopSaleItem>();
}
