using System.ComponentModel.DataAnnotations;

namespace ClawFlgma.AuthService.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [StringLength(50)]
    public string? TimeZone { get; set; } = "UTC";
}
