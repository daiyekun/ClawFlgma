using ClawFlgma.UserService.Models;
using ClawFlgma.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClawFlgma.UserService.Data;

/// <summary>
/// 用户服务数据库上下文
/// </summary>
public class UserDbContext : IdentityDbContextLong
{
    private readonly SnowflakeIdGenerator _idGenerator = new SnowflakeIdGenerator(1, 2);

    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user && user.Id == 0)
            {
                user.Id = _idGenerator.NextId();
            }
            if (entry.Entity is UserRole role && role.Id == 0)
            {
                role.Id = _idGenerator.NextId();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Gender).HasMaxLength(20);
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(200);
        });
    }
}

