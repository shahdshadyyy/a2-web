using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Student;

public class CreateStudentDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string UniversityId { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Level { get; set; } = string.Empty;
}
