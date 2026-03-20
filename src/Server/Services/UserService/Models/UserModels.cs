using ClawFlgma.Shared;

namespace ClawFlgma.UserService.Models;

/// <summary>
/// 用户模型 - 使用雪花算法生成的long类型ID
/// </summary>
public class User : IdentityUserLong
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

    public string FullName => string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrEmpty(x)));
}

/// <summary>
/// 用户详细信息（用于 API 返回）
/// </summary>
public class UserProfile
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? TimeZone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 用户统计信息
/// </summary>
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int EmailVerifiedUsers { get; set; }
    public int EmailUnverifiedUsers { get; set; }
    public int UsersWithLastLogin7Days { get; set; }
    public int UsersWithLastLogin30Days { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
}

/// <summary>
/// 用户角色模型
/// </summary>
public class UserRole : IdentityRoleLong
{
    public string? Description { get; set; }
    public int Priority { get; set; }
}
