using ClawFlgma.AuthService.Data;
using ClawFlgma.AuthService.DTOs;
using ClawFlgma.AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClawFlgma.AuthService.Services;

/// <summary>
/// 账户服务 - 负责认证相关操作（注册、登录、Token管理）
/// 注意：用户信息管理应调用独立的 UserService
/// </summary>
public class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    public async Task<(bool success, AuthResponseDto? result, string message)> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return (false, null, "Email already registered");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            ProfilePictureUrl = dto.ProfilePictureUrl,
            TimeZone = dto.TimeZone,
            EmailVerified = false,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // 分配默认角色
        await _userManager.AddToRoleAsync(user, "User");

        // 生成 Token
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        var userDto = await MapToUserDtoAsync(user);

        _logger.LogInformation("User {Email} registered successfully", dto.Email);

        var response = new AuthResponseDto
        {
            User = userDto,
            Token = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                TokenType = "Bearer",
                ExpiresIn = 3600
            }
        };
        return (true, response, "Registration successful");
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<(bool success, AuthResponseDto? result, string message)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return (false, null, "Invalid email or password");
        }

        if (!user.IsActive)
        {
            return (false, null, "Account is disabled");
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.Email!,
            dto.Password,
            dto.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            string message = result.IsLockedOut
                ? $"Account locked out. Try again after {user.LockoutEnd}"
                : result.IsNotAllowed
                ? "Account not allowed to sign in"
                : "Invalid email or password";

            return (false, null, message);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // 生成 Token
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        var userDto = await MapToUserDtoAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", dto.Email);

        var response = new AuthResponseDto
        {
            User = userDto,
            Token = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                IdToken = accessToken
            }
        };
        return (true, response, "Login successful");
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    public async Task<(bool success, AuthResponseDto? result, string message)> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || !tokenEntity.IsActive)
        {
            return (false, null, "Invalid or expired refresh token");
        }

        var user = tokenEntity.User;
        if (!user.IsActive)
        {
            return (false, null, "Account is disabled");
        }

        // 撤销旧的 Refresh Token
        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedBy = "TokenRefresh";
        await _dbContext.SaveChangesAsync();

        // 生成新的 Token
        var accessToken = await GenerateAccessTokenAsync(user);
        var newRefreshToken = await CreateRefreshTokenAsync(user);

        var userDto = await MapToUserDtoAsync(user);

        var response = new AuthResponseDto
        {
            User = userDto,
            Token = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                TokenType = "Bearer",
                ExpiresIn = 3600
            }
        };
        return (true, response, "Token refreshed successfully");
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    public async Task<(bool success, string message)> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return (false, "Refresh token is required");
        }

        var tokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedBy = "Logout";
            await _dbContext.SaveChangesAsync();
        }

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User logged out successfully");

        return (true, "Logout successful");
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    public async Task<(bool success, string message)> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return (false, "User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, "Password changed successfully");
    }

    #region Helper Methods

    /// <summary>
    /// 获取用户（仅用于认证流程）
    /// </summary>
    public async Task<ApplicationUser?> GetUserByIdAsync(long userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    /// <summary>
    /// 根据邮箱获取用户（仅用于认证流程）
    /// </summary>
    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    /// <summary>
    /// 检查邮箱是否存在
    /// </summary>
    public async Task<bool> UserExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    /// <summary>
    /// 创建 Refresh Token
    /// </summary>
    public async Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// 生成 JWT Access Token
    /// </summary>
    private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty),
            new("fullName", user.FullName),
            new("emailVerified", user.EmailVerified.ToString().ToLower()),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)));

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes("ClawFlgma-Super-Secret-JWT-Key-2026-Change-In-Production"));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "ClawFlgma",
            audience: "ClawFlgma.Users",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 清理过期的 Token
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.IsExpired || rt.IsRevoked)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _dbContext.RefreshTokens.RemoveRange(expiredTokens);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired tokens", expiredTokens.Count);
        }
    }

    private async Task<UserDto> MapToUserDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            TimeZone = user.TimeZone,
            Bio = user.Bio,
            Roles = roles.ToList(),
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion
}
