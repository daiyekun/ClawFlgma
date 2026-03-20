using ClawFlgma.Shared;

namespace ClawFlgma.AuthService.Models;

/// <summary>
/// 应用用户模型 - 使用雪花算法生成的long类型ID
/// </summary>
public class ApplicationUser : IdentityUserLong
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? TimeZone { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TwoFactorEnabledMethod { get; set; }

    public string FullName => string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrEmpty(x)));
}
