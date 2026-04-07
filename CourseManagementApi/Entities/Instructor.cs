namespace CourseManagementApi.Entities;

public class Instructor
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Department { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }

    public User User { get; set; } = null!;
    public InstructorProfile? InstructorProfile { get; set; }
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
