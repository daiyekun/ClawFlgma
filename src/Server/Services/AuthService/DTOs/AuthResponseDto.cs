namespace ClawFlgma.AuthService.DTOs;

public class AuthResponseDto
{
    public UserDto? User { get; set; }
    public TokenResponseDto? Token { get; set; }
}

public class UserDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? TimeZone { get; set; }
    public string? Bio { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string IdToken { get; set; } = string.Empty;
}
