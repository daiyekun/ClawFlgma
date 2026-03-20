using ClawFlgma.AuthService.Models;
using ClawFlgma.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClawFlgma.AuthService.Data;

/// <summary>
/// 应用数据库上下文 - 配置雪花算法ID
/// </summary>
public class ApplicationDbContext : IdentityDbContextLong
{
    private readonly SnowflakeIdGenerator _idGenerator = new SnowflakeIdGenerator(1, 1);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        RefreshTokens = null!;
    }

    public required DbSet<RefreshToken> RefreshTokens { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity is ApplicationUser user && user.Id == 0)
            {
                user.Id = _idGenerator.NextId();
            }
            if (entry.Entity is ApplicationRole role && role.Id == 0)
            {
                role.Id = _idGenerator.NextId();
            }
            if (entry.Entity is RefreshToken token && token.Id == 0)
            {
                token.Id = _idGenerator.NextId();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Gender).HasMaxLength(20);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(200);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
