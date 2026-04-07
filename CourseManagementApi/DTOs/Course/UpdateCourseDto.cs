using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Course;

public class UpdateCourseDto
{
    [MinLength(3)]
    [MaxLength(200)]
    public string? Title { get; set; }

    [MinLength(2)]
    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Range(1, 12)]
    public int? CreditHours { get; set; }

    public Guid? InstructorId { get; set; }
}
