using System.ComponentModel.DataAnnotations;

namespace ClawFlgma.AuthService.DTOs;

public class ChangePasswordDto
{
    [Required]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
