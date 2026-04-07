using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CourseManagementApi.Enums;
using CourseManagementApi.Services.Interfaces;

namespace CourseManagementApi.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return sub != null && Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var r = Principal?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(r, out var role) ? role : null;
        }
    }

    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsInstructor => Role == UserRole.Instructor;
    public bool IsStudent => Role == UserRole.Student;
}
