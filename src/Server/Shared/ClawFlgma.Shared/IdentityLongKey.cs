using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClawFlgma.Shared;

/// <summary>
/// 支持long类型主键的Identity用户基类
/// </summary>
public class IdentityUserLong : IdentityUser<long>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 支持long类型主键的Identity角色基类
/// </summary>
public class IdentityRoleLong : IdentityRole<long>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 支持long类型主键的Identity角色关联
/// </summary>
public class IdentityUserRoleLong : IdentityUserRole<long>
{
}

/// <summary>
/// 支持long类型主键的Identity用户声明
/// </summary>
public class IdentityUserClaimLong : IdentityUserClaim<long>
{
}

/// <summary>
/// 支持long类型主键的Identity用户登录
/// </summary>
public class IdentityUserLoginLong : IdentityUserLogin<long>
{
}

/// <summary>
/// 支持long类型主键的Identity用户令牌
/// </summary>
public class IdentityUserTokenLong : IdentityUserToken<long>
{
}

/// <summary>
/// 支持long类型主键的Identity角色声明
/// </summary>
public class IdentityRoleClaimLong : IdentityRoleClaim<long>
{
}

/// <summary>
/// 使用long类型主键的Identity数据库上下文
/// </summary>
public class IdentityDbContextLong : IdentityDbContext<
    IdentityUserLong,
    IdentityRoleLong,
    long,
    IdentityUserClaimLong,
    IdentityUserRoleLong,
    IdentityUserLoginLong,
    IdentityRoleClaimLong,
    IdentityUserTokenLong>
{
    protected IdentityDbContextLong(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 配置表名为复数形式
        builder.Entity<IdentityUserLong>(b =>
        {
            b.ToTable("Users");
        });

        builder.Entity<IdentityRoleLong>(b =>
        {
            b.ToTable("Roles");
        });

        builder.Entity<IdentityUserRoleLong>(b =>
        {
            b.ToTable("UserRoles");
        });

        builder.Entity<IdentityUserClaimLong>(b =>
        {
            b.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLoginLong>(b =>
        {
            b.ToTable("UserLogins");
        });

        builder.Entity<IdentityUserTokenLong>(b =>
        {
            b.ToTable("UserTokens");
        });

        builder.Entity<IdentityRoleClaimLong>(b =>
        {
            b.ToTable("RoleClaims");
        });
    }
}
