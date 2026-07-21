using System.ComponentModel.DataAnnotations;

namespace HMS.Models.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "Waiter";

    [Display(Name = "Auto-generate secure password")]
    public bool AutoGeneratePassword { get; set; } = true;

    [DataType(DataType.Password)]
    [Display(Name = "Custom Initial Password")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? CustomPassword { get; set; }
}

public class EditUserViewModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "Waiter";

    public bool IsActive { get; set; }
}

public class UserCredentialsViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public string LoginUrl { get; set; } = string.Empty;
    public bool IsNewUser { get; set; } = true;
}
