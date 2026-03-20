using ClawFlgma.Shared;

namespace ClawFlgma.AuthService.Models;

/// <summary>
/// 应用角色模型
/// </summary>
public class ApplicationRole : IdentityRoleLong
{
    public string? Description { get; set; }
    public int Priority { get; set; }
}
