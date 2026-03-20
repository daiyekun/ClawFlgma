using ClawFlgma.AuthService.Data;
using ClawFlgma.AuthService.Models;
using Microsoft.AspNetCore.Identity;

namespace ClawFlgma.AuthService.Services;

public class SeedDataService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<SeedDataService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task EnsureSeedDataAsync()
    {
        await EnsureRolesExistAsync();
        await EnsureAdminUserExistsAsync();
    }

    private async Task EnsureRolesExistAsync()
    {
        var roles = new[]
        {
            new ApplicationRole { Name = "User", Description = "Standard user", Priority = 1 },
            new ApplicationRole { Name = "PremiumUser", Description = "Premium user with extended features", Priority = 2 },
            new ApplicationRole { Name = "Moderator", Description = "Content moderator", Priority = 3 },
            new ApplicationRole { Name = "Admin", Description = "Administrator with full access", Priority = 10 }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
            {
                await _roleManager.CreateAsync(role);
                _logger.LogInformation("Created role: {RoleName}", role.Name);
            }
        }
    }

    private async Task EnsureAdminUserExistsAsync()
    {
        var adminEmail = "admin@clawflgma.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailVerified = true,
                IsActive = true
            };

            var password = "Admin@123456";
            var result = await _userManager.CreateAsync(adminUser, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
                _logger.LogWarning("Default admin password: {Password}. Please change it!", password);
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
