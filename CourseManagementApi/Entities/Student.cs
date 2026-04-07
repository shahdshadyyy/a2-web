namespace CourseManagementApi.Entities;

public class Student
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UniversityId { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
