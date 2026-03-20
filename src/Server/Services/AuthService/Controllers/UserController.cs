using ClawFlgma.AuthService.DTOs;
using ClawFlgma.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClawFlgma.AuthService.Controllers;

/// <summary>
/// 用户控制器 - AuthService 中的 UserController 只处理当前用户的基本信息
/// 完整的用户管理功能由独立的 UserService 提供
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly ILogger<UserController> _logger;

    public UserController(AccountService accountService, ILogger<UserController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(new UserDto());
        }

        var userId = long.Parse(userIdClaim);
        var user = await _accountService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new UserDto());
        }

        return Ok(new UserDto
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
            Roles = new List<string>(), // 角色信息从 UserService 获取
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<string>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var (success, message) = await _accountService.ChangePasswordAsync(dto);
        if (!success)
        {
            return BadRequest(message);
        }

        return Ok(message);
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    [Authorize]
    [HttpGet("verify")]
    public ActionResult<UserDto> VerifyToken()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(new UserDto());
        }

        return Ok(new UserDto
        {
            Id = long.Parse(userIdClaim),
            Email = email ?? string.Empty
        });
    }
}

