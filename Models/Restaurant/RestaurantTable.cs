using System.ComponentModel.DataAnnotations;

namespace HMS.Models.Restaurant;

public class RestaurantTable
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Table Number")]
    public string TableNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 50)]
    public int Capacity { get; set; }

    [Required]
    public TableStatus Status { get; set; } = TableStatus.Available;

    public virtual ICollection<RestaurantOrder> Orders { get; set; } = new List<RestaurantOrder>();
}
