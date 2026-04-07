using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Course;

public class CreateCourseDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 12)]
    public int CreditHours { get; set; }

    [Required]
    public Guid InstructorId { get; set; }
}
