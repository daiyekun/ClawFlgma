using ClawFlgma.UserService.Data;
using ClawFlgma.UserService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClawFlgma.UserService.Services;

/// <summary>
/// 用户管理服务 - 负责用户信息管理、角色管理、统计等
/// </summary>
public class UserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<UserRole> _roleManager;
    private readonly UserDbContext _dbContext;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager,
        UserDbContext dbContext,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    #region 用户查询

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    public async Task<UserProfile?> GetUserByIdAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null ? await MapToProfileAsync(user) : null;
    }

    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    public async Task<UserProfile?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null ? await MapToProfileAsync(user) : null;
    }

    /// <summary>
    /// 获取用户列表（分页）
    /// </summary>
    public async Task<(List<UserProfile> Users, int TotalCount)> GetUsersPagedAsync(
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
                (u.FirstName != null && u.FirstName.Contains(searchKeyword)) ||
                (u.LastName != null && u.LastName.Contains(searchKeyword)));
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

        var profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            profiles.Add(await MapToProfileAsync(user));
        }

        return (profiles, totalCount);
    }

    /// <summary>
    /// 获取活跃用户列表
    /// </summary>
    public async Task<List<UserProfile>> GetActiveUsersAsync(int limit = 100)
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive && u.EmailVerified)
            .OrderByDescending(u => u.LastLoginAt)
            .Take(limit)
            .ToListAsync();

        var profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            profiles.Add(await MapToProfileAsync(user));
        }

        return profiles;
    }

    /// <summary>
    /// 获取最近登录的用户
    /// </summary>
    public async Task<List<UserProfile>> GetRecentlyLoggedInUsersAsync(int days = 7, int limit = 50)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var users = await _userManager.Users
            .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt >= since)
            .OrderByDescending(u => u.LastLoginAt)
            .Take(limit)
            .ToListAsync();

        var profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            profiles.Add(await MapToProfileAsync(user));
        }

        return profiles;
    }

    #endregion

    #region 用户管理

    /// <summary>
    /// 更新用户信息
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateUserAsync(
        long userId,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        string? bio = null,
        string? profilePictureUrl = null,
        string? timeZone = null,
        string? website = null,
        string? location = null)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return (false, "User not found");
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
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {UserId} updated successfully", userId);
        return (true, "User updated successfully");
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteUserAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return (false, "User not found");
        }

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
    /// 激活/停用用户
    /// </summary>
    public async Task<(bool Success, string Message)> SetUserActiveAsync(long userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
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

    /// <summary>
    /// 重置用户密码
    /// </summary>
    public async Task<(bool Success, string Message)> ResetPasswordAsync(long userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
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

        _logger.LogWarning("Password reset for user {UserId}", userId);
        return (true, "Password reset successfully");
    }

    #endregion

    #region 角色管理

    /// <summary>
    /// 获取用户角色
    /// </summary>
    public async Task<IList<string>> GetUserRolesAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
    }

    /// <summary>
    /// 为用户添加角色
    /// </summary>
    public async Task<(bool Success, string Message)> AddRoleToUserAsync(long userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
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
    public async Task<(bool Success, string Message)> RemoveRoleFromUserAsync(long userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
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
    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        return new UserStatistics
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            EmailVerifiedUsers = users.Count(u => u.EmailVerified),
            EmailUnverifiedUsers = users.Count(u => !u.EmailVerified),
            UsersWithLastLogin7Days = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt >= DateTime.UtcNow.AddDays(-7)),
            UsersWithLastLogin30Days = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt >= DateTime.UtcNow.AddDays(-30)),
            NewUsersThisWeek = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
            NewUsersThisMonth = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
        };
    }

    #endregion

    #region 验证

    /// <summary>
    /// 验证邮箱是否已存在
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    #endregion

    #region 辅助方法

    private async Task<UserProfile> MapToProfileAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfile
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            TimeZone = user.TimeZone,
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            EmailVerifiedAt = user.EmailVerifiedAt,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    #endregion
}
