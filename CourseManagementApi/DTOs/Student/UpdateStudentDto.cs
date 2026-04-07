using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Student;

public class UpdateStudentDto
{
    [MinLength(2)]
    [MaxLength(200)]
    public string? FullName { get; set; }

    [MinLength(3)]
    [MaxLength(50)]
    public string? UniversityId { get; set; }

    [MinLength(2)]
    [MaxLength(50)]
    public string? Level { get; set; }
}
