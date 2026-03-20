using ClawFlgma.Shared;
using ClawFlgma.AuthService.DTOs;
using ClawFlgma.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClawFlgma.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AccountService accountService,
        ILogger<AuthController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        var (success, result, message) = await _accountService.RegisterAsync(dto);
        if (!success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(message));
        }

        return Ok(ApiResponse<AuthResponseDto>.Success(result, message));
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var (success, result, message) = await _accountService.LoginAsync(dto);
        if (!success)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Unauthorized(message));
        }

        _logger.LogInformation("User {Email} logged in successfully", dto.Email);
        return Ok(ApiResponse<AuthResponseDto>.Success(result, message));
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var (success, result, message) = await _accountService.RefreshTokenAsync(dto.RefreshToken);
        if (!success)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Unauthorized(message));
        }

        return Ok(ApiResponse<AuthResponseDto>.Success(result, message));
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutRequestDto dto)
    {
        var (success, message) = await _accountService.LogoutAsync(dto.RefreshToken);
        if (success)
        {
            return Ok(ApiResponse.Success(message));
        }
        return Ok(ApiResponse.Success("登出成功"));
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var (success, message) = await _accountService.ChangePasswordAsync(dto);
        if (!success)
        {
            return BadRequest(ApiResponse.Fail(message));
        }

        return Ok(ApiResponse.Success(message));
    }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

