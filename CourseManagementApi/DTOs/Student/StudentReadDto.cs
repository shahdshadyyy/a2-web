namespace CourseManagementApi.DTOs.Student;

public class StudentReadDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UniversityId { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}
