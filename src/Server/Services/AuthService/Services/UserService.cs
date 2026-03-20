using ClawFlgma.AuthService.Data;
using ClawFlgma.AuthService.Models;
using ClawFlgma.AuthService.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClawFlgma.AuthService.Services;

/// <summary>
/// 用户服务 - 提供用户管理、查询、统计等功能
/// </summary>
public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    #region 用户查询

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    public async Task<ApplicationUser?> GetUserByNameAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    /// <summary>
    /// 获取用户列表（分页）
    /// </summary>
    public async Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchKeyword = null,
        bool? isActive = null)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchKeyword))
        {
            query = query.Where(u =>
                u.Email!.Contains(searchKeyword) ||
                u.FirstName!.Contains(searchKeyword) ||
                u.LastName!.Contains(searchKeyword));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    /// <summary>
    /// 获取活跃用户列表
    /// </summary>
    public async Task<List<ApplicationUser>> GetActiveUsersAsync(int limit = 100)
    {
        return await _userManager.Users
            .Where(u => u.IsActive && u.EmailVerified)
            .OrderByDescending(u => u.LastLoginAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 获取最近登录的用户
    /// </summary>
    public async Task<List<ApplicationUser>> GetRecentlyLoggedInUsersAsync(int days = 7, int limit = 50)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await _userManager.Users
            .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt >= since)
            .OrderByDescending(u => u.LastLoginAt)
            .Take(limit)
            .ToListAsync();
    }

    #endregion

    #region 用户管理

    /// <summary>
    /// 创建用户
    /// </summary>
    public async Task<(bool Success, ApplicationUser? User, string Message)> CreateUserAsync(
        string email,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        List<string>? roles = null)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return (false, null, "Email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName ?? string.Empty,
            LastName = lastName ?? string.Empty,
            PhoneNumber = phoneNumber,
            EmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // 分配默认角色
        if (roles == null || roles.Count == 0)
        {
            await _userManager.AddToRoleAsync(user, "User");
        }
        else
        {
            foreach (var role in roles)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        _logger.LogInformation("User {Email} created successfully", email);
        return (true, user, "User created successfully");
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    public async Task<(bool Success, ApplicationUser? User, string Message)> UpdateUserAsync(
        string userId,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        string? bio = null,
        string? profilePictureUrl = null,
        string? timeZone = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, null, "User not found");
        }

        if (firstName != null)
            user.FirstName = firstName;
        if (lastName != null)
            user.LastName = lastName;
        if (phoneNumber != null)
            user.PhoneNumber = phoneNumber;
        if (bio != null)
            user.Bio = bio;
        if (profilePictureUrl != null)
            user.ProfilePictureUrl = profilePictureUrl;
        if (timeZone != null)
            user.TimeZone = timeZone;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {UserId} updated successfully", userId);
        return (true, user, "User updated successfully");
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        // 软删除：设置为不活跃
        user.IsActive = false;
        user.Email = $"{user.Email}_deleted_{Guid.NewGuid():N}";
        user.UserName = $"{user.UserName}_deleted_{Guid.NewGuid():N}";
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {UserId} soft deleted successfully", userId);
        return (true, "User deleted successfully");
    }

    /// <summary>
    /// 永久删除用户
    /// </summary>
    public async Task<(bool Success, string Message)> PermanentlyDeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogWarning("User {UserId} permanently deleted", userId);
        return (true, "User permanently deleted");
    }

    /// <summary>
    /// 激活/停用用户
    /// </summary>
    public async Task<(bool Success, string Message)> SetUserActiveAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {UserId} set to {IsActive}", userId, isActive ? "active" : "inactive");
        return (true, $"User {(isActive ? "activated" : "deactivated")} successfully");
    }

    #endregion

    #region 角色管理

    /// <summary>
    /// 获取用户角色
    /// </summary>
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
    }

    /// <summary>
    /// 为用户添加角色
    /// </summary>
    public async Task<(bool Success, string Message)> AddRoleToUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return (false, "Role does not exist");
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return (false, "User already has this role");
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("Role {Role} added to user {UserId}", roleName, userId);
        return (true, "Role added successfully");
    }

    /// <summary>
    /// 从用户移除角色
    /// </summary>
    public async Task<(bool Success, string Message)> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            return (false, "User does not have this role");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("Role {Role} removed from user {UserId}", roleName, userId);
        return (true, "Role removed successfully");
    }

    #endregion

    #region 用户统计

    /// <summary>
    /// 获取用户统计数据
    /// </summary>
    public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        return new Dictionary<string, int>
        {
            { "TotalUsers", users.Count },
            { "ActiveUsers", users.Count(u => u.IsActive) },
            { "InactiveUsers", users.Count(u => !u.IsActive) },
            { "EmailVerifiedUsers", users.Count(u => u.EmailVerified) },
            { "EmailUnverifiedUsers", users.Count(u => !u.EmailVerified) },
            { "UsersWithLastLogin7Days", users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt >= DateTime.UtcNow.AddDays(-7)) },
            { "UsersWithLastLogin30Days", users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt >= DateTime.UtcNow.AddDays(-30)) },
            { "NewUsersThisWeek", users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)) },
            { "NewUsersThisMonth", users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)) }
        };
    }

    /// <summary>
    /// 获取用户注册趋势
    /// </summary>
    public async Task<Dictionary<DateTime, int>> GetUserRegistrationTrendAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var users = await _userManager.Users
            .Where(u => u.CreatedAt >= startDate)
            .GroupBy(u => new { u.CreatedAt.Date })
            .Select(g => new { Date = g.Key.Date, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        var result = new Dictionary<DateTime, int>();
        for (var i = 0; i <= days; i++)
        {
            var date = DateTime.UtcNow.AddDays(-i).Date;
            result[date] = 0;
        }

        foreach (var item in users)
        {
            if (result.ContainsKey(item.Date))
            {
                result[item.Date] = item.Count;
            }
        }

        return result;
    }

    #endregion

    #region 验证和安全

    /// <summary>
    /// 验证邮箱是否已存在
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    /// <summary>
    /// 验证用户名是否已存在
    /// </summary>
    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName) != null;
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    public async Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("Password reset for user {UserId}", userId);
        return (true, "Password reset successfully");
    }

    /// <summary>
    /// 验证密码强度
    /// </summary>
    public (bool IsValid, string Message) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least one digit");

        return (true, "Password is valid");
    }

    #endregion

    #region 搜索和筛选

    /// <summary>
    /// 高级搜索用户
    /// </summary>
    public async Task<List<ApplicationUser>> SearchUsersAdvancedAsync(
        string? email = null,
        string? name = null,
        string? phoneNumber = null,
        bool? emailVerified = null,
        bool? isActive = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int limit = 100)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(email))
            query = query.Where(u => u.Email!.Contains(email));

        if (!string.IsNullOrEmpty(name))
            query = query.Where(u => u.FirstName!.Contains(name) || u.LastName!.Contains(name));

        if (!string.IsNullOrEmpty(phoneNumber))
            query = query.Where(u => u.PhoneNumber == phoneNumber);

        if (emailVerified.HasValue)
            query = query.Where(u => u.EmailVerified == emailVerified.Value);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (createdAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= createdAfter.Value);

        if (createdBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= createdBefore.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    #endregion
}
