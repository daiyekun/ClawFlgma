using ClawFlgma.Shared;
using ClawFlgma.UserService.Models;
using ClawFlgma.UserService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClawFlgma.UserService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManagementService _userManagementService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManagementService userManagementService, ILogger<UsersController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfile>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized(ApiResponse<UserProfile>.Unauthorized("Invalid token"));
        }

        var profile = await _userManagementService.GetUserByIdAsync(userId);
        if (profile == null)
        {
            return NotFound(ApiResponse<UserProfile>.NotFound("User not found"));
        }

        return Ok(ApiResponse<UserProfile>.Success(profile, "获取用户信息成功"));
    }

    /// <summary>
    /// 更新当前用户信息
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse>> UpdateCurrentUser([FromBody] UpdateUserProfileDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized(ApiResponse.Unauthorized("Invalid token"));
        }

        var (success, message) = await _userManagementService.UpdateUserAsync(
            userId,
            dto.FirstName,
            dto.LastName,
            dto.PhoneNumber,
            dto.Bio,
            dto.ProfilePictureUrl,
            dto.TimeZone,
            dto.Website,
            dto.Location
        );

        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<UserProfile>>> GetUserById(long userId)
    {
        var profile = await _userManagementService.GetUserByIdAsync(userId);
        if (profile == null)
        {
            return NotFound(ApiResponse<UserProfile>.NotFound("User not found"));
        }

        return Ok(ApiResponse<UserProfile>.Success(profile, "获取用户信息成功"));
    }

    /// <summary>
    /// 获取用户列表（管理员）
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<UserListDataDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchKeyword = null,
        [FromQuery] bool? isActive = null)
    {
        var (users, totalCount) = await _userManagementService.GetUsersPagedAsync(
            page, pageSize, searchKeyword, isActive);

        var data = new UserListDataDto
        {
            Users = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(ApiResponse<UserListDataDto>.Success(data, "获取用户列表成功"));
    }

    /// <summary>
    /// 获取活跃用户列表
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<List<UserProfile>>>> GetActiveUsers([FromQuery] int limit = 100)
    {
        var users = await _userManagementService.GetActiveUsersAsync(limit);
        return Ok(ApiResponse<List<UserProfile>>.Success(users, "获取活跃用户成功"));
    }

    /// <summary>
    /// 获取最近登录的用户
    /// </summary>
    [HttpGet("recent")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<List<UserProfile>>>> GetRecentlyLoggedInUsers(
        [FromQuery] int days = 7,
        [FromQuery] int limit = 50)
    {
        var users = await _userManagementService.GetRecentlyLoggedInUsersAsync(days, limit);
        return Ok(ApiResponse<List<UserProfile>>.Success(users, "获取最近登录用户成功"));
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(long userId)
    {
        var (success, message) = await _userManagementService.DeleteUserAsync(userId);
        if (!success)
        {
            return NotFound(ApiResponse.Fail(message, 404));
        }

        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 激活/停用用户
    /// </summary>
    [HttpPatch("{userId}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse>> SetUserStatus(long userId, [FromBody] SetUserStatusDto dto)
    {
        var (success, message) = await _userManagementService.SetUserActiveAsync(userId, dto.IsActive);
        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 获取用户角色
    /// </summary>
    [HttpGet("{userId}/roles")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<IList<string>>>> GetUserRoles(long userId)
    {
        var roles = await _userManagementService.GetUserRolesAsync(userId);
        return Ok(ApiResponse<IList<string>>.Success(roles, "获取用户角色成功"));
    }

    /// <summary>
    /// 为用户添加角色
    /// </summary>
    [HttpPost("{userId}/roles")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse>> AddRole(long userId, [FromBody] AddRoleDto dto)
    {
        var (success, message) = await _userManagementService.AddRoleToUserAsync(userId, dto.RoleName);
        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 从用户移除角色
    /// </summary>
    [HttpDelete("{userId}/roles/{roleName}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse>> RemoveRole(long userId, string roleName)
    {
        var (success, message) = await _userManagementService.RemoveRoleFromUserAsync(userId, roleName);
        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 获取用户统计信息
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<UserStatistics>>> GetStatistics()
    {
        var stats = await _userManagementService.GetUserStatisticsAsync();
        return Ok(ApiResponse<UserStatistics>.Success(stats, "获取统计数据成功"));
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [HttpPost("{userId}/reset-password")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse>> ResetPassword(long userId, [FromBody] ResetPasswordDto dto)
    {
        var (success, message) = await _userManagementService.ResetPasswordAsync(userId, dto.NewPassword);
        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        _logger.LogWarning("Admin reset password for user {UserId}", userId);
        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// 验证邮箱是否存在
    /// </summary>
    [HttpGet("check-email")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists([FromQuery] string email)
    {
        var exists = await _userManagementService.EmailExistsAsync(email);
        return Ok(ApiResponse<bool>.Success(exists, "检查邮箱成功"));
    }
}

#region DTOs

public class UpdateUserProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? TimeZone { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
}

public class UserListDataDto
{
    public List<UserProfile> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class SetUserStatusDto
{
    public bool IsActive { get; set; }
}

public class AddRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

#endregion
