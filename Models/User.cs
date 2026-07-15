using System.ComponentModel.DataAnnotations;
using HMS.Models.Restaurant;


namespace HMS.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(20)]
    public string Role { get; set; } = "Waiter";

    public bool IsActive { get; set; } = true;

    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<RestaurantOrder> RestaurantOrders { get; set; } = new List<RestaurantOrder>();
}
