using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Enrollment;

public class UpdateEnrollmentDto
{
    [Range(0, 100)]
    public decimal? Grade { get; set; }
}
