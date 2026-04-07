using CourseManagementApi.DTOs.User;

namespace CourseManagementApi.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserReadDto User { get; set; } = null!;
}
