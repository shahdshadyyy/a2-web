namespace CourseManagementApi.DTOs.Instructor;

public class InstructorProfileDto
{
    public Guid Id { get; set; }
    public string? Bio { get; set; }
    public string? OfficeLocation { get; set; }
    public string? PhoneNumber { get; set; }
}
