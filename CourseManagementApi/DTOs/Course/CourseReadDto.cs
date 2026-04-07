namespace CourseManagementApi.DTOs.Course;

public class CourseReadDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
}
