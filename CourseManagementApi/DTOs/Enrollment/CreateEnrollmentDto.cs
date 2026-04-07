using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Enrollment;

public class CreateEnrollmentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid CourseId { get; set; }
}
