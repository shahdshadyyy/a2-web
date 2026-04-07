using CourseManagementApi.DTOs.Auth;

namespace CourseManagementApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
}
