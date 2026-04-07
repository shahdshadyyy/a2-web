namespace CourseManagementApi.Entities;

public class InstructorProfile
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public string? Bio { get; set; }
    public string? OfficeLocation { get; set; }
    public string? PhoneNumber { get; set; }

    public Instructor Instructor { get; set; } = null!;
}
