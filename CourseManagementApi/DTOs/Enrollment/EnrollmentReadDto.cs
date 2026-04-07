namespace CourseManagementApi.DTOs.Enrollment;

public class EnrollmentReadDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public decimal? Grade { get; set; }
}
