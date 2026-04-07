using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
