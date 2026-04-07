using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Instructor;

public class UpdateInstructorDto
{
    [MinLength(2)]
    [MaxLength(200)]
    public string? FullName { get; set; }

    [MinLength(2)]
    [MaxLength(150)]
    public string? Department { get; set; }

    public DateTime? HireDate { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? OfficeLocation { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }
}
