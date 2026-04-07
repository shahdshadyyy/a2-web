using CourseManagementApi.DTOs.Course;
using CourseManagementApi.DTOs.Instructor;

namespace CourseManagementApi.Services.Interfaces;

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InstructorReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InstructorReadDto> CreateAsync(CreateInstructorDto dto, CancellationToken cancellationToken = default);
    Task<InstructorReadDto?> UpdateAsync(Guid id, UpdateInstructorDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseReadDto>> GetCoursesAsync(Guid instructorId, CancellationToken cancellationToken = default);
}
