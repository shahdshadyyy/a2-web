using CourseManagementApi.DTOs.Enrollment;

namespace CourseManagementApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EnrollmentReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EnrollmentReadDto> CreateAsync(CreateEnrollmentDto dto, CancellationToken cancellationToken = default);
    Task<EnrollmentReadDto?> UpdateAsync(Guid id, UpdateEnrollmentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
