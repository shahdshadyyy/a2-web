using CourseManagementApi.DTOs.User;

namespace CourseManagementApi.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
