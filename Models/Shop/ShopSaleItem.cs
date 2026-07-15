using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models.Shop;

public class ShopSaleItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SaleId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal => UnitPrice * Quantity;

    public virtual ShopSale Sale { get; set; } = null!;
    public virtual ShopProduct Product { get; set; } = null!;
}
