using CourseManagementApi.DTOs.Course;
using CourseManagementApi.DTOs.Enrollment;

namespace CourseManagementApi.Services.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyList<CourseReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CourseReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CourseReadDto> CreateAsync(CreateCourseDto dto, CancellationToken cancellationToken = default);
    Task<CourseReadDto?> UpdateAsync(Guid id, UpdateCourseDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentReadDto>> GetStudentsAsync(Guid courseId, CancellationToken cancellationToken = default);
}
