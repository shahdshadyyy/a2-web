using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Instructor;

public class CreateInstructorDto
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
    [MinLength(2)]
    [MaxLength(150)]
    public string Department { get; set; } = string.Empty;

    [Required]
    public DateTime HireDate { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? OfficeLocation { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }
}
