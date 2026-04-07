namespace CourseManagementApi.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public Guid InstructorId { get; set; }

    public Instructor Instructor { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
