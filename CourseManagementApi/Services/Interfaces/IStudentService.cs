using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.DTOs.Student;

namespace CourseManagementApi.Services.Interfaces;

public interface IStudentService
{
    Task<IReadOnlyList<StudentReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StudentReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentReadDto> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default);
    Task<StudentReadDto?> UpdateAsync(Guid id, UpdateStudentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentReadDto>> GetEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken = default);
}
