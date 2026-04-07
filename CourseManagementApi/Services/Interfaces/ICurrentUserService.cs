using CourseManagementApi.Enums;

namespace CourseManagementApi.Services.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsInstructor { get; }
    bool IsStudent { get; }
}
